// <copyright file="Auditable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static class AuditableExtensions
    {
        public static void CoreOnPostDerive(this Auditable @this, ObjectOnPostDerive method)
        {
            var user = @this.Strategy.Session.State().User;
            if (user != null)
            {
                var derivation = method.Derivation;
                var changeSet = derivation.ChangeSet;

                if (changeSet.Created.Contains(@this.Strategy))
                {
                    @this.CreationDate = @this.Strategy.Session.Now();
                    @this.CreatedBy = user;
                }

                if (changeSet.Associations.Contains(@this.Id))
                {
                    @this.LastModifiedDate = @this.Strategy.Session.Now();
                    @this.LastModifiedBy = user;
                }
            }
        }
    }
}
