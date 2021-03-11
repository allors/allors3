// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Protocol.Json
{
    using Allors.Protocol.Json.Data;
    using Data;
    using Extent = Allors.Protocol.Json.Data.Extent;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Select = Allors.Protocol.Json.Data.Select;

    public static class Extensions
    {
        public static Pull ToJson(this Allors.Workspace.Data.Pull pull)
        {
            var toJsonVisitor = new ToJsonVisitor();
            pull.Accept(toJsonVisitor);
            return toJsonVisitor.Pull;
        }

        public static Extent ToJson(this IExtent extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Extent;
        }

        public static Select ToJson(this Allors.Workspace.Data.Select @select)
        {
            var toJsonVisitor = new ToJsonVisitor();
            @select.Accept(toJsonVisitor);
            return toJsonVisitor.Select;
        }
    }
}
