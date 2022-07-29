// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
    public partial class Organisations
    {
        private UniquelyIdentifiableCache<Organisation> cache;

        public UniquelyIdentifiableCache<Organisation> Cache => this.cache ??= new UniquelyIdentifiableCache<Organisation>(this.Transaction);

        protected override void CustomPrepare(Security security) => security.AddDependency(this.ObjectType, M.Revocation);

        protected override void CustomSecure(Security security)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.ToggleRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Name, Operations.Write),
                permissions.Get(this.Meta, this.Meta.Owner, Operations.Write),
                permissions.Get(this.Meta, this.Meta.Employees, Operations.Write),
            };
        }
    }
}
