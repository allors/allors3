// <copyright file="Login.cshtml.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.IdentityUi.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    // Overrides the Microsoft Identity UI Login page so it binds a user NAME rather than an
    // [EmailAddress] Input.Email — Allors user names are not necessarily e-mail addresses
    // (e.g. "administrator"), which could never pass e-mail validation.
    [AllowAnonymous]
    public partial class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public void OnGet(string returnUrl = null) => this.ReturnUrl = returnUrl ?? this.Url.Content("~/");

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= this.Url.Content("~/");
            this.ReturnUrl = returnUrl;

            if (this.ModelState.IsValid)
            {
                var result = await this.signInManager.PasswordSignInAsync(this.Input.UserName, this.Input.Password, this.Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    this.LogLoggedIn(this.Input.UserName);
                    return this.LocalRedirect(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    this.LogLockedOut(this.Input.UserName);
                    return this.RedirectToPage("./Lockout");
                }

                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return this.Page();
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "User {UserName} logged in.")]
        private partial void LogLoggedIn(string userName);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "User {UserName} account locked out.")]
        private partial void LogLockedOut(string userName);

        public class InputModel
        {
            [Required]
            [Display(Name = "User name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}
