﻿namespace SynkerIdpAdminUI.STS.Identity.Models.Account
{
    public class LoggedOutViewModel
    {
        public string PostLogoutRedirectUri { get; set; }
        public string ClientName { get; set; }
        public string SignOutIframeUrl { get; set; }
    }
}