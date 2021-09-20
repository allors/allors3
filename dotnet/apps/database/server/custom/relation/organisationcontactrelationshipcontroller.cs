// <copyright file="OrganisationContactRelationshipController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using Allors.Services;
    using Domain;
    using Services;
    using Protocol.Json;
    using Microsoft.AspNetCore.Mvc;

    public class OrganisationContactRelationshipController : Controller
    {

        public OrganisationContactRelationshipController(ITransactionService transactionService, IWorkspaceService workspaceService)
        {
            this.TransactionService = transactionService;
            this.WorkspaceService = workspaceService;
        }

        public ITransactionService TransactionService { get; }

        public IWorkspaceService WorkspaceService { get; }

        [HttpPost]
        public IActionResult Pull([FromBody] Model model)
        {
            var api = new Api(this.TransactionService.Transaction, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();

            var organisationContactRelationship = (OrganisationContactRelationship)this.TransactionService.Transaction.Instantiate(model.Id);
            response.AddObject("organisationContactRelationship", organisationContactRelationship);

            response.AddObject("contact", organisationContactRelationship.Contact);

            var locales = new Locales(this.TransactionService.Transaction).Extent();
            response.AddCollection("locales", locales);

            var genders = new GenderTypes(this.TransactionService.Transaction).Extent();
            response.AddCollection("genders", genders);

            var salutations = new Salutations(this.TransactionService.Transaction).Extent();
            response.AddCollection("salutations", salutations);

            var contactKinds = new OrganisationContactKinds(this.TransactionService.Transaction).Extent();
            response.AddCollection("organisationContactKinds", contactKinds);

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}
