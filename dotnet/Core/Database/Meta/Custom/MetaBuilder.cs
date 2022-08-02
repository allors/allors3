// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Database.Meta
{
    using System.Linq;

    public partial class MetaBuilder
    {
        static void AddWorkspace(Class @class, string workspaceName) => @class.AssignedWorkspaceNames = (@class.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(MethodType methodType, string workspaceName) => methodType.AssignedWorkspaceNames = (methodType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(RelationType relationType, string workspaceName) => relationType.AssignedWorkspaceNames = (relationType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        private void BuildCustom(MetaPopulation meta, Domains domains, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            // Methods
            AddWorkspace(methodTypes.DeletableDelete, "Default");

            // Relations
            AddWorkspace(relationTypes.UserUserEmail, "Default");
            AddWorkspace(relationTypes.UserUserName, "Default");

            AddWorkspace(relationTypes.RoleName, "Default");

            // Objects
            AddWorkspace(meta.UserGroup, "Default");

            // Classes
            var classes = meta.Classes.Where(@class =>
                @class.RoleTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                @class.AssociationTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                @class.MethodTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")))
                .ToArray();

            foreach (Class @class in classes)
            {
                AddWorkspace(@class, "Default");
            }
        }
    }
}
