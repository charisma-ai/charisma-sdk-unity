using CharismaPlugin;
using Newtonsoft.Json;

namespace CharismaSDK
{
    public class Speech
    {
        [JsonConstructor]
        public Speech(Audio audio, float duration)
        {
            this.Audio = audio;
            this.Duration = duration;
        }

        public Audio Audio { get; }
        public float Duration { get; set; }
    }
}
