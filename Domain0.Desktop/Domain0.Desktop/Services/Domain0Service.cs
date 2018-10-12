using Domain0.Api.Client;
using Domain0.Desktop.Models;
using Domain0.Desktop.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Domain0.Desktop.Services
{
    public class Domain0Service : IDomain0Service
    {
        private readonly HttpClient _httpClient;
        private readonly Domain0Client _client;

        public Domain0Service()
        {
            _httpClient = new HttpClient();
            _client = new Domain0Client(null, _httpClient);

            Model = new Domain0Model();
        }

        public string HostUrl
        {
            get => _client.BaseUrl;
            set => _client.BaseUrl = value;
        }

        public bool LoadToken()
        {
            var savedToken = Settings.Default.AccessToken;

            if (string.IsNullOrEmpty(savedToken))
                return false;

            try
            {
                var token = AccessTokenResponse.FromJson(savedToken);
                AccessToken = token;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void ResetAccessToken()
        {
            Settings.Default.AccessToken = string.Empty;
            Settings.Default.Save();

            AccessToken = null;
        }

        public void UpdateAccessToken(AccessTokenResponse token, bool shouldRemember)
        {
            if (shouldRemember)
            {
                Settings.Default.AccessToken = token.ToJson();
                Settings.Default.Save();
            }
            else
            {
                Settings.Default.AccessToken = string.Empty;
                Settings.Default.Save();
            }

            AccessToken = token;
        }

        private AccessTokenResponse AccessToken
        {
            set => _httpClient.DefaultRequestHeaders.Authorization =
                value != null ? new AuthenticationHeaderValue("Bearer", value.AccessToken) : null;
        }


        public IDomain0Client Client => _client;
        public Domain0Model Model { get; }

        public async Task LoadModel()
        {
            var controller = await (System.Windows.Application.Current.MainWindow as MetroWindow)
                .ShowProgressAsync("Loading data...", "Load everything");


            controller.SetMessage("Load User Profiles...");
            var userProfiles = await _client.GetUserByFilterAsync(new UserProfileFilter(new List<int>()));
            Model.UserProfiles.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(userProfiles);
            });

            controller.SetMessage("Load Permissions...");
            var permissions = await _client.LoadPermissionsByFilterAsync(new PermissionFilter(new List<int>()));
            Model.Permissions.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(permissions);
            });

            controller.SetMessage("Load Roles...");
            var roles = await _client.LoadRolesByFilterAsync(new RoleFilter(new List<int>()));
            Model.Roles.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(roles);
            });

            controller.SetMessage("Load Appliations...");
            var applications = await _client.LoadApplicationsByFilterAsync(new ApplicationFilter(new List<int>()));
            Model.Applications.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(applications);
            });


            controller.SetMessage("Load MessageTemplates...");
            var messageTemplates = await _client.LoadMessageTemplatesByFilterAsync(new MessageTemplateFilter(new List<int>()));
            Model.MessageTemplates.Edit(innerCache =>
            {
                innerCache.Clear();
                innerCache.AddOrUpdate(messageTemplates);
            });

            await controller.CloseAsync();
        }

    }
}