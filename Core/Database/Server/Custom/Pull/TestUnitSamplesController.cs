// <copyright file="TestUnitSamplesController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Services;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Database.Domain;
    using Database.Protocol.Json;

    public class TestUnitSamplesController : Controller
    {
        public TestUnitSamplesController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = transactionService.Transaction;
            this.TreeCache = this.Transaction.Database.Context().TreeCache;
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        public async Task<IActionResult> Pull([FromBody] TestUnitSamplesParams @params)
        {
            try
            {
                var api = new Api(this.Transaction, this.WorkspaceService.Name);
                var response = api.CreatePullResponseBuilder();

                var unitSample = this.Transaction.UnitSample(@params.Step);
                response.AddObject("unitSample", unitSample);
                var pullResponse = response.Build();

                return this.Ok(pullResponse);
            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }
        }

        public class TestUnitSamplesParams
        {
            public int Step
            {
                get;
                set;
            }
        }
    }
}
