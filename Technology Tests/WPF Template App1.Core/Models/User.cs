﻿using System.Collections.Generic;

namespace WPF_Template_App1.Core.Models
{
    // This class contains user members to download user information from Microsoft Graph
    // https://docs.microsoft.com/graph/api/resources/user?view=graph-rest-1.0
    public class User
    {
        public string Id { get; set; }

        public List<string> BusinessPhones { get; set; }

        public string DisplayName { get; set; }

        public string GivenName { get; set; }

        public object JobTitle { get; set; }

        public string Mail { get; set; }

        public string MobilePhone { get; set; }

        public object OfficeLocation { get; set; }

        public string PreferredLanguage { get; set; }

        public string Surname { get; set; }

        public string UserPrincipalName { get; set; }

        public string Photo { get; set; }
    }
}
