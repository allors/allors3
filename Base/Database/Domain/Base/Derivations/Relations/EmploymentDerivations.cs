// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class EmploymentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdEmployments = changeSet.Created.Select(session.Instantiate).OfType<Employment>();

                foreach(var employment in createdEmployments)
                {
                    employment.Parties = new Party[] { employment.Employee, employment.Employer };
                }
            }
        }

        public static void EmploymentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("49a7e662-f612-40bf-824d-307d4b245566")] = new EmploymentCreationDerivation();
        }
    }
}
