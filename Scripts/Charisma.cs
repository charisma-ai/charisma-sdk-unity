using System;
using System.Text;
using BestHTTP;
using BestHTTP.Logger;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using Newtonsoft.Json;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

namespace CharismaSdk
{
    /// <summary>
    /// Interact with Charisma using this object.
    /// </summary>
    public class Charisma
    {
        #region Static Variables

        private const string BaseUrl = "https://api.charisma.ai";

        #endregion

        #region Static Methods

        public static void Setup()
        {
            try
            {
                // Move HTTP updates to different thread
                if(!HTTPUpdateDelegator.IsThreaded)
                    HTTPUpdateDelegator.IsThreaded = true;
                
                // Create the coroutine consumer
                GameObject.Instantiate(Resources.Load<MainThreadConsumer>
                    ("Prefabs/MainThreadConsumer"));   
                
                CharismaLogger.Log("Set up complete!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: Failed to set up properly! {e}" );
                
                throw;
            }
        }

        /// <summary>
        /// Generate a new play-through.
        /// </summary>
        /// <param name="tokenParams">Settings object for this play-through</param>
        /// <param name="callback">Called when a valid token has been generated</param>
        public static void CreatePlaythroughToken(GetPlaythroughTokenParams tokenParams, Action<string> callback)
        {
            var requestParams = tokenParams.StoryVersion != 0
                ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new {storyId = tokenParams.StoryId, version = tokenParams.StoryVersion}))
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new {storyId = tokenParams.StoryId}));
            
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
                        var token = JsonConvert.DeserializeObject<TokenResponseParams>(data).Token;
                        callback?.Invoke(token);
                        CharismaLogger.Log("Token request complete");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Could not deserialize token. Is your debug token up to date?: {e}");
                        throw;
                    }
                    
                }))
            {
                RawData = requestParams
            };
            
            // Only pass the user token if we are debugging
            if (tokenParams.StoryVersion == -1 && !string.IsNullOrEmpty(tokenParams.DraftToken))
            {
                request.SetHeader("Authorization", $"Bearer {tokenParams.DraftToken}");
                CharismaLogger.Log("Using user draft token to generate playthrough");
            }
            
            // If the draft token is null or nonexistent, throw error
            if (tokenParams.StoryVersion == -1 && string.IsNullOrEmpty(tokenParams.DraftToken))
            {
                Debug.LogError("Please provide a draft token in order to play the draft version");
                return;
            }
            
            if(tokenParams.StoryVersion == 0 )
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
                        callback?.Invoke(JsonConvert.DeserializeObject<Conversation>(data).ConversationId);
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
        /// Set the mood of a Character in your story.
        /// </summary>
        /// <param name="token">Provide the token of the play-through where a characters mood should be set.</param>
        /// <param name="characterName">The name of the character who's mood should be set.</param>
        /// <param name="mood">The mood to update to.</param>
        /// <param name="callback">Called when the mood has successfully been set.</param>
        public static void SetMood(string token, string characterName, Mood mood, Action callback = null)
        {
            
            if (mood == null || characterName == null)
            {
                Debug.LogError("Charisma: You need to provide both a character name and a character mood modifier.");
                return;
            }
            
            var newMood = new SetMoodParams(characterName, mood);
            CharismaLogger.Log($"Setting new mood for {characterName}: CharismaUtilities.ToJson(newMood)");
            
            var request = new HTTPRequest(
                new Uri($"{BaseUrl}/play/set-mood"), HTTPMethods.Post, (
                    (originalRequest, response) =>
                    {
                        if (!response.IsSuccess)
                        {
                            Debug.LogError("Error:" + originalRequest.Response.DataAsText);
                            return;
                        }
                        
                        CharismaLogger.Log($"Updated mood of '{characterName}'");

                        callback?.Invoke();
                    }))
            {
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(newMood))
            };
           
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.Send();
        }

        /// <summary>
        /// Set a memory in your story
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
                RawData = Encoding.UTF8.GetBytes(CharismaUtilities.ToJson(memory))
            };
           
            request.SetHeader("Authorization", $"Bearer {token}");			
            request.AddHeader("Content-Type", "application/json");
            request.Send();
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Returns true if the socket is open.
        /// </summary>
        public bool IsConnected => _socket != null && _socket.IsOpen;

        /// <summary>
        /// The last token that was generated.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// A successful connection to the socket has been made. Charisma is ready to start play.
        /// </summary>
        public bool IsReadyToPlay { get; set; }

        /// <summary>
        /// Assign a new Speech config.
        ///  - To add speech, pass in a new speech config.
        ///  - To remove audio, set this to null.
        /// </summary>
        public SpeechOptions SpeechOptions
        {
            get { return _speechOptions; }
            set { _speechOptions = value; } 
        }

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
        
        private SocketManager _socketManager;
        private Socket _socket;
        private SpeechOptions _speechOptions;
        private MainThreadConsumer _mainThreadConsumer;

        #endregion

        #region Constructors

        /// <summary>
        /// Interaction with Charisma
        /// </summary>
        /// <param name="token">A valid play-though token.</param>
        public Charisma(string token)
        {
            Token = token;
        }

        /// <summary>
        /// Interaction with Charisma
        /// </summary>
        /// <param name="token">A valid play-though token.</param>
        /// <param name="loglevel">Log levels for advanced debugging.</param>
        public Charisma(string token, Loglevels loglevel)
        {
            Token = token;
            HTTPManager.Logger.Level = loglevel;
        }

        ~Charisma()
        {
            Disconnect();
        }

        #endregion

        #region Connect / Disconnect
        
        /// <summary>
        /// Connect to Charisma
        /// </summary>
        /// <param name="onReadyCallback">Called when successfully connected to Charisma.</param>
        public void Connect(Action onReadyCallback)
        {
             if(IsConnected) return;
             
            var options = new SocketOptions
            {
                ConnectWith = TransportTypes.WebSocket,
                AdditionalQueryParams = new ObservableDictionary<string, string> {{"token", Token}}
            };

            _socketManager = new SocketManager(new Uri($"{BaseUrl}/socket.io/"), options)
            {
                Encoder = new LitJsonEncoder()
            };
            
            _socket = _socketManager.GetSocket("/play");
            
            _socket.On(SocketIOEventTypes.Connect, (socket, packet, args) =>
            {
                CharismaLogger.Log("Connected to socket");
            });
			
            _socket.On("error", (socket, packet, args) => {		
				
                Debug.LogError(args[0].ToString());								
            });		
			
            _socket.On("status", (socket, packet, args) => {

                if ((string) args[0] == "ready")
                {
                    CharismaLogger.Log("Ready to begin play");
                    IsReadyToPlay = true;
                    
                    onReadyCallback?.Invoke();
                }
                else
                {
                    Debug.LogError("Charisma: Failed to set up websocket connection to server");
                }
            });
			
            _socket.On("message", (socket, packet, args) =>
            {
                MainThreadConsumer.Instance.Enqueue((async () =>
                {
                    var response = await CharismaUtilities.GenerateResponse(packet.Payload);

                    OnMessage?.Invoke(response.ConversationId, response);

                    CharismaLogger.Log($"Received message");
                }));
            });
			
            _socket.On("start-typing", (socket, packet, args) =>
            {

                OnStartTyping?.Invoke(JsonConvert.DeserializeObject<Conversation>(packet.Payload).ConversationId);
                
                CharismaLogger.Log("Start typing");
            });
			
            _socket.On("stop-typing", (socket, packet, args) =>
            {
                OnStopTyping?.Invoke(JsonConvert.DeserializeObject<Conversation>(packet.Payload).ConversationId);
                
                CharismaLogger.Log("Stop typing");
            });
			
            _socket.On("problem", (socket, packet, args) =>
            {
                CharismaLogger.Log(JsonConvert.DeserializeObject<CharismaError>(packet.Payload).Error);
            });
        }

        // Disconnect from the current interaction.
        public void Disconnect()
        {
            if(!_socket.IsOpen) return;

            try
            {
                _socket.Disconnect();
                _socket = null;
                _socketManager.Close();
                _socketManager = null;

                IsReadyToPlay = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Charisma: failed to disconnect: {e}");
                return;
            }
            
            CharismaLogger.Log("Successfully disconnected");
        }
        
        #endregion
        
        #region Interaction

        /// <summary>
        /// Start the story from selected scene.
        /// </summary>
        /// <param name="sceneIndex">The scene to start from.</param>
        /// <param name="speechOptions">Speech settings for the interaction.</param>
        /// <param name="conversationId">Id of the conversation we want to start.</param>
        public void Start(int conversationId, int sceneIndex, SpeechOptions speechOptions = null)
        {
            if (!IsReadyToPlay)
            {
                Debug.LogError("Charisma: Socket not open. Connect before starting the interaction");
                return;
            };
            
            // Initialise speech options
            _speechOptions = speechOptions;

            var startOptions = new StartOptions(conversationId, sceneIndex, speechOptions);			
            _socket?.Emit("start", startOptions);	

            
            CharismaLogger.Log("Starting interaction");
        }

        /// <summary>
        /// Start the story from selected scene.
        /// </summary>
        /// <param name="conversationId">Id of the conversation we want to resume.</param>
        /// <param name="speechOptions">Speech settings for the interaction.</param>
        public void Resume(int conversationId , SpeechOptions speechOptions = null)
        {
            if (!IsReadyToPlay)
            {
                Debug.LogError("Charisma: Socket not open. Connect before resuming the interaction");
                return;
            };
            
            // Initialise speech options
            _speechOptions = speechOptions;

            var resumeOptions = new ResumeOptions(conversationId, _speechOptions);			
            _socket?.Emit("resume", resumeOptions);

            CharismaLogger.Log("Resuming interaction");
        }

        /// <summary>
        /// Send a tap event to Charisma.
        /// </summary>
        /// <param name="conversationId">Id of the conversation the tap should be sent to.</param>
        public void Tap(int conversationId)
        {
            if (!IsReadyToPlay)
            {
                Debug.LogError("Charisma: Socket not open. Connect before starting the interaction");
                return;
            };

            var tapOptions = new Tap(conversationId, _speechOptions);
            _socket?.Emit("tap", tapOptions);
            
            CharismaLogger.Log("Tap");
        }
        
        
        /// <summary>
        /// Send player response to Charisma.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="conversationId">Conversation to interact with.</param>
        public void Reply(int conversationId, string message)
        {
            if (!IsReadyToPlay)
            {
                Debug.LogError("Charisma: Socket not open. Connect before starting the interaction");
                return;
            };

            var playerMessage = new Reply(message, conversationId, _speechOptions);
            _socket?.Emit("reply", playerMessage);
            
            CharismaLogger.Log($"Sending player response '{message}' to conversation {conversationId}");  
        }

        #endregion
    }

    public class GetPlaythroughTokenParams
    {
        public int StoryId { get; }
        public int StoryVersion { get; }
        public string DraftToken { get; }
        
        /// <summary>
        /// Charisma will generate a play-through token based on this setting.
        /// </summary>
        /// <param name="storyId">Id of the story we want to interact with.</param>
        /// <param name="storyVersion">Version of the story we want to interact with.
        ///  - In order to play the draft version of the story, set this to -1. A Draft Token also has to be supplied.
        ///  - In order to play the latest published version of this story, this to 0.</param>
        /// <param name="draftToken">Token from the Charisma website.</param>
        public GetPlaythroughTokenParams(int storyId, int storyVersion, string draftToken = null)
        {
            StoryId = storyId;
            StoryVersion = storyVersion;
            DraftToken = draftToken;
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
        public int conversationId;
        public string text;
        public SpeechOptions speechConfig;
        
        /// <summary>
        /// Player response to Charisma.
        /// </summary>
        /// <param name="text">Message to send</param>
        /// <param name="speechConfig">Changes the speech settings of the interaction.
        ///  - Don't pass unless you want to change settings.'</param>
        /// <param name="conversationId">Id of the conversation to send the reply to.</param>
        public Reply(string text, int conversationId, SpeechOptions speechConfig = null)
        {
            this.text = text;
            this.speechConfig = speechConfig;
            this.conversationId = conversationId;
        }
    }
    
    public class Tap
    {
        public int conversationId;
        public SpeechOptions speechConfig;
        
        public Tap(int conversationId, SpeechOptions speechConfig = null)
        {
            this.conversationId = conversationId;
            this.speechConfig = speechConfig;
        }
    }
    
    public class StartOptions
    {
        /// <summary>
        /// Options with which to start the interaction with Charisma.
        /// </summary>
        /// <param name="conversationId">Id of the conversation to start.</param>
        /// <param name="sceneIndex">Index of the scene to start.</param>
        /// <param name="speechConfig">To use speech, pass speech options.</param>
        public StartOptions(int conversationId, int sceneIndex, SpeechOptions speechConfig = null)
        {
            this.conversationId = conversationId;
            this.sceneIndex = sceneIndex;
            this.speechConfig = speechConfig;			
        }

        public int conversationId { get; set; }
        public int sceneIndex { get; }
        public SpeechOptions speechConfig { get; }
    }
    
    public class ResumeOptions
    {
        /// <summary>
        /// Options with which to resume a playthrough in Charisma.
        /// </summary>
        /// <param name="conversationId">Id of the conversation to resume.</param>
        /// <param name="speechConfig">To use speech, pass speech options.</param>
        public ResumeOptions(int conversationId, SpeechOptions speechConfig = null)
        {
            this.conversationId = conversationId;
            this.speechConfig = speechConfig;
        }

        public int conversationId { get; set; }
        public SpeechOptions speechConfig { get; }
    }

    public class CharismaError
    {
        public string Error { get; }

        [JsonConstructor]
        public CharismaError(string error)
        {
            Error = error;
        }
    }
}


