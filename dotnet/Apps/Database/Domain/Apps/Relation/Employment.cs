// <copyright file="Employment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class Employment
    {
        public void AppsOnInit(ObjectOnInit method)
        {
            // TODO: Don't extent for InternalOrganisations
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistEmployer && internalOrganisations.Length == 1)
            {
                this.Employer = internalOrganisations.First();
            }
        }
    }
}
