// <copyright file="DatabaseExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Api.Json.Pull
{
    using System.Linq;

    using Allors.Data;
    using Allors.Meta;
    using Allors.State;


    public static class DatabaseExtensions
    {
        public static Node[] FullTree(this IDatabase @this, IComposite composite, ITreeCache treeCache)
        {
            var tree = treeCache.Get(composite);
            if (tree == null)
            {
                tree = composite.DatabaseRoleTypes.Where(v => v.ObjectType.IsComposite && ((RoleType)v).RelationType.WorkspaceNames.Length > 0).Select(v => new Node(v)).ToArray();
                treeCache.Set(composite, tree);
            }

            return tree;
        }
    }
}
