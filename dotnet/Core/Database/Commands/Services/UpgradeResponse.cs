// <copyright file="UpgradeResponse.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands.Services
{
    using System;

    public class UpgradeResponse
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public Guid[] NotLoadedObjectTypeIds { get; set; }

        public Guid[] NotLoadedRelationTypeIds { get; set; }
    }
}
