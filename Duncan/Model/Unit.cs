using Shard.Shared.Core;

namespace Duncan.Model
{
    public class Unit
    {
        public string? Type { get; set; }
        public string? Id { get; set; }
        public string? planet { get; set; }
        public string? system { get; set; }

        public Unit(string id)
        {
            this.Id = id;
        }

    }

    
}
