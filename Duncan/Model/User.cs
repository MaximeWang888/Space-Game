namespace Duncan.Model
{
    public class User
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public string? DateOfCreation { get; set; }

        public IReadOnlyDictionary<string, int>? ResourcesQuantity { get; set; }
    }
}
