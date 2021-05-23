// <copyright file="AccessControl.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Security;

    public partial class AccessControl : IAccessControl
    {
        public void CoreOnDerive(ObjectOnDerive method)
        {
            var derivation = method.Derivation;

            derivation.Validation.AssertAtLeastOne(this, this.Meta.Subjects, this.Meta.SubjectGroups);

            this.EffectiveUsers = this.SubjectGroups.SelectMany(v => v.Members).Union(this.Subjects).ToArray();

            var permissions = this.Role?.Permissions.ToArray();
            this.EffectivePermissions = permissions;

            // Invalidate cache
            this.DatabaseContext().AccessControlCache.Clear(this.Id);
        }

        IPermission[] IAccessControl.Permissions => this.EffectivePermissions;
    }
}
