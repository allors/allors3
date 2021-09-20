// <copyright file="TestTransactionController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Database;
    using Database.Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Services;

    public class TestTransactionController : Controller
    {
        public TestTransactionController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = transactionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services.Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        [AllowAnonymous]
        [Authorize]
        public IActionResult UserName()
        {
            var userService = this.Transaction.Services.Get<IUserService>();
            var result = userService?.User?.UserName ?? string.Empty;
            return this.Content(result);
        }
    }
}
