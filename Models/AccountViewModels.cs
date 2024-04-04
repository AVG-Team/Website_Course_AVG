﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Website_Course_AVG.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
		[Required(ErrorMessage = "username is required")]
		[RegularExpression(@"^.{6,30}$", ErrorMessage = "Must contain at least 6 characters and at least one character.")]
		public string userName { get; set; }


		[Required(ErrorMessage = "Password is required")]
		[StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[RegularExpression(@"^.{6,30}$", ErrorMessage = "Password must contain at least 6 characters.")]
		[DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
		[Required(ErrorMessage = "Required")]
		[StringLength(12, ErrorMessage = "The {0} must be have", MinimumLength = 6)]
		[RegularExpression(@"^.{6,30}$", ErrorMessage = "Must contain at least 6 characters and at least one character.")]
		[Display(Name = "userName")]
		public string userName { get; set; }

		[Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
		[RegularExpression(@"^.{6,30}$", ErrorMessage = "Password must contain at least 6 characters.")]
		[Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[RegularExpression(@"^.{6,30}$", ErrorMessage = "Password must contain at least 6 characters.")]
		[DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]    
        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

		public string ReturnUrl { get; set; }
	}

	public class EmailConfirmation
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Code")]
		public string Code { get; set; }

		public string Name { get; set; }

		public string RedirectURL { get; set; }
		public string UrlWebsite { get; set; }
	}
}
