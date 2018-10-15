using Domain0.Api.Client;
using DynamicData;

namespace Domain0.Desktop.Models
{
    public class Domain0Model
    {
        public SourceCache<UserProfile, int> UserProfiles { get; }
        public SourceCache<Role, int> Roles { get; }
        public SourceCache<Permission, int> Permissions { get; }
        public SourceCache<Application, int> Applications { get; }
        public SourceCache<MessageTemplate, int> MessageTemplates { get; }

        public SourceList<RolePermission> RolePermissions { get; }
        public SourceList<UserPermission> UserPermissions { get; }

        public Domain0Model()
        {
            UserProfiles = new SourceCache<UserProfile, int>(x => x.Id);
            Roles = new SourceCache<Role, int>(x => x.Id.Value);
            Permissions = new SourceCache<Permission, int>(x => x.Id.Value);
            Applications = new SourceCache<Application, int>(x => x.Id.Value);
            MessageTemplates = new SourceCache<MessageTemplate, int>(x => x.Id.Value);

            RolePermissions = new SourceList<RolePermission>();
            UserPermissions = new SourceList<UserPermission>();
        }
    }
}
