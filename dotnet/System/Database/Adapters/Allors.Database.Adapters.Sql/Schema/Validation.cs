// <copyright file="Validation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using Meta;

    public abstract class Validation
    {
        public abstract string Message { get; }

        public abstract bool IsValid { get; }

        // A single role value is keyed by its association. A collection is keyed by the pair (association, role).
        protected static bool HasRelationPrimaryKey(IRoleType roleType, int keyColumnCount, int? associationOrdinal, int? roleOrdinal) =>
            associationOrdinal == 1 && (roleType.IsOne ? keyColumnCount == 1 : keyColumnCount == 2 && roleOrdinal == 2);
    }
}
