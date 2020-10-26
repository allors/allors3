// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class SupplierRelationshipDerivation : DomainDerivation
    {
        public SupplierRelationshipDerivation(M m) : base(m, new Guid("D0B8E2E4-3A11-474A-99FC-B39E4DDAD6E5")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.SupplierRelationship.Class),
                new ChangedPattern(M.SupplierRelationship.FromDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (SupplierRelationship supplierRelationship in matches.Cast<SupplierRelationship>())
            {
                if (supplierRelationship.ExistSupplier)
                {
                    var internalOrganisationDerivedRoles = supplierRelationship.InternalOrganisation;
                    if (internalOrganisationDerivedRoles != null)
                    {
                        if (supplierRelationship.FromDate <= supplierRelationship.Session().Now() && (!supplierRelationship.ExistThroughDate || supplierRelationship.ThroughDate >= supplierRelationship.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.AddActiveSupplier(supplierRelationship.Supplier);
                        }

                        if (supplierRelationship.FromDate > supplierRelationship.Session().Now() || (supplierRelationship.ExistThroughDate && supplierRelationship.ThroughDate < supplierRelationship.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.RemoveActiveSupplier(supplierRelationship.Supplier);
                        }
                    }


                    if (supplierRelationship.Supplier.ContactsUserGroup != null)
                    {
                        foreach (OrganisationContactRelationship contactRelationship in supplierRelationship.Supplier.OrganisationContactRelationshipsWhereOrganisation)
                        {
                            if (contactRelationship.FromDate <= supplierRelationship.Session().Now() &&
                                (!contactRelationship.ExistThroughDate || supplierRelationship.ThroughDate >= supplierRelationship.Session().Now()))
                            {
                                if (!supplierRelationship.Supplier.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    supplierRelationship.Supplier.ContactsUserGroup.AddMember(contactRelationship.Contact);
                                }
                            }
                            else
                            {
                                if (supplierRelationship.Supplier.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    supplierRelationship.Supplier.ContactsUserGroup.RemoveMember(contactRelationship.Contact);
                                }
                            }
                        }
                    }
                }

                supplierRelationship.Parties = new Party[] { supplierRelationship.Supplier, supplierRelationship.InternalOrganisation };
            }
        }
    }
}
