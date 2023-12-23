namespace Duncan.Model
{
    public record class Wormholes
    {
        public Uri BaseUri { get; init; }
        public string System { get; init; }
        public string User { get; init; }
        public string SharedPassword { get; init; }
    }
}
