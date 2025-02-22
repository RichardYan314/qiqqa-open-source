﻿using System;
using System.Threading.Tasks;

using WPF_Template_App1.Core.Helpers;

namespace WPF_Template_App1.Core.Contracts.Services
{
    public interface IIdentityService
    {
        event EventHandler LoggedIn;

        event EventHandler LoggedOut;

        void InitializeWithAadAndPersonalMsAccounts(string clientId, string redirectUri = null);

        void InitializeWithAadMultipleOrgs(string clientId, bool integratedAuth = false, string redirectUri = null);

        void InitializeWithAadSingleOrg(string clientId, string tenant, bool integratedAuth = false, string redirectUri = null);

        bool IsLoggedIn();

        Task<LoginResultType> LoginAsync();

        bool IsAuthorized();

        string GetAccountUserName();

        Task LogoutAsync();

        Task<string> GetAccessTokenForGraphAsync();

        Task<string> GetAccessTokenAsync(string[] scopes);

        Task<bool> AcquireTokenSilentAsync();
    }
}
