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
            this.DefaultWorkspace(meta, relationTypes, methodTypes);
            this.ExtranetWorkspace(meta, relationTypes, methodTypes);
        }

        private void DefaultWorkspace(MetaPopulation meta, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            const string workspaceName = "Default";

            // Methods
            AddWorkspace(methodTypes.DeletableDelete, workspaceName);
            AddWorkspace(methodTypes.PrintablePrint, workspaceName);

            // Relations
            AddWorkspace(relationTypes.CountryName, workspaceName);
            AddWorkspace(relationTypes.CurrencyIsoCode, workspaceName);
            AddWorkspace(relationTypes.EnumerationName, workspaceName);
            AddWorkspace(relationTypes.EnumerationIsActive, workspaceName);
            AddWorkspace(relationTypes.LanguageName, workspaceName);
            AddWorkspace(relationTypes.LocaleName, workspaceName);
            AddWorkspace(relationTypes.LocaleCountry, workspaceName);
            AddWorkspace(relationTypes.LocaleLanguage, workspaceName);
            AddWorkspace(relationTypes.LocalisedLocale, workspaceName);
            AddWorkspace(relationTypes.ObjectStateName, workspaceName);
            AddWorkspace(relationTypes.PersonFirstName, workspaceName);
            AddWorkspace(relationTypes.PersonLastName, workspaceName);
            AddWorkspace(relationTypes.PersonMiddleName, workspaceName);
            AddWorkspace(relationTypes.UserUserEmail, workspaceName);
            AddWorkspace(relationTypes.UserUserName, workspaceName);
            AddWorkspace(relationTypes.RoleName, workspaceName);

            // Classes
            AddWorkspace(meta.WorkTask, workspaceName);

            var classes = meta.Classes.Where(@class =>
                    @class.RoleTypes.Any(v => v.AssignedWorkspaceNames.Contains(workspaceName)) ||
                    @class.AssociationTypes.Any(v => v.AssignedWorkspaceNames.Contains(workspaceName)) ||
                    @class.MethodTypes.Any(v => v.AssignedWorkspaceNames.Contains(workspaceName)))
                .ToArray();

            foreach (Class @class in classes)
            {
                AddWorkspace(@class, workspaceName);
            }
        }

        private void ExtranetWorkspace(MetaPopulation meta, RelationTypes relationTypes, MethodTypes methodTypes)
        {
            const string workspaceName = "Extranet";

            // Relations
            AddWorkspace(relationTypes.CountryName, workspaceName);
            AddWorkspace(relationTypes.CurrencyIsoCode, workspaceName);
            AddWorkspace(relationTypes.EnumerationName, workspaceName);
            AddWorkspace(relationTypes.EnumerationIsActive, workspaceName);
            AddWorkspace(relationTypes.LanguageName, workspaceName);
            AddWorkspace(relationTypes.LocaleName, workspaceName);
            AddWorkspace(relationTypes.LocaleCountry, workspaceName);
            AddWorkspace(relationTypes.LocaleLanguage, workspaceName);
            AddWorkspace(relationTypes.LocalisedLocale, workspaceName);
            AddWorkspace(relationTypes.ObjectStateName, workspaceName);
            AddWorkspace(relationTypes.PersonFirstName, workspaceName);
            AddWorkspace(relationTypes.PersonLastName, workspaceName);
            AddWorkspace(relationTypes.PersonMiddleName, workspaceName);
            AddWorkspace(relationTypes.UserUserEmail, workspaceName);
            AddWorkspace(relationTypes.UserUserName, workspaceName);
            AddWorkspace(relationTypes.RoleName, workspaceName);

            // Classes
            AddWorkspace(meta.Country, workspaceName);
            AddWorkspace(meta.Currency, workspaceName);
            AddWorkspace(meta.Language, workspaceName);
            AddWorkspace(meta.Locale, workspaceName);
            AddWorkspace(meta.Person, workspaceName);
            AddWorkspace(meta.Role, workspaceName);
            AddWorkspace(meta.WorkTask, workspaceName);
        }
    }
}
