// <copyright file="Permissions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using Meta;

    public partial class Permissions
    {
        // TODO: Make extension method on Class
        public Permission Get(Class @class, IRoleType roleType, Operations operation)
        {
            var id = operation switch
            {
                Operations.Read => @class.ReadPermissionIdByRelationTypeId[roleType.RelationType.Id],
                Operations.Write => @class.WritePermissionIdByRelationTypeId[roleType.RelationType.Id],
                Operations.Create => 0,
                Operations.Execute => 0,
            };

            return (Permission)this.Transaction.Instantiate(id);
        }

        // TODO: Make extension method on Class
        public Permission Get(Class @class, IMethodType methodType)
        {
            var id = @class.ExecutePermissionIdByMethodTypeId[methodType.Id];
            return (Permission)this.Transaction.Instantiate(id);
        }
    }
}
