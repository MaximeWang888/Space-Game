namespace Duncan.Model
{
    public class UnitLocation
    {
        public string? System { get; set; }
        public string? Planet { get; set; }
        public IReadOnlyDictionary<string, int>? ResourcesQuantity { get; set; }
        
    }
}
