namespace Duncan.Model
{
    public class CustomSystem
    {
        public string Name { get; }
        public IList<Planet> Planets { get; }

        public CustomSystem(string Name, IList<Planet> Planets) {
            this.Name = Name;
            this.Planets = Planets;
        } 
    }
}