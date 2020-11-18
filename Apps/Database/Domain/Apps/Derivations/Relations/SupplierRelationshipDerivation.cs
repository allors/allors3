// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class SupplierRelationshipDerivation : DomainDerivation
    {
        public SupplierRelationshipDerivation(M m) : base(m, new Guid("D0B8E2E4-3A11-474A-99FC-B39E4DDAD6E5")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(M.SupplierRelationship.FromDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                if (@this.ExistSupplier)
                {
                    var internalOrganisationDerivedRoles = @this.InternalOrganisation;
                    if (internalOrganisationDerivedRoles != null)
                    {
                        if (@this.FromDate <= @this.Session().Now() && (!@this.ExistThroughDate || @this.ThroughDate >= @this.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.AddActiveSupplier(@this.Supplier);
                        }

                        if (@this.FromDate > @this.Session().Now() || (@this.ExistThroughDate && @this.ThroughDate < @this.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.RemoveActiveSupplier(@this.Supplier);
                        }
                    }


                    if (@this.Supplier.ContactsUserGroup != null)
                    {
                        foreach (OrganisationContactRelationship contactRelationship in @this.Supplier.OrganisationContactRelationshipsWhereOrganisation)
                        {
                            if (contactRelationship.FromDate <= @this.Session().Now() &&
                                (!contactRelationship.ExistThroughDate || @this.ThroughDate >= @this.Session().Now()))
                            {
                                if (!@this.Supplier.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    @this.Supplier.ContactsUserGroup.AddMember(contactRelationship.Contact);
                                }
                            }
                            else
                            {
                                if (@this.Supplier.ContactsUserGroup.Members.Contains(contactRelationship.Contact))
                                {
                                    @this.Supplier.ContactsUserGroup.RemoveMember(contactRelationship.Contact);
                                }
                            }
                        }
                    }
                }

                @this.Parties = new Party[] { @this.Supplier, @this.InternalOrganisation };
            }
        }
    }
}
