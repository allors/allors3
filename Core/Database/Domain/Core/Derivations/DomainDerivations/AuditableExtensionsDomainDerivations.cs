// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Allors.Meta;
    using DataUtils;
    using HeyRed.Mime;

    public static partial class DabaseExtensions
    {
        public class AuditableExtensionsDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdAuditables = changeSet.Created.Select(session.Instantiate).OfType<Auditable>();

                foreach (var auditable in createdAuditables)
                {
                    var user = auditable.Strategy.Session.GetUser();
                    if (user != null)
                    {
                        if (changeSet.Created.Contains(auditable.Id))
                        {
                            auditable.CreationDate = auditable.Strategy.Session.Now();
                            auditable.CreatedBy = user;
                        }

                        if (changeSet.Associations.Contains(auditable.Id))
                        {
                            auditable.LastModifiedDate = auditable.Strategy.Session.Now();
                            auditable.LastModifiedBy = user;
                        }
                    }
                }
            }
        }

        public static void AuditableExtensionsRegisterDerivations(this IDatabase auditable)
        {
            auditable.DomainDerivationById[new Guid("a310ed3b-2129-4bee-8457-ae3c8441597f")] = new AuditableExtensionsDerivation();
        }
    }
}
