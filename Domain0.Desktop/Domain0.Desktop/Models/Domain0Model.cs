using Domain0.Api.Client;
using System.Collections.Generic;

namespace Domain0.Desktop.Models
{
    public class Domain0Model
    {
        public Dictionary<int, UserProfile> UserProfiles { get; set; }
        public Dictionary<int, Role> Roles { get; set; }
        public Dictionary<int, Permission> Permissions { get; set; }
        public Dictionary<int, MessageTemplate> MessageTemplates { get; set; }
        public Dictionary<int, Application> Applications { get; set; }
    }
}
