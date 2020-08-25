// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class SupplierRelationshipCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSupplierRelationship = changeSet.Created.Select(session.Instantiate).OfType<SupplierRelationship>();

                foreach (SupplierRelationship supplierRelationship in createdSupplierRelationship)
                {

                    if (supplierRelationship.ExistSupplier)
                    {
                        // HACK: DerivedRoles
                        var internalOrganisationDerivedRoles = supplierRelationship.InternalOrganisation;

                        if (supplierRelationship.FromDate <= supplierRelationship.Session().Now() && (!supplierRelationship.ExistThroughDate || supplierRelationship.ThroughDate >= supplierRelationship.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.AddActiveSupplier(supplierRelationship.Supplier);
                        }

                        if (supplierRelationship.FromDate > supplierRelationship.Session().Now() || (supplierRelationship.ExistThroughDate && supplierRelationship.ThroughDate < supplierRelationship.Session().Now()))
                        {
                            internalOrganisationDerivedRoles.RemoveActiveSupplier(supplierRelationship.Supplier);
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

        public static void SupplierRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0013700f-2b32-4f36-8735-30add58ba1ec")] = new SupplierRelationshipCreationDerivation();
        }
    }
}
