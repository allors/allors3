// <copyright file="CommunicationEventExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class CommunicationEventExtensions
    {
        public static void AppsOnPostDerive(this CommunicationEvent @this, ObjectOnPostDerive method)
        {
            @this.AddSecurityToken(new SecurityTokens(@this.Session()).DefaultSecurityToken);
            @this.AddSecurityToken(@this.Owner?.OwnerSecurityToken);
        }
    }
}
