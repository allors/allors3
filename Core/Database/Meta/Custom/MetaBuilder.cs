// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta
{
    public partial class MetaBuilder
    {
        private void BuildCustom(MetaPopulation meta, Domains domains, ObjectTypes objectTypes, RoleTypes roleTypes, MethodTypes methodTypes, RoleClasses roleClasses)
        {
            roleTypes.OrganisationName.IsRequired = true;
        }
    }
}
