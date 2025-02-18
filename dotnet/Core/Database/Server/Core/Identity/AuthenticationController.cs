// <copyright file="AuthenticationController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using Protocol.Json.Auth;
    using Security;
    using Services;

    public class AuthenticationController : Controller
    {
        public AuthenticationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config, ITransactionService transactionService)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.Configuration = config;
            this.TransactionService = transactionService;
        }

        public UserManager<IdentityUser> UserManager { get; }

        public SignInManager<IdentityUser> SignInManager { get; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public IConfiguration Configuration { get; }

        public ITransactionService TransactionService { get; }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Token([FromBody] AuthenticationTokenRequest request)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindByNameAsync(request.l);

                var host = this.Request.Host;

                if (host.Host.StartsWith("extranet"))
                {
                }

                if (host.Host.StartsWith("intranet"))
                {
                }

                if (user != null)
                {
                    var result = await this.SignInManager.CheckPasswordSignInAsync(user, request.p, false);
                    if (result.Succeeded)
                    {
                        var token = user.CreateToken(this.Configuration);
                        var response = new AuthenticationTokenResponse
                        {
                            a = true,
                            u = user.Id,
                            t = token,
                        };
                        return this.Ok(response);
                    }
                }
            }

            return this.Ok(new { Authenticated = false });
        }
    }
}
