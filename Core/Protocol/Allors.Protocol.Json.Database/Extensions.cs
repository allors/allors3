// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Database
{
    using System;
    using Meta;

    public static class Extensions
    {
        public static IAssociationType FindAssociationType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).AssociationType : null;

        public static IRoleType FindRoleType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).RoleType : null;

        public static Data.Pull FromJson(this Pull pull, ISession session)
        {
            var fromJsonVisitor = new FromJsonVisitor(session);
            pull.Accept(fromJsonVisitor);
            return fromJsonVisitor.Pull;
        }

        public static Data.IExtent FromJson(this Extent extent, ISession session)
        {
            var fromJsonVisitor = new FromJsonVisitor(session);
            extent.Accept(fromJsonVisitor);
            return fromJsonVisitor.Extent;
        }

        public static Data.Fetch FromJson(this Fetch extent, ISession session)
        {
            var fromJsonVisitor = new FromJsonVisitor(session);
            extent.Accept(fromJsonVisitor);
            return fromJsonVisitor.Fetch;
        }
        
        public static Pull ToJson(this Data.Pull pull)
        {
            var toJsonVisitor = new ToJsonVisitor();
            pull.Accept(toJsonVisitor);
            return toJsonVisitor.Pull;
        }

        public static Extent ToJson(this Data.IExtent extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Extent;
        }

        public static Fetch ToJson(this Data.Fetch extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Fetch;
        }
    }
}
