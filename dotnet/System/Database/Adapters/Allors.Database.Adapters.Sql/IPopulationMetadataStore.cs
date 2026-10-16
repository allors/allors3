// <copyright file="IPopulationMetadataStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    public interface IPopulationMetadataStore
    {
        // Return null only when the metadata table is absent. Existing malformed metadata is an error.
        PopulationMetadata Read();

        // Durably retire the preceding incarnation before any domain data is changed.
        void BeginMaintenance(Guid incarnation);

        // Only the still-current maintenance owner can publish readiness.
        bool CompleteMaintenance(Guid expectedIncarnation);

        // Validate the current physical schema afresh, without destructive initialization.
        void Validate();
    }
}
