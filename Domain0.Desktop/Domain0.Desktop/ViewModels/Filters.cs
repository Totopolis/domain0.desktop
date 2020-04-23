using Domain0.Api.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using Environment = Domain0.Api.Client.Environment;

namespace Domain0.Desktop.ViewModels
{
    public static class Filters
    {

        public static Func<T, bool> CreateModelFilter<T>(IEnumerable<ModelFilter> filters)
        {
            return model =>
            {
                foreach (var filter in filters)
                {
                    if (string.IsNullOrEmpty(filter.Filter))
                        continue;

                    var value = filter.Property.GetValue(model)?.ToString();
                    if (string.IsNullOrEmpty(value) || !value.Contains(filter.Filter, StringComparison.InvariantCultureIgnoreCase))
                        return false;
                }

                return true;
            };
        }

        public static Func<Role, bool> CreateRolesPredicate(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return role => true;

            return role => !string.IsNullOrEmpty(role.Name) &&
                           role.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        }

        public static Func<Permission, bool> CreatePermissionsPredicate(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return permission => true;

            return permission => !string.IsNullOrEmpty(permission.Name) &&
                                 permission.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase);
        }

        public static Func<MessageTemplate, bool> CreateEnvironmentPredicate(Environment filter)
        {
            if (filter?.Id == null)
                return message => true;

            return message => message.EnvironmentId == filter.Id;
        }

    }

    public class ModelFilter
    {
        public PropertyInfo Property { get; set; }
        public string Filter { get; set; }
    }
}
