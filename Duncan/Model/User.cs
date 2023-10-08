using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Duncan.Model
{
    public class User
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public Unit? unit { get; set; }
    }
}
