using System;
using Newtonsoft.Json;

namespace CharismaSdk
{
    [Serializable]
    public class Memory
    {
        public string memoryRecallValue;
        public string saveValue;

        private readonly int _id;

        [JsonConstructor]
        public Memory(int id, string recallValue, string saveValue)
        {
            this._id = id;
            this.memoryRecallValue = recallValue;
            this.saveValue = saveValue;
        }

        /// <summary>
        /// Id of this memory
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Recall value of this memory
        /// </summary>
        public string MemoryRecallValue => memoryRecallValue;

        /// <summary>
        /// Save value of this memory
        /// </summary>
        public string SaveValue
        {
            get => saveValue;
            set => saveValue = value;
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
