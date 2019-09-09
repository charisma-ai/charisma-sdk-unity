using System;
using Newtonsoft.Json;
using UnityEngine;

namespace CharismaSDK
{
    [Serializable]
    public class Memory
    {
        public string memoryRecallValue;
        public string saveValue;
        
        private readonly int _id;
        
        public Memory(string recallValue, string saveValue)
        {
            this.memoryRecallValue = recallValue;
            this.saveValue = saveValue;          
        }
        
        [JsonConstructor]
        public Memory(int id, string recallValue, string saveValue)
        {
            this._id = id;
            this.memoryRecallValue = recallValue;
            this.saveValue = saveValue;          
        }

        public int Id => _id;
        public string MemoryRecallValue => memoryRecallValue;
        public string SaveValue
        {
            get => saveValue;
            set
            {
                saveValue = value; 
                Debug.LogFormat($"Memory: {memoryRecallValue} new value: {value}");

                if (memoryRecallValue == "round_counter")
                {
                    Debug.Log("");
                }
            }
        }

    }
}
