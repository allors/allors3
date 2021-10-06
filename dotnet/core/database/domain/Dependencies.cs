// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;
    using Services;

    public static class Dependencies
    {
        public static readonly string OrganisationDisplayNameId = "organisationDisplayName";

        public static void Create(IDependencyService service, MetaPopulation m)
        {
            var organisationDisplayName = service.GetDependencySet(OrganisationDisplayNameId);
            organisationDisplayName.Add(m.Organisation, m.Organisation.Owner);
        }
    }
}
