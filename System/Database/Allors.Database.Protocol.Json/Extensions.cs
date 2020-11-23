// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using Allors.Protocol.Json.Data;
    using Meta;
    using Extent = Data.Extent;
    using Fetch = Data.Fetch;
    using Pull = Allors.Protocol.Json.Data.Pull;

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

        public static Data.IExtent FromJson(this Allors.Protocol.Json.Data.Extent extent, ISession session)
        {
            var fromJsonVisitor = new FromJsonVisitor(session);
            extent.Accept(fromJsonVisitor);
            return fromJsonVisitor.Extent;
        }

        public static Fetch FromJson(this Allors.Protocol.Json.Data.Fetch extent, ISession session)
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

        public static Allors.Protocol.Json.Data.Extent ToJson(this Data.IExtent extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Extent;
        }

        public static Allors.Protocol.Json.Data.Fetch ToJson(this Fetch extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Fetch;
        }
    }
}
