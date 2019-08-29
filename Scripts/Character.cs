using Newtonsoft.Json;

namespace CharismaPlugin
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
        
        public int Id { get; }
        public string Name { get; }
        public string Avatar { get; }
    }
}
