// <copyright file="TestAuthenticationController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Protocol.Json.Auth;

    [AllowAnonymous]
    public class TestAuthenticationController : Controller
    {
        public TestAuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
        }

        public UserManager<IdentityUser> UserManager { get; }

        public SignInManager<IdentityUser> SignInManager { get; }

        // Passwordless cookie sign-in for browser-context tests: issues the Identity application
        // cookie (no bearer token), mirroring how the real app authenticates.
        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody]AuthenticationTokenRequest request)
        {
            if (this.ModelState.IsValid && !string.IsNullOrWhiteSpace(request.l))
            {
                var user = await this.UserManager.FindByNameAsync(request.l);
                if (user != null)
                {
                    await this.SignInManager.SignInAsync(user, isPersistent: false);
                    return this.Ok(new AuthenticationTokenResponse { a = true, u = user.Id });
                }
            }

            return this.Unauthorized();
        }
    }
}

