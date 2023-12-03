using System.Text.Json.Serialization;

namespace Duncan.Model
{
    public class Building
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }
        public string? BuilderId {  get; set; }
        public string? ResourceCategory { get; set; }
        public bool? IsBuilt { get; set; }
        public DateTime? EstimatedBuildTime { get; set; }

        [JsonIgnore]
        public Task? Task { get; set; }

        [JsonIgnore]
        public Task? TaskTwo { get; set; }

        [JsonIgnore]
        public CancellationTokenSource? CancellationSource { get; set; }
        public Building()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
