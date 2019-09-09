using Newtonsoft.Json;

namespace CharismaSDK
{
    public class CharacterMood
    {
        [JsonConstructor]
        public CharacterMood(int id, string name, Emotions mood)
        {
            this.Id = id;
            this.Name = name;
            this.Emotions = mood;
        }
        
        public int Id { get; }
        public string Name { get; }
        public Emotions Emotions { get; }
    }
}
