// <copyright file="Auditable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors.Database.Services;

    public static class AuditableExtensions
    {
        public static void CoreOnPostDerive(this Auditable @this, ObjectOnPostDerive method)
        {
            var user = @this.Strategy.Transaction.Services.Get<IUserService>().User;
            if (user != null)
            {
                var derivation = method.Derivation;
                var changeSet = derivation.ChangeSet;

                if (changeSet.Created.Contains(@this))
                {
                    @this.CreationDate = @this.Strategy.Transaction.Now();
                    @this.CreatedBy = (User)user;
                }

                if (changeSet.Associations.Contains(@this))
                {
                    @this.LastModifiedDate = @this.Strategy.Transaction.Now();
                    @this.LastModifiedBy = (User)user;
                }
            }
        }
    }
}
