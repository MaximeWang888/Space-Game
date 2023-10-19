namespace Duncan.Model
{
    public class BuildingBody
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? BuilderId { get; set; }

        public BuildingBody()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}
