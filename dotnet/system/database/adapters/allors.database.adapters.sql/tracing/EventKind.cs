// <copyright file="ITrace.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Tracing
{
    public enum EventKind
    {
        CommandsDeleteObject,
        CommandsGetUnitRoles,
        CommandsSetUnitRoles,
        CommandsGetCompositeRole,
        CommandsSetCompositeRole,
        CommandsGetCompositesRole,
        CommandsAddCompositeRole,
        CommandsRemoveCompositeRole,
        CommandsClearCompositeAndCompositesRole,
        CommandsGetCompositeAssociation,
        CommandsGetCompositesAssociation,
        CommandsCreateObject,
        CommandsCreateObjects,
        CommandsInstantiateObject,
        CommandsInstantiateReferences,
        CommandsGetVersions,
        CommandsUpdateVersion,

        PrefetcherPrefetchUnitRoles,
        PrefetcherPrefetchCompositeRoleObjectTable,
        PrefetcherPrefetchCompositeRoleRelationTable,
        PrefetcherPrefetchCompositesRoleObjectTable,
        PrefetcherPrefetchCompositesRoleRelationTable,
        PrefetcherPrefetchCompositeAssociationObjectTable,
        PrefetcherPrefetchCompositeAssociationRelationTable,
        PrefetcherPrefetchCompositesAssociationObjectTable,
        PrefetcherPrefetchCompositesAssociationRelationTable
    }
}
