namespace Duncan.Model
{
    public class User
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public DateTime? DateOfCreation { get; set; }

        public IDictionary<string, int>? ResourcesQuantity { get; set; }
    }
}
