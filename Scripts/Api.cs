using System;
using System.Collections;
using System.Text;
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

            var request = UnityWebRequest.Put($"{BaseUrl}/play/token/", SerializeBody(requestParams));
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Content-Type", "application/json");

            // Only pass the API key if we are debugging
            if (tokenParams.StoryVersion == -1 && !string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                request.SetRequestHeader("Authorization", $"API-Key {tokenParams.ApiKey}");
                CharismaLogger.Log("Using API key to generate playthrough");
            }

            // If the API key is null or nonexistent, throw error
            if (tokenParams.StoryVersion == -1 && string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                Debug.LogError("Please provide an API key in order to play the draft version");
                yield break;
            }

            if (tokenParams.StoryVersion == 0)
            {
                CharismaLogger.Log("Generating playthrough token with latest published version");
            }

            if (tokenParams.StoryVersion != 0 && tokenParams.StoryVersion != -1)
            {
                CharismaLogger.Log($"Generating playthrough token with version {tokenParams.StoryVersion} of the story");
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);

            try
            {
                var deserialized = JsonConvert.DeserializeObject<CreatePlaythroughTokenResponse>(data);
                callback?.Invoke(deserialized);
                CharismaLogger.Log("Token request complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not deserialize token. Are you using the correct API key?: {e}");
                throw;
            }
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

            CharismaLogger.Log("Requesting conversation...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);

            try
            {
                var deserialized = JsonConvert.DeserializeObject<CreateConversationResponse>(data);
                callback?.Invoke(deserialized.ConversationUuid);
                CharismaLogger.Log("Conversation request complete");
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not generate conversation; {e}");
                throw;
            }
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
            var memory = new SetMemoryParams(recallValue, saveValue);

            var request = UnityWebRequest.Get($"{BaseUrl}/play/set-memory");
            request.method = "POST"; // hack to send POST to server instead of PUT
            request.SetRequestHeader("Authorization", $"Bearer {token}");

            CharismaLogger.Log("Setting memory...");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error:" + request.error);
                yield break;
            }

            var data = Encoding.UTF8.GetString(request.downloadHandler.data);

            CharismaLogger.Log($"Set memory - '{memory.memoryRecallValue}' with value '{memory.saveValue}'");
            callback?.Invoke();
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

    public class SetMemoryParams
    {
        public string memoryRecallValue;
        public string saveValue;

        public SetMemoryParams(string recallValue, string saveValue)
        {
            this.memoryRecallValue = recallValue;
            this.saveValue = saveValue;
        }
    }
}


