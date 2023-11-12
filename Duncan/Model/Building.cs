﻿namespace Duncan.Model
{
    public class Building
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }
        public string? ResourceCategory { get; set; }
        public Boolean? IsBuilt { get; set; }
        public DateTime? EstimatedBuildTime { get; set; }
        public Building()
        {
            this.Id = Guid.NewGuid().ToString();
        }
    }
}
