// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Protocol.Json
{
    using Allors.Workspace.Data;
    using Extent = Allors.Protocol.Json.Data.Extent;
    using Fetch = Allors.Protocol.Json.Data.Fetch;
    using Pull = Allors.Protocol.Json.Data.Pull;

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

        public static Fetch ToJson(this Allors.Workspace.Data.Fetch fetch)
        {
            var toJsonVisitor = new ToJsonVisitor();
            fetch.Accept(toJsonVisitor);
            return toJsonVisitor.Fetch;
        }
    }
}
