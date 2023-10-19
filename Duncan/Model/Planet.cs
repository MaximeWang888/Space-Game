namespace Duncan.Model
{
    public class Planet
    {
        public string Name { get; }
        public int Size { get; }

        public Planet(string Name, int Size)
        {
            this.Name = Name;
            this.Size = Size;
        }
    }

}

