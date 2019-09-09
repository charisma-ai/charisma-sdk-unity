using System.Collections.Generic;
using BestHTTP.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.Example
{
    [RequireComponent(typeof(Charisma))]
    public class ExampleScript : MonoBehaviour
    {
        [Header("Assign charisma settings object here")]
        [SerializeField] private CharismaSettings _settings;
        [SerializeField] private InputField _inputField;
        [SerializeField] private Button _button;
        [SerializeField] private Text _outputField;
        [SerializeField] private bool _useAudio;
        [SerializeField] private AudioSource _audioSource;
        
        [Header("For advanced debugging")]
        [SerializeField] private Loglevels _logLevels;

        // Reference to the charisma component on this Game Object.
        private Charisma _charisma;       
        
        // List of all current conversation.
        private Dictionary<string, Conversation> _conversations;
        
        #region Monobehaviour Callbacks
        
        private void Start()
        {
            _charisma = GetComponent<Charisma>();
            _conversations = new Dictionary<string, Conversation>();
            
            BindEvents();
            
            // Entry point.
            StartConversation();
            
            _inputField.ActivateInputField();
        }

        private void Update()
        {            
            if (!Input.GetKeyDown(KeyCode.Return) || _inputField.text == string.Empty) return;
            
            SendPlayerMessage(_inputField.text);
            _inputField.text = string.Empty;
        }

        #endregion
        
        #region Events

        // Bind all events relevant to communicating with Charisma.
        private void BindEvents()
        {
            _charisma.OnTokenReceived += InitializeCharisma;
            _charisma.OnConversationInitialised += Connect;
            _charisma.OnConnected += StartInteraction;
            _charisma.OnReceivedResponse += HandleResponse;
            
            _button.onClick.AddListener((() =>
            {
                SendPlayerMessage(_inputField.text);
                _inputField.text = string.Empty;
                
            }));
        }
        
        private void OnDisable()
        {
            UnbindEvents();
            StopConversation();
        }

        private void OnDestroy()
        {
            UnbindEvents();
            StopConversation();
        }       

        private void UnbindEvents()
        {
            _charisma.OnTokenReceived -= InitializeCharisma;
            _charisma.OnConversationInitialised -= Connect;
            _charisma.OnConnected -= StartInteraction;
            _charisma.OnReceivedResponse -= HandleResponse;
        }
        
        #endregion

        #region Initialisation

        private void StartConversation()
        {
            // Get token.
            _charisma.GetToken();
        }

        private void InitializeCharisma(string token)
        {
            // Initialise our Charisma object.
            _charisma.Initialise(token, _logLevels);
            
            // Use token to add conversation.
            _charisma.AddConversation(token);    
        }

        private void Connect(int id)
        {
            // Save conversation   
            _conversations.Add("MyFirstConversation", new Conversation(id));
            
            // Once a conversation has been added. Connect to Charisma.
            _charisma.Connect();
        }

        private void StartInteraction()
        {
            // Once we are connected. Start the interaction.
            _charisma.StartInteraction(1, _useAudio);       
        }

        #endregion

        #region Communication

        private void HandleResponse(Response response, AudioClip responseAudio, string url)
        {
            // We have received a response.             
            Debug.Log($"Charisma: {response.Message.Text}");                      
            _outputField.text = $"{response.Message.Character.Name}: {response.Message.Text}";

            if (!_useAudio) return;
                
            // If we have defined audio, play it now.
            _audioSource.clip = responseAudio;
            _audioSource.Play();
            
            if (!response.EndStory) return;
            
            // If the node is marked as "End Story", disconnect.
            StopConversation();   
        }
        
        // Send a player message to charisma. Include our audio preferences.
        private void SendPlayerMessage(string message)
        {
            _charisma.SendPlayerMessage(message, _useAudio);
        }

        #endregion

        #region Termination

        private void StopConversation()
        {
            _charisma.Disconnect();               
        }

        #endregion

    }
}
