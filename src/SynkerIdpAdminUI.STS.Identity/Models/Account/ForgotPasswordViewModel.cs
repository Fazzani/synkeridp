﻿using System.ComponentModel.DataAnnotations;

namespace SynkerIdpAdminUI.STS.Identity.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "EMAIL_REQUIRED")]
        [EmailAddress(ErrorMessage = "EMAIL_INVALID")]
        public string Email { get; set; }
    }
}
