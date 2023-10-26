using Shard.Shared.Core;

namespace Duncan.Model
{
    public class Unit
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string? Planet { get; set; }

        public Unit()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}
