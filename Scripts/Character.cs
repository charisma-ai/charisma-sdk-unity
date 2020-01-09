using Newtonsoft.Json;

namespace CharismaSdk
{
    public class Character
    {
        [JsonConstructor]
        public Character(int id, string name, string avatar)
        {
            this.Id = id;
            this.Name = name;
            this.Avatar = avatar;
        }

        /// <summary>
        /// Id of this character.
        /// </summary>
        public int Id { get; }
        
        public string Name { get; }
        
        /// <summary>
        /// Url string of this characters avatar.
        /// </summary>
        public string Avatar { get; }
    }

    public class CharacterMood
    {
        [JsonConstructor]
        public CharacterMood(int id, string name, Mood mood)
        {
            this.Id = id;
            this.Name = name;
            this.Mood = mood;
        }
        
        public int Id { get; }
        public string Name { get; }
        
        /// <summary>
        /// Contains the mood of this character.
        /// </summary>
        public Mood Mood { get; }
    }
    
    public class Mood
    {
        public int anger { get; set; }
        public int trust { get; set; }
        public int patience { get; set; }
        public int happiness { get; set; }
        public int fearlessness { get; set; }
        
        [JsonConstructor]
        public Mood(int happiness, int anger, int trust, int patience, int fearlessness)
        {
            this.happiness = happiness;
            this.anger = anger;
            this.trust = trust;
            this.patience = patience;
            this.fearlessness = fearlessness;
        }
    }

    public class SetMoodParams
    {
        public string characterName;
        public Mood modifier;
        
        public SetMoodParams(string characterName, Mood modifier)
        {
            this.characterName = characterName;
            this.modifier = modifier;
        }
    }
}
