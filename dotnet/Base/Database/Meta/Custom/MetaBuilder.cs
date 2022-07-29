// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;
    using System.Linq;

    public partial class MetaBuilder
    {
        static void AddWorkspace(Class @class, string workspaceName) => @class.AssignedWorkspaceNames = (@class.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(MethodType methodType, string workspaceName) => methodType.AssignedWorkspaceNames = (methodType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(RelationType relationType, string workspaceName) => relationType.AssignedWorkspaceNames = (relationType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        private void BuildCustom(MetaPopulation meta, Domains domains, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            this.DefaultWorkspace(meta, domains, relationTypes, methodTypes);
        }

        private void DefaultWorkspace(MetaPopulation meta, Domains domains, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            const string workspaceName = "Default";

            relationTypes.OrganisationName.RoleType.IsRequired = true;

            AddWorkspace(meta.Gender, workspaceName);
        }
    }
}
