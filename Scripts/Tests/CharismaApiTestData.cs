#if UNITY_EDITOR

using UnityEngine;

namespace CharismaSDK.Tests
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Charisma/Testing/ApiTestData", fileName = "NewApiTestData")]
    public class CharismaApiTestData : ScriptableObject
    {
        public int StoryId => _storyId;
        public int StoryVersion => _storyVersion;
        public string ApiKey => _apiKey;

        public string FirstReply => _firstReply;
        public string FirstExpectedMessage => _firstExpectedMessage;
        public string SecondReply => _secondReply;
        public string SecondExpectedMessage => _secondExpectedMessage;

        public bool HasMemoryValue => !string.IsNullOrEmpty(MemoryRecallValue) && !string.IsNullOrEmpty(MemorySaveValue);

        public string MemoryRecallValue => _memoryRecallValue;
        public string MemorySaveValue => _memorySaveValue;

        public bool HasPublishedPlaythroughForFork => _hasPublishedPlaythroughAndDraft;

        public int DraftActorCount => _draftActorCount;
        public int ForkActorCount => _forkActorCount;

        [Space(10)]

        [Header(header: "Playthrough Information")]
        [SerializeField]
        private int _storyId;
        [SerializeField]
        private int _storyVersion;
        [SerializeField]
        private string _apiKey;

        [Header(header: "Expected Test/Story Data")]
        [SerializeField]
        [Tooltip("First reply to send to the story.")]
        private string _firstReply;
        [SerializeField]
        [Tooltip("First expected message to be received from Charisma API. Used in GetMessageHistory test")]
        private string _firstExpectedMessage;
        [SerializeField]
        [Tooltip("Second reply to send to the story.")]
        private string _secondReply;
        [SerializeField]
        [Tooltip("Second expected message to be received from Charisma API. Used in GetMessageHistory test")]
        private string _secondExpectedMessage;

        [Space(10)]

        [SerializeField]
        [Tooltip("Memory recall value, only use if Playthrough has a valid memory.")]
        private string _memoryRecallValue;
        [SerializeField]
        [Tooltip("Memory Save Value, to be registered to the Memory defined by the RecallValue above. Used in SetMemory test")]
        private string _memorySaveValue;

        [Space(10)]

        [SerializeField]
        [Tooltip("Set to true if Playthrough has both published and draft versions. For ForkPlaythrough test")]
        private bool _hasPublishedPlaythroughAndDraft;
        [SerializeField]
        [Tooltip("Number of actor the draft version of the story has. Should be different from ForkActorCount. For ForkPlaythrough test")]
        private int _draftActorCount;
        [SerializeField]
        [Tooltip("Number of actor the forked/published version of the story has. Should be different from DraftActorCount. For ForkPlaythrough test")]
        private int _forkActorCount;
    }
}

#endif
