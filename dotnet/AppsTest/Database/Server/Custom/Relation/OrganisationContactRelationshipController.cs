// <copyright file="OrganisationContactRelationshipController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading;
    using Allors.Services;
    using Domain;
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
        public IActionResult Pull([FromBody] Model model, CancellationToken cancellationToken)
        {
            var api = new Api(this.TransactionService.Transaction, this.WorkspaceService.Name, cancellationToken);
            var response = api.CreatePullResponseBuilder();

            var organisationContactRelationship = (OrganisationContactRelationship)this.TransactionService.Transaction.Instantiate(model.Id);
            response.AddObject("organisationContactRelationship", organisationContactRelationship);

            response.AddObject("contact", organisationContactRelationship.Contact);

            var locales = new Locales(this.TransactionService.Transaction);
            response.AddCollection("locales", locales.ObjectType, locales.Extent());

            var genders = new GenderTypes(this.TransactionService.Transaction);
            response.AddCollection("genders", genders.ObjectType, genders.Extent());

            var salutations = new Salutations(this.TransactionService.Transaction);
            response.AddCollection("salutations", salutations.ObjectType, salutations.Extent());

            var contactKinds = new OrganisationContactKinds(this.TransactionService.Transaction);
            response.AddCollection("organisationContactKinds", contactKinds.ObjectType, contactKinds.Extent());

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}
