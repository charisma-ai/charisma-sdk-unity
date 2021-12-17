using System;
using System.Text;
using BestHTTP;
using BestHTTP.Logger;
using Newtonsoft.Json;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharismaSDK
{
    /// <summary>
    /// Interact with Charisma using this object.
    /// </summary>
    public class CharismaAPI
    {
        #region Static Variables

        private const string BaseUrl = "https://api.charisma.ai";

        #endregion

        #region Static Methods

        public static void Setup()
        {
            HTTPUpdateDelegator.IsThreaded = true;
        }

        /// <summary>
        /// Generate a new play-through.
        /// </summary>
        /// <param name="tokenParams">Settings object for this play-through</param>
        /// <param name="callback">Called when a valid token has been generated</param>
        public static void CreatePlaythroughToken(CreatePlaythroughTokenParams tokenParams, Action<string> callback)
        {
            var requestParams = tokenParams.StoryVersion != 0
                ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new { storyId = tokenParams.StoryId, version = tokenParams.StoryVersion }))
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new { storyId = tokenParams.StoryId }));

            var request = new HTTPRequest(new Uri($"{BaseUrl}/play/token/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" + originalRequest.Response.DataAsText);
                        return;
                    }

                    var data = Encoding.UTF8.GetString(response.Data);

                    try
                    {
                        var deserialized = JsonConvert.DeserializeObject<CreatePlaythroughTokenResponse>(data);
                        var token = deserialized.Token;
                        callback?.Invoke(token);
                        CharismaLogger.Log("Token request complete");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Could not deserialize token. Are you using the correct API key?: {e}");
                        throw;
                    }

                }))
            {
                RawData = requestParams
            };

            // Only pass the API key if we are debugging
            if (tokenParams.StoryVersion == -1 && !string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                request.SetHeader("Authorization", $"API-Key {tokenParams.ApiKey}");
                CharismaLogger.Log("Using API key to generate playthrough");
            }

            // If the API key is null or nonexistent, throw error
            if (tokenParams.StoryVersion == -1 && string.IsNullOrEmpty(tokenParams.ApiKey))
            {
                Debug.LogError("Please provide an API key in order to play the draft version");
                return;
            }

            if (tokenParams.StoryVersion == 0)
            {
                CharismaLogger.Log("Generating playthrough token with latest published version");
            }

            if (tokenParams.StoryVersion != 0 && tokenParams.StoryVersion != -1)
            {
                CharismaLogger.Log($"Generating playthrough token with version {tokenParams.StoryVersion} of the story");
            }

            request.AddHeader("Content-Type", "application/json");
            request.Send();
        }

        /// <summary>
        /// Initialise a new conversation in a play-through.
        /// </summary>
        /// <param name="token">Valid play-through token.</param>
        /// <param name="callback">Called when a conversation has been generated.</param>
        public static void CreateConversation(string token, Action<int> callback)
        {
            var request = new HTTPRequest(new Uri($"{BaseUrl}/play/conversation/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" + originalRequest.Response.DataAsText);
                        return;
                    }

                    var data = Encoding.UTF8.GetString(response.Data);

                    try
                    {
                        var deserialized = JsonConvert.DeserializeObject<CreateConversationResponse>(data);
                        callback?.Invoke(deserialized.ConversationId);
                        CharismaLogger.Log("Conversation request complete");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Could not generate conversation; {e}");
                        throw;
                    }

                }))
            {
                RawData = Encoding.UTF8.GetBytes(token)
            };

            request.SetHeader("Authorization", $"Bearer {token}");
            request.Send();

            CharismaLogger.Log("Requesting conversation");
        }

        /// <summary>
        /// Sets a memory to a specific value in a playthrough.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where the memory should be changed.</param>
        /// <param name="recallValue">The recall value of the memory.</param>
        /// <param name="saveValue">The new value of the memory.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static void SetMemory(string token, string recallValue, string saveValue, Action callback = null)
        {
            var memory = new SetMemoryParams(recallValue, saveValue);

            var request = new HTTPRequest(
                new Uri($"{BaseUrl}/play/set-memory/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" + originalRequest.Response.DataAsText);
                        return;
                    }

                    CharismaLogger.Log($"Set memory - '{memory.memoryRecallValue}' with value '{memory.saveValue}'");
                    callback?.Invoke();
                }))
            {
                RawData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(memory))
            };

            request.SetHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
            request.Send();
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
        /// Out generated token.
        /// </summary>
        public string Token { get; }

        [JsonConstructor]
        public CreatePlaythroughTokenResponse(string token)
        {
            Token = token;
        }
    }

    public class CreateConversationResponse
    {
        /// <summary>
        /// The id of the conversation we have just initialized.
        /// </summary>
        public int ConversationId { get; }

        [JsonConstructor]
        public CreateConversationResponse(int conversationId)
        {
            this.ConversationId = conversationId;
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


