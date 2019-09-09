using UnityEngine;

namespace CharismaSDK
{
    [CreateAssetMenu(fileName = "CharismaSettings", menuName = "Charisma/Charisma Settings")]
    public class CharismaSettings : ScriptableSingleton<CharismaSettings>
    {
        [Header("Server Url")]
        [SerializeField] private string _baseUrl = "https://api.charisma.ai/";

        public const string TokenUrl = "play/token/";
        public const string ServerUrl = "socket.io/";
        public const string ConversationUrl = "play/conversation/";
        public const string SetMoodUrl = "play/set-mood/";
        public const string SetMemoryUrl = "play/set-memory/";
        public const string GetPlaythroughInfoUrl = "play/playthrough-info/";
        
        [Header("StoryId")] 
        [SerializeField] private int _storyId;
        [Header("Story version")]
        [SerializeField] private int _storyVersion;
        [Header("Audio Config")]
        [SerializeField] private AudioConfig _audioConfig;
        
        [Space(10)]
        [Header("Debugging('Story version' has to be -1)")]
        [SerializeField] private bool _isDebugging;
        [Tooltip("This can be retrieved from the inspector in your browser")] 
        [Header("Debug Token")] 
        [SerializeField] private string _debugToken;


        public string BaseUrl => _baseUrl;
        public int StoryId => _storyId;
        public int StoryVersion => _storyVersion;
        public string DebugToken => _debugToken;
        public AudioConfig AudioConfig => _audioConfig;
        public bool IsDebugging => _isDebugging;    
    }
}
