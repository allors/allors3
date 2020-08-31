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
        public class PassportCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPassport = changeSet.Created.Select(v=>v.GetObject()).OfType<Passport>();

                foreach(var passport in createdPassport)
                {
                    validation.AssertIsUnique(passport, M.Passport.Number, changeSet);
                }
            }
        }

        public static void PassportRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("f0e0513a-e10a-4ec4-bd4d-952f34fd87c8")] = new EmploymentCreationDerivation();
        }
    }
}
