// <copyright file="FromJsonVisitor.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Json.Workspace
{
    using Allors.Workspace.Data;

    public static class Extensions
    {
        public static Json.Pull ToJson(this Pull pull)
        {
            var toJsonVisitor = new ToJsonVisitor();
            pull.Accept(toJsonVisitor);
            return toJsonVisitor.Pull;
        }

        public static Json.Extent ToJson(this IExtent extent)
        {
            var toJsonVisitor = new ToJsonVisitor();
            extent.Accept(toJsonVisitor);
            return toJsonVisitor.Extent;
        }

        public static Json.Fetch ToJson(this Fetch fetch)
        {
            var toJsonVisitor = new ToJsonVisitor();
            fetch.Accept(toJsonVisitor);
            return toJsonVisitor.Fetch;
        }
    }
}
