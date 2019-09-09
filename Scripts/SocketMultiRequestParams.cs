using System;
using Newtonsoft.Json;

namespace CharismaSDK
{
    public class SocketMultiRequestParams 
    {
        [JsonConstructor]
        public SocketMultiRequestParams(int storyId,  int version)
        {
            this.storyId = storyId;
            this.version = version;
        }
        
        public int storyId;
        public int version;
    }

    public class SocketSingleRequestParams
    {
        [JsonConstructor]
        public SocketSingleRequestParams(int storyId)
        {
            this.storyId = storyId;
        }
        
        public int storyId;
    }
    
}
