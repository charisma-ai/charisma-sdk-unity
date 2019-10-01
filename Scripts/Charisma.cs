using System;
using System.Text;
using BestHTTP;
using BestHTTP.Logger;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

#pragma warning disable 618

namespace CharismaSDK
{
    /// <summary>
    /// Interact with Charisma using this object.
    /// </summary>
    public class Charisma
    {
        #region Static Variables

        private const string BaseUrl = "https://api.charisma.ai/";

        #endregion

        #region Static Methods

        /// <summary>
        /// Generate a new play-through.
        /// </summary>
        /// <param name="tokenSetting">Settings object for this play-through</param>
        /// <param name="callback">Called when a valid token has been generated</param>
        public static void CreatePlayThroughToken(CharismaTokenSetting tokenSetting, Action<string> callback)
        {
            var requestString = tokenSetting.StoryVersion != 0
                ? new JObject
                    {{"storyId", tokenSetting.StoryId}, {"storyVersion", tokenSetting.StoryVersion}}.ToString()
                : new JObject {{"storyId", tokenSetting.StoryId}}.ToString();

            
            var request = new HTTPRequest(new Uri($"{BaseUrl}play/token/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" +response.Message);
                        return;
                    }
                    
                    var data = Encoding.UTF8.GetString(response.Data);
                    var token = CharismaUtilities.TokenToString(data);
                                          
                    callback?.Invoke(token);
                    CharismaLogger.Log("HTTP: Token request complete");
                }))
            {
                RawData = Encoding.UTF8.GetBytes(requestString)
            };
            
            // Only pass the user token if we are debugging
            if (tokenSetting.StoryVersion == -1 && !string.IsNullOrEmpty(tokenSetting.DraftToken))
            {
                request.SetHeader("Authorization", $"Bearer {tokenSetting.DraftToken}");
                CharismaLogger.Log("HTTP: Requesting token with draft");
            }
            else
            {
                CharismaLogger.Log("HTTP: Requesting token with published");
            }
                
			
            request.AddHeader("Content-Type", "application/json");
            request.UseAlternateSSL = true;
            request.Send();
        }

        /// <summary>
        /// Initialise a new conversation in a play-through.
        /// </summary>
        /// <param name="token">Valid play-through token.</param>
        /// <param name="callback">Called when a conversation has been generated.</param>
        public static void CreateConversation(string token, Action<int> callback)
        {
            var request = new HTTPRequest(new Uri($"{BaseUrl}play/conversation/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" + originalRequest.Exception.Message);
                        return;
                    } 
                   
                    var data = Encoding.UTF8.GetString(response.Data);
                    var conversation = CharismaUtilities.GenerateConversation(data);

                    callback?.Invoke(conversation.ConversationId);
                    
                    CharismaLogger.Log("HTTP: Conversation request complete");
                }))
            {
                RawData = Encoding.UTF8.GetBytes(token)
            };

            request.SetHeader("Authorization", $"Bearer {token}");
            request.UseAlternateSSL = true;
            request.Send();
            
            CharismaLogger.Log("HTTP: Requesting conversation");
        }

        /// <summary>
        /// Set the mood of a Character in your story.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where a characters mood should be set.</param>
        /// <param name="characterName">The name of the character who's mood should be set.</param>
        /// <param name="mood">The mood to update to.</param>
        public static void SetMood(string token, string characterName, Mood mood)
        {
            if (mood == null || characterName == null)
            {
                Debug.LogError("Charisma: You need to provide both a character name and a character mood modifier.");
                return;
            }
            
            var newMood = new MoodSetter(characterName, mood);
            Debug.Log(CharismaUtilities.ToJson(newMood));
            
            var request = new HTTPRequest(
                new Uri($"{BaseUrl}play/set-mood/"), HTTPMethods.Post, (
                    (originalRequest, response) =>
                    {
                        if (!response.IsSuccess)
                        {
                            Debug.LogError("Error:" +response.Message);
                            return;
                        }
                        
                        CharismaLogger.Log($"Charisma: updated mood of '{characterName}'");
                    }))
            {
                
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(newMood))
                
            };
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.UseAlternateSSL = true;
            request.Send();
        }
        
        /// <summary>
        /// Set the mood of a Character in your story.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where a characters mood should be set.</param>
        /// <param name="characterName">The name of the character who's mood should be set.</param>
        /// <param name="mood">The mood to update to.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static void SetMood(string token, string characterName, Mood mood, Action callback)
        {
            
            if (mood == null || characterName == null)
            {
                Debug.LogError("Charisma: You need to provide both a character name and a character mood modifier.");
                return;
            }
            
            var newMood = new MoodSetter(characterName, mood);
            
            var request = new HTTPRequest(
                new Uri($"{BaseUrl}play/set-mood/"), HTTPMethods.Post, (
                    (originalRequest, response) =>
                    {
                        if (!response.IsSuccess)
                        {
                            Debug.LogError("Error:" +response.Message);
                            return;
                        }
                        
                        CharismaLogger.Log($"Charisma: updated mood of '{characterName}'");
                        callback?.Invoke();
                    }))
            {
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(newMood))
            };
           
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.UseAlternateSSL = true;
            request.Send();
        }

        /// <summary>
        /// Set a memory in your story
        /// </summary>
        /// <param name="token">Provide the token of the play-through where the memory should be changed.</param>
        /// <param name="recallValue">The recall value of the memory.</param>
        /// <param name="saveValue">The new value of the memory.</param>
        public static void SetMemory(string token, string recallValue, string saveValue)
        {
            var memory = new Memory(recallValue, saveValue);
            var request = new HTTPRequest(
                new Uri($"{BaseUrl}play/set-memory/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" +response.Message);
                        return;
                    }
                    
                    CharismaLogger.Log($"Charisma: Set memory: '{memory.MemoryRecallValue}' with value '{memory.SaveValue}'");
                }))
            {
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(memory))
            };
           
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.UseAlternateSSL = true;
            request.Send();
        }
        
        /// <summary>
        /// Set a memory in your story
        /// </summary>
        /// <param name="token">Provide the token of the play-through where the memory should be changed.</param>
        /// <param name="recallValue">The recall value of the memory.</param>
        /// <param name="saveValue">The new value of the memory.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static void SetMemory(string token, string recallValue, string saveValue, Action callback)
        {
            var memory = new Memory(recallValue, saveValue);
            
            var request = new HTTPRequest(
                new Uri($"{BaseUrl}play/set-memory/"), HTTPMethods.Post, (
                (originalRequest, response) =>
                {
                    if (!response.IsSuccess)
                    {
                        Debug.LogError("Error:" +response.Message);
                        return;
                    }
                    
                    CharismaLogger.Log($"Charisma: Set memory: '{memory.MemoryRecallValue}' with value '{memory.SaveValue}'");
                    callback?.Invoke();
                }))
            {
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(memory))
            };
           
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.UseAlternateSSL = true;
            request.Send();
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Returns true if the socket is open
        /// </summary>
        public bool IsConnected => _socket != null && _socket.IsOpen;

        #endregion

        #region Degelates

        public delegate void MessageDelegate(int conversationId, Response response);
        public delegate void TypingDelegate(int conversationId);

        #endregion

        #region Events

        /// <summary>
        /// Called when a new message has been generated by Charisma.
        /// </summary>
        public event MessageDelegate OnMessage;
        
        /// <summary>
        /// Called when a character has started typing.
        /// </summary>
        public event TypingDelegate OnStartTyping;
        
        /// <summary>
        /// Called when a character has stopped typing.
        /// </summary>
        public event TypingDelegate OnStopTyping;
        
        #endregion

        #region MemberVariables

        private string _token;
        private SocketManager _socketManager;
        private Socket _socket;
        private bool _isProcessing;
        private SpeechOptions _speechOptions;
        private CoroutineConsumer _coroutineConsumer;

        #endregion

        #region Constructors

        /// <summary>
        /// Interaction with Charisma
        /// </summary>
        /// <param name="token">A valid play-though token.</param>
        public Charisma(string token)
        {
            _coroutineConsumer = GameObject.Instantiate(Resources.Load<CoroutineConsumer>
                ("Prefabs/CoroutineConsumer"));   
            
            _token = token;
        }

        /// <summary>
        /// Interaction with Charisma
        /// </summary>
        /// <param name="token">A valid play-though token.</param>
        /// <param name="loglevels">Log levels for advanced debugging.</param>
        public Charisma(string token, Loglevels loglevels)
        {
            _coroutineConsumer = GameObject.Instantiate(Resources.Load<CoroutineConsumer>
                ("Prefabs/CoroutineConsumer"));  
            
            _token = token;
            HTTPManager.Logger.Level = loglevels;
        }

        ~Charisma()
        {
            GameObject.Destroy(_coroutineConsumer.gameObject);
            
            Disconnect();
        }

        #endregion

        #region Connect / Disconnect
        
        /// <summary>
        /// Connect to Charisma
        /// </summary>
        /// <param name="onConnectCallback">Called when successfully connected to Charisma</param>
        public void Connect(Action onConnectCallback)
        {
             if(IsConnected) return;
             
            var options = new SocketOptions
            {
                ConnectWith = TransportTypes.WebSocket,
                AdditionalQueryParams = new ObservableDictionary<string, string> {{"token", _token}}
            };

            var manager = new SocketManager(new Uri($"{BaseUrl}socket.io/"), options)
            {
                Encoder = new LitJsonEncoder()
            };
            
            _socket = manager.GetSocket("/play");
            
            _socket.On(SocketIOEventTypes.Connect, (socket, packet, args) =>
            {
                CharismaLogger.Log("Socket: Connected");
                
                _socketManager = manager;
            });
			
            _socket.On("error", (socket, packet, args) => {		
				
                Debug.LogError(args[0].ToString());								
            });		
			
            _socket.On("status", (socket, packet, args) => {
                
                    CharismaLogger.Log("Socket: Status ready");		
                    
                    onConnectCallback?.Invoke();
            });
			
            _socket.On("message", async (socket, packet, args) =>
            {
                var response = await CharismaUtilities.GenerateResponse(packet.Payload);
                
                OnMessage?.Invoke(response.ConversationId, response);
                 
                // We are done processing this message
                _isProcessing = false;           
                
                CharismaLogger.Log($"Charisma: Received message: {response.Message.Text}");                           
            });
			
            _socket.On("start-typing", (socket, packet, args) =>
            {
                _isProcessing = true;

                OnStartTyping?.Invoke(CharismaUtilities.GenerateConversation(packet.Payload).ConversationId);
                
                CharismaLogger.Log("Charisma: Start typing");
            });
			
            _socket.On("stop-typing", (socket, packet, args) =>
            {
                OnStopTyping?.Invoke(CharismaUtilities.GenerateConversation(packet.Payload).ConversationId);
                
                CharismaLogger.Log("Charisma: Stop typing");
            });
			
            _socket.On("problem", (socket, packet, args) =>
            {
                CharismaLogger.Log(packet.Payload);
            });
        }

        // Disconnect from the current interaction.
        public void Disconnect()
        {
            if (_socket == null) return;
            if (_socketManager == null) return;            
            
            try
            {
                _socket.Disconnect();
                _socket = null;
                _socketManager.Close();
                _socketManager = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Charisma: failed to disconnect: {e}");
                return;
            }
            
            CharismaLogger.Log("Charisma: Successfully disconnected");
        }
        
        #endregion
        
        #region Interaction

        /// <summary>
        /// Start the story from selected scene.
        /// </summary>
        /// <param name="sceneIndex">The scene to start from.</param>
        /// <param name="speechOptions">Speech settings for the interaction.</param>
        /// <param name="conversationId">Id of the conversation we want to start.</param>
        public void Start(int sceneIndex, int conversationId, SpeechOptions speechOptions)
        {
            // Initialise speech options
            _speechOptions = speechOptions;
            
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before starting the interaction");
                return;
            };

            var startOptions = new StartOptions(conversationId, sceneIndex, speechOptions);			
            _socket?.Emit("start", startOptions);	

            
            CharismaLogger.Log("Charisma: Starting interaction");
        }
        
        /// <summary>
        /// Start the story from selected scene, without speech. In order to use speech, pass a Speech Options object.
        /// </summary>
        /// <param name="sceneIndex">The scene to start from.</param>
        /// <param name="conversationId">Id of the conversation to start.</param>
        public void Start(int sceneIndex, int conversationId)
        {
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before starting the interaction");
                return;
            };

            var startOptions = new StartOptions(conversationId, sceneIndex);			
            _socket?.Emit("start", startOptions);

            CharismaLogger.Log("Charisma: Starting interaction");
        }

        /// <summary>
        /// Send a tap event to Charisma.
        /// </summary>
        /// <param name="conversationId">Id of the conversation the tap should be sent to.</param>
        public void Tap(int conversationId)
        {
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before sending commands");
                return;
            };

            var tapOptions = _speechOptions != null ? new Tap(conversationId, _speechOptions) : new Tap(conversationId);
            _socket?.Emit("tap", tapOptions);
            
            CharismaLogger.Log("Charisma: Tap");
        }

        /// <summary>
        /// Send a tap event to Charisma. 
        /// </summary>
        /// <param name="conversationId">Id of the conversation the tap should be sent to.</param>
        /// <param name="speechOptions">Change the speech option of the interaction.</param>
        public void Tap(int conversationId, SpeechOptions speechOptions)
        {
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before sending commands");
                return;
            };

            // Set new speech options
            _speechOptions = speechOptions;
            
            var tapOptions = _speechOptions != null ? new Tap(conversationId, _speechOptions) : new Tap(conversationId);
            _socket?.Emit("tap", tapOptions);
            
            CharismaLogger.Log("Charisma: Tap");
        }
        
        /// <summary>
        /// Send player response to Charisma.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="conversationId">Conversation to interact with.</param>
        public void Reply(string message, int conversationId)
        {
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before sending player response");
                return;
            };

            if (_isProcessing)
            {
                Debug.LogWarning("Charisma: Cannot send player response when Charisma is processing");
                return;
            }

            var playerMessage = _speechOptions != null
                ? new Reply(message, conversationId, _speechOptions)
                : new Reply(message, conversationId);
            
            _socket?.Emit("reply", playerMessage);
            
            CharismaLogger.Log("Charisma: Sending player response");  
        }

        /// <summary>
        /// Send player response to Charisma.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="conversationId">Conversation to interact with.</param>
        /// <param name="speechOptions">Change the speech option of the interaction.</param>
        public void Reply(string message, int conversationId, SpeechOptions speechOptions)
        {
            if (_socket == null)
            {
                Debug.LogError("Charisma: Socket not open. Connect before sending player response");
                return;
            };

            if (_isProcessing)
            {
                Debug.LogWarning("Charisma: Cannot send player response when Charisma is processing");
                return;
            }
            
            // Set new speech options
            _speechOptions = speechOptions;

            var playerMessage = _speechOptions != null
                ? new Reply(message, conversationId, _speechOptions)
                : new Reply(message, conversationId);
            
            _socket?.Emit("reply", playerMessage);
            
            CharismaLogger.Log("Charisma: Sending player response");  
        }

        #endregion
    }

    public class CharismaTokenSetting
    {
        public int StoryId { get; set; }
        public int StoryVersion { get; set; }
        public string DraftToken { get; set; }
        
        /// <summary>
        /// Charisma will generate a play-through token based on this setting.
        /// </summary>
        /// <param name="storyId">Id of the story we want to interact with.</param>
        /// <param name="storyVersion">Version of the story we want to interact with.
        ///  - In order to play the draft version of the story, set this to -1. A Draft Token also has to be supplied.
        ///  - In order to play the latest published version of this story, this to 0.</param>
        /// <param name="draftToken">Token from the Charisma website.</param>
        public CharismaTokenSetting(int storyId, int storyVersion, string draftToken)
        {
            StoryId = storyId;
            StoryVersion = storyVersion;
            DraftToken = draftToken;
        }
        
        /// <summary>
        /// Charisma will generate a play-through token based on this setting.
        /// </summary>
        /// <param name="storyId">Id of the story we want to interact with.</param>
        /// <param name="storyVersion">Version of the story we want to interact with.
        ///  - In order to play the draft version of the story, set this to -1. A Draft Token also has to be supplied.
        ///  - In order to play the latest published version of this story, this to 0.</param>
        public CharismaTokenSetting(int storyId, int storyVersion)
        {
            StoryId = storyId;
            StoryVersion = storyVersion;
            DraftToken = null;
        }
    }
    
    public class TokenResponseParams
    {
        /// <summary>
        /// Out generated token.
        /// </summary>
        public string Token { get; }

        [JsonConstructor]
        public TokenResponseParams(string token)
        {
            Token = token;
        }
    }
    
    public class Conversation
    {
        /// <summary>
        /// The id of the conversation we have just initialized.
        /// </summary>
        public int ConversationId { get; }
        
        [JsonConstructor]
        public Conversation(int conversationId)
        {
            this.ConversationId = conversationId;
        }
    }
    
    public class Reply
    {
        public int conversationId { get; }
        public string text { get; }
        public SpeechOptions speechConfig { get; }
        
        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="text">Message to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.
        ///  - Don't pass unless you want to change settings.'</param>
        /// <param name="conversationId">Id of the conversation to send the reply to.</param>
        public Reply(string text, int conversationId, SpeechOptions speechConfig)
        {
            this.text = text;
            this.speechConfig = speechConfig;
            this.conversationId = conversationId;
        }
        
        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="text">Message to send</param>
        /// <param name="conversationId">Id of the conversation to send the reply to.</param>
        public Reply(string text, int conversationId)
        {
            this.text = text;
            this.speechConfig = null;
            this.conversationId = conversationId;
        }	
    }
    
    public class Tap
    {
        public int conversationId;
        public SpeechOptions speechConfig;
        
        public Tap(int conversationId, SpeechOptions speechConfig)
        {
            this.conversationId = conversationId;
            this.speechConfig = speechConfig;
        }

        public Tap(int conversationId)
        {
            this.conversationId = conversationId;
            this.speechConfig = null;
        }
    }
    
    public class StartOptions
    {
        /// <summary>
        /// The options with which to start the interaction with Charisma.
        /// </summary>
        /// <param name="conversationId">Id of the conversation to start.</param>
        /// <param name="sceneIndex">Index of the scene to start.</param>
        /// <param name="speechConfig">To use speech, pass speech options.</param>
        public StartOptions(int conversationId, int sceneIndex, SpeechOptions speechConfig)
        {
            this.conversationId = conversationId;
            this.sceneIndex = sceneIndex;
            this.speechConfig = speechConfig;			
        }
        
        
        /// <summary>
        /// The options with which to start the interaction with Charisma. To use speech, pass speech options.
        /// </summary>
        /// <param name="conversationId">Id of the conversation to start.</param>
        /// <param name="sceneIndex">Index of the scene to start.</param>
        public StartOptions(int conversationId, int sceneIndex)
        {
            this.conversationId = conversationId;
            this.sceneIndex = sceneIndex;
            this.speechConfig = null;			
        }

        public int conversationId { get; set; }
        public int sceneIndex { get; }
        public SpeechOptions speechConfig { get; }
    }
}


