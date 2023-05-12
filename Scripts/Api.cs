using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CharismaSDK.Events;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace CharismaSDK
{
    /// <summary>
    /// Interact with Charisma using this object.
    /// </summary>
    public class CharismaAPI : MonoBehaviour
    {
        #region Static Variables

        private const string BaseUrl = "https://play.charisma.ai";

        #endregion

        #region Static Methods

        private static byte[] SerializeBody(object value)
        {
            string json = JsonConvert.SerializeObject(value);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// used for creating query strings for appending onto HTTP Get functions
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns>created query string</returns>
        private static string CreateQueryString(Dictionary<string, string> dictionary)
        {
            string result = "?";

            // TODO: will need to add handling for illegal HTTP characters found in the key and value bodies
            // to prevent sending bad requests to the server
            foreach(var entry in dictionary)
            {
                result += entry.Key;
                result += "=";
                result += entry.Value;
                result += "&";
            }

            return result;
        }

        /// <summary>
        /// Creates a new playthrough token.
        /// </summary>
        /// <param name="tokenParams">Settings object for this playthrough</param>
        /// <param name="callback">Called when a valid token has been generated</param>
        public static IEnumerator CreatePlaythroughToken(
            CreatePlaythroughTokenParams tokenParams,
            Action<CreatePlaythroughTokenResponse> callback)
        {
            object requestParams = tokenParams.StoryVersion != 0
                ? new { storyId = tokenParams.StoryId, version = tokenParams.StoryVersion }
                : new { storyId = tokenParams.StoryId };

            var request = UnityWebRequest.Put($"{BaseUrl}/play/token", SerializeBody(requestParams));
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Content-Type", "application/json");

            // Only pass the API key if we are debugging
            if (tokenParams.StoryVersion == -1 && !string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                request.SetRequestHeader("Authorization", $"API-Key {tokenParams.ApiKey}");
                Logger.Log("Using API key to generate playthrough");
            }

            // If the API key is null or nonexistent, throw error
            if (tokenParams.StoryVersion == -1 && string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                Logger.LogError("Please provide an API key in order to play the draft version");
                yield break;
            }

            if (tokenParams.StoryVersion == 0)
            {
                Logger.Log("Generating playthrough token with latest published version");
            }

            if (tokenParams.StoryVersion != 0 && tokenParams.StoryVersion != -1)
            {
                Logger.Log($"Generating playthrough token with version {tokenParams.StoryVersion} of the story");
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);
            CreatePlaythroughTokenResponse deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject<CreatePlaythroughTokenResponse>(data);
                Logger.Log("Token request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not deserialize token. Are you using the correct API key? ", e);
            }

            callback?.Invoke(deserialized);

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Initialise a new conversation in a play-through.
        /// </summary>
        /// <param name="token">Valid play-through token.</param>
        /// <param name="callback">Called when a conversation has been generated.</param>
        public static IEnumerator CreateConversation(string token, Action<string> callback)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/play/conversation");
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            Logger.Log("Requesting conversation...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);
            CreateConversationResponse deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject<CreateConversationResponse>(data);
                Logger.Log("Conversation request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not generate conversation; ", e);
            }
            callback?.Invoke(deserialized.ConversationUuid);

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Retrives the message history of an active conversation.
        /// </summary>
        /// <param name="token">Valid play-through token.</param>
        /// <param name="conversationUuid">Active generated Conversation ID.</param>
        /// <param name="minEventId">Minimum event Id from which to start reading message history. Only assign if you want messages after a certain point</param>
        /// <param name="callback">Called when a valid history is generated.</param>
        public static IEnumerator GetMessageHistory(string token, string conversationUuid, string minEventId, Action<GetMessageHistoryResponse> callback)
        {
            Dictionary<string, string> requestParams = new Dictionary<string, string>();
            if(conversationUuid != default)
            {
                requestParams.Add("conversationUuid", conversationUuid);
            }
            if (minEventId != default)
            {
                requestParams.Add("minEventId", minEventId);
            }

            var request = UnityWebRequest.Get($"{BaseUrl}/play/message-history" + CreateQueryString(requestParams));
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            Logger.Log("Requesting message history...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);
            GetMessageHistoryResponse deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject<GetMessageHistoryResponse>(data);
                Logger.Log("Message history request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not get message history;", e);
            }
            callback?.Invoke(deserialized);

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Returns information about the playthrough, including emotions of the characters and saved memories.
        /// </summary>
        /// <param name="token">Valid play-through token.</param>
        /// <param name="callback">Called when a valid playthrough info response is generated.</param>
        public static IEnumerator GetPlaythroughInfo(string token, Action<GetPlaythroughInfoResponse> callback)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/play/playthrough-info");
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            Logger.Log("Requesting playthrough info...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);
            GetPlaythroughInfoResponse deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject<GetPlaythroughInfoResponse>(data);
                Logger.Log("Playthrough info request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not generate playthrough information.", e);
            }

            callback?.Invoke(deserialized);

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Sets a memory to a specific value in a playthrough.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where the memory should be changed.</param>
        /// <param name="recallSaveValues">A collection of recall and save values. The key of the Dict acts as the recall value and value acts as the save.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static IEnumerator SetMemory(string token, Dictionary<string, string> recallSaveValues, Action callback = null)
        {
            var count = recallSaveValues.Count;

            Logger.Log($"Setting {count} memories...");
            List<object> memoriesToSet = new List<object>();
            foreach (var entry in recallSaveValues)
            {
                Logger.Log($"Setting memory `{entry.Key}` with value `{entry.Value}`...");
                memoriesToSet.Add(new
                {
                    recallValue = entry.Key,
                    saveValue = entry.Value,
                });
            }

            object requestParams = new
            {
                memories = memoriesToSet,
            };

            var request = UnityWebRequest.Put($"{BaseUrl}/play/set-memory", SerializeBody(requestParams));
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);

                yield break;
            }

            Logger.Log($"Successfully set memories!");

            callback?.Invoke();

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Sets a memory to a specific value in a playthrough.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where the memory should be changed.</param>
        /// <param name="recallValue">The recall value of the memory.</param>
        /// <param name="saveValue">The new value of the memory.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static IEnumerator SetMemory(string token, string recallValue, string saveValue, Action callback = null)
        {
            yield return SetMemory(token,
                new Dictionary<string, string>()
                 {
                    { recallValue, saveValue }
                }, callback);
        }

        /// <summary>
        /// Forks the current playthrough from the current version into the latest published version.
        /// </summary>
        /// <param name="oldToken">Provide the token of the play-through which should be forked.</param>
        /// <param name="callback">Called when the fork has successfully been set.</param>
        public static IEnumerator ForkPlaythroughToken(string oldToken, Action<ForkPlaythroughResponse> callback)
        {
            var request = UnityWebRequest.Get($"{BaseUrl}/play/fork-playthrough");
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Authorization", $"Bearer {oldToken}");

            Logger.Log("Requesting playthrough fork...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);
            ForkPlaythroughResponse deserialized;

            try
            {
                deserialized = JsonConvert.DeserializeObject<ForkPlaythroughResponse>(data);
                Logger.Log("Fork request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not request playthrough fork", e);
            }

            callback?.Invoke(deserialized);

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        /// <summary>
        /// Resets the current playthrough instance, by pointing it to a previously registered eventId
        /// </summary>
        /// <param name="token">Provide the token of the play-through which should be reset.</param>
        /// <param name="eventId">Event Id of the message to which the playthrough should be reset to. Can be obtained from the OnMessage callback, or GetMessageHistory</param>
        /// <param name="callback">Called when the reset has been successful.</param>
        public static IEnumerator ResetPlaythrough(string token, long eventId, Action callback)
        {
            object requestParams = new
            {
                eventId = eventId.ToString(),
            };

            var request = UnityWebRequest.Put($"{BaseUrl}/play/reset-playthrough", SerializeBody(requestParams));
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Content-Type", "application/json");

            Logger.Log("Requesting playthrough reset...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);

            try
            {
                Logger.Log("Reset request complete");
            }
            catch (Exception e)
            {
                throw new Exception($"Could not reset playthrough;", e);
            }

            callback?.Invoke();

            // Need to dispose of WebRequest to prevent following error:
            // "A Native Collection has not been disposed, resulting in a memory leak. Enable Full StackTraces to get more details."
            request.Dispose();
        }

        #endregion
    }

    public class CreatePlaythroughTokenParams
    {
        public int StoryId { get; }
        public int StoryVersion { get; }
        public string ApiKey { get; }

        /// <summary>
        /// Charisma will generate a play-through token based on this setting.
        /// </summary>
        /// <param name="storyId">Id of the story we want to interact with.</param>
        /// <param name="storyVersion">Version of the story we want to interact with.
        ///  - In order to play the draft version of the story, set this to -1. A an api key also has to be supplied.
        ///  - In order to play the latest published version of this story, this to 0.</param>
        /// <param name="apiKey">Api key from the Charisma website.</param>
        public CreatePlaythroughTokenParams(int storyId, int storyVersion, string apiKey = null)
        {
            StoryId = storyId;
            StoryVersion = storyVersion;
            ApiKey = apiKey;
        }
    }

    public class CreatePlaythroughTokenResponse
    {
        /// <summary>
        /// The created playthrough token. You can save this to resume play in the same playthrough.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// The UUID of the playthrough.
        /// </summary>
        public string PlaythroughUuid { get; }

        [JsonConstructor]
        public CreatePlaythroughTokenResponse(string token, string playthroughUuid)
        {
            Token = token;
            PlaythroughUuid = playthroughUuid;
        }
    }

    public class CreateConversationResponse
    {
        /// <summary>
        /// The uuid of the conversation we have just initialized.
        /// </summary>
        public string ConversationUuid { get; }

        [JsonConstructor]
        public CreateConversationResponse(string conversationUuid)
        {
            this.ConversationUuid = conversationUuid;
        }
    }

    public class GetMessageHistoryResponse
    {
        public MessageEvent[] Messages { get; }

        [JsonConstructor]
        public GetMessageHistoryResponse(MessageEvent[] messages) 
        {
            Messages = messages;
        }
    }

    public class ForkPlaythroughResponse
    {
        /// <summary>
        /// The new forked playthrough token.
        /// </summary>
        public string NewToken { get; }

        /// <summary>
        /// The UUID of the playthrough.
        /// </summary>
        public string PlaythroughUuid { get; }

        [JsonConstructor]
        public ForkPlaythroughResponse(string token, string playthroughUuid)
        {
            NewToken = token;
            PlaythroughUuid = playthroughUuid;
        }
    }

    public class GetPlaythroughInfoResponse
    {
        public Emotion[] Emotions { get; }
        public Memory[] Memories { get; }

        [JsonConstructor]
        public GetPlaythroughInfoResponse(Emotion[] emotions, Memory[] memories)
        {
            Emotions = emotions;
            Memories = memories;
        }
    }
}
