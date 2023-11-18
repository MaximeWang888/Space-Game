namespace Duncan.Model
{
    public class UserWithUnits
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public List<Unit>? Units { get; set; } = new List<Unit>();
        public List<Building>? Buildings { get; set; } = new List<Building>();
        public IDictionary<string, int>? ResourcesQuantity { get; set; }
    }
}
