﻿namespace Duncan.Model
{
    public class UnitBody
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? Planet { get; set; }
        public string? System { get; set; }
        public string? DestinationSytem { get; set; }
        public string? DestinationPlanet { get; set; }

        public UnitBody()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}