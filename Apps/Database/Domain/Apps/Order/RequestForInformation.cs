// <copyright file="RequestForInformation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class RequestForInformation
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.RequestForInformation, this.M.RequestForInformation.RequestState),
        };

        public void AppsOnPostDerive(ObjectOnPostDerive method)
        {
            var deletePermission = new Permissions(this.Strategy.Session).Get(this.Meta.ObjectType, this.Meta.Delete);
            if (this.IsDeletable())
            {
                this.RemoveDeniedPermission(deletePermission);
            }
            else
            {
                this.AddDeniedPermission(deletePermission);
            }
        }
    }
}
