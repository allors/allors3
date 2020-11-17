// <copyright file="OrganisationContactRelationshipController.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Controllers
{
    using Allors.Domain;
    using Allors.Services;
    using Database.Protocol.Json;
    using Microsoft.AspNetCore.Mvc;

    public class OrganisationContactRelationshipController : Controller
    {

        public OrganisationContactRelationshipController(ISessionService sessionService, IWorkspaceService workspaceService)
        {
            this.SessionService = sessionService;
            this.WorkspaceService = workspaceService;
        }

        public ISessionService SessionService { get; }

        public IWorkspaceService WorkspaceService { get; }

        [HttpPost]
        public IActionResult Pull([FromBody] Model model)
        {
            var api = new Api(this.SessionService.Session, this.WorkspaceService.Name);
            var response = api.CreatePullResponseBuilder();

            var organisationContactRelationship = (OrganisationContactRelationship)this.SessionService.Session.Instantiate(model.Id);
            response.AddObject("organisationContactRelationship", organisationContactRelationship);

            response.AddObject("contact", organisationContactRelationship.Contact);

            var locales = new Locales(this.SessionService.Session).Extent();
            response.AddCollection("locales", locales);

            var genders = new GenderTypes(this.SessionService.Session).Extent();
            response.AddCollection("genders", genders);

            var salutations = new Salutations(this.SessionService.Session).Extent();
            response.AddCollection("salutations", salutations);

            var contactKinds = new OrganisationContactKinds(this.SessionService.Session).Extent();
            response.AddCollection("organisationContactKinds", contactKinds);

            return this.Ok(response.Build());
        }

        public class Model
        {
            public string Id { get; set; }
        }
    }
}
