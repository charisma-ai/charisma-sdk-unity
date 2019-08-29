using Newtonsoft.Json;

namespace CharismaSDK
{
    public class Audio
    {
        [JsonConstructor]
        public Audio(byte[] data, string type)
        {
            this.Data = data;
            this.Type = type;
        }

/*        [JsonConstructor]
        public Audio(string url)
        {
            this.Url = url;
        }*/
        
        public string Url { get; }
        public string Type { get; }
        public byte[] Data { get; }
      
    }
}
