// <copyright file="TestAuthenticationController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Auth;
    using Allors.Security;
    using Allors.Server;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    public class TestAuthenticationController : Controller
    {
        public TestAuthenticationController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            this.UserManager = userManager;
            this.Configuration = config;
        }

        public UserManager<IdentityUser> UserManager { get; }

        public IConfiguration Configuration { get; }

        [HttpPost]
        public async Task<IActionResult> Token([FromBody]AuthenticationTokenRequest request)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindByNameAsync(request.l);

                if (user != null)
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

            return this.Ok(new { Authenticated = false });
        }
    }
}
