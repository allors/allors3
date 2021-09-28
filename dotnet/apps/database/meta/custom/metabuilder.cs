// <copyright file="MetaBuilder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;

namespace Allors.Database.Meta
{
    public partial class MetaBuilder
    {
        static void AddWorkspace(Class @class, string workspaceName) => @class.AssignedWorkspaceNames = (@class.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(MethodType methodType, string workspaceName) => methodType.AssignedWorkspaceNames = (methodType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        static void AddWorkspace(RelationType relationType, string workspaceName) => relationType.AssignedWorkspaceNames = (relationType.AssignedWorkspaceNames ?? Array.Empty<string>()).Append(workspaceName).Distinct().ToArray();

        private void BuildCustom(MetaPopulation meta, Domains domains, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            // Methods
            AddWorkspace(methodTypes.DeletableDelete, "Default");

            AddWorkspace(methodTypes.PrintablePrint, "Default");

            // Relations
            AddWorkspace(relationTypes.CountryName, "Default");

            AddWorkspace(relationTypes.CurrencyIsoCode, "Default");

            AddWorkspace(relationTypes.EnumerationName, "Default");
            AddWorkspace(relationTypes.EnumerationIsActive, "Default");

            AddWorkspace(relationTypes.LanguageName, "Default");

            AddWorkspace(relationTypes.LocaleName, "Default");
            AddWorkspace(relationTypes.LocaleCountry, "Default");
            AddWorkspace(relationTypes.LocaleLanguage, "Default");

            AddWorkspace(relationTypes.LocalisedLocale, "Default");

            AddWorkspace(relationTypes.ObjectStateName, "Default");

            AddWorkspace(relationTypes.PersonFirstName, "Default");
            AddWorkspace(relationTypes.PersonLastName, "Default");
            AddWorkspace(relationTypes.PersonMiddleName, "Default");

            AddWorkspace(relationTypes.UserUserEmail, "Default");
            AddWorkspace(relationTypes.UserUserName, "Default");

            AddWorkspace(relationTypes.RoleName, "Default");

            // Classes
            // TODO: Optimize
            foreach (Class @class in meta.Classes)
            {
                if (@class.RoleTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                      @class.AssociationTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")) ||
                      @class.MethodTypes.Any(v => v.AssignedWorkspaceNames.Contains("Default")))
                {
                    AddWorkspace(@class, "Default");
                }
            }
        }
    }
}
