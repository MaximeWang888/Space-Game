namespace Duncan.Model
{
    public class BuildingBody
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? BuilderId { get; set; }
        public string? ResourceCategory { get; set; }
        public BuildingBody()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
