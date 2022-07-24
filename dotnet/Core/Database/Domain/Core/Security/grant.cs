// <copyright file="AccessControl.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Security;

    public partial class Grant : IGrant
    {
        public void CoreOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            derivation.Validation.AssertAtLeastOne(this, this.Meta.Subjects, this.Meta.SubjectGroups);
        }

        IPermission[] IGrant.Permissions => this.EffectivePermissions.ToArray();
    }
}
