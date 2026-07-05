// <copyright file="UserInfoController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using Allors.Services;
    using Database;
    using Database.Domain;
    using Database.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    // The SPA's user-id source once it no longer receives the JWT token response: an authenticated
    // client GETs this to learn who is logged in; an anonymous request is challenged (401 for /allors),
    // which the client's 401 interceptor turns into a login redirect.
    [Authorize]
    [Route("allors/UserInfo")]
    public class UserInfoController : Controller
    {
        public UserInfoController(ITransactionService transactionService) => this.Transaction = transactionService.Transaction;

        private ITransaction Transaction { get; }

        [HttpGet]
        public IActionResult Get()
        {
            if (this.Transaction.Services.Get<IUserService>().User is not User user)
            {
                return this.Unauthorized();
            }

            return this.Ok(new UserInfoResponse { u = user.Id.ToString(), userName = user.UserName });
        }

        public class UserInfoResponse
        {
            public string u { get; set; }

            public string userName { get; set; }
        }
    }
}
