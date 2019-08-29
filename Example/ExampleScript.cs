using BestHTTP.Logger;
using UnityEngine;

namespace CharismaSDK.Example
{
    public class ExampleScript : MonoBehaviour
    {
        [Header("Assign charisma settings object here")]
        [SerializeField] private CharismaSettings _settings;
        [SerializeField] private Charisma _charisma;
        [SerializeField] private bool _useAudio;
        [SerializeField] private AudioSource _audioSource;

        private CharismaSettings _charismaSettings;
        
        private void Start()
        {

            // Get token
            _charisma.GetToken();
            _charisma.OnTokenReceived += token =>
            {
                // Use token to add conversation
                _charisma.Initialise(token, Loglevels.Error);
                _charisma.AddConversation(token);    
            };

            _charisma.OnConversationInitialised += id =>
            {
                // Once a conversation has been added. Connect to Charisma
                _charisma.Connect();
            };

            _charisma.OnConnected += () =>
            {
                // Once we are connected. Start the interaction
                _charisma.StartInteraction(1, _useAudio); 
                
            };

            _charisma.OnReceivedResponse += (response, responseAudio, url) =>
            {
                // We have received a response 
                Debug.Log($"Charisma: {response.Message.Text}");

                if (!_useAudio) return;
                
                // If we have defined audio, play it now
                _audioSource.clip = responseAudio;
                _audioSource.Play();
            };
        }
    }
}
