namespace Duncan.Model
{
    public class Building
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }

        public Building()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}
