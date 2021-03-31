// <copyright file="IDatabaseContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Database.Data;
    using Derivations.Default;
    using Meta;

    public partial interface IDatabaseContext : IDatabaseLifecycle
    {
        IDatabase Database { get; }

        MetaPopulation MetaPopulation { get; }

        M M { get; }

        IPrefetchPolicyCache PrefetchPolicyCache { get; }

        IPermissionsCache PermissionsCache { get; }

        IAccessControlCache AccessControlCache { get; }

        IDerivationFactory DerivationFactory { get; }

        IPreparedExtents PreparedExtents { get; }

        IPreparedSelects PreparedSelects { get; }

        IMetaCache MetaCache { get; }

        IPasswordHasher PasswordHasher { get; }

        ICaches Caches { get; }

        ITime Time { get; }

        ITreeCache TreeCache { get; }

        IClassById ClassById { get; }

        IVersionedIdByStrategy VersionedIdByStrategy { get; }
    }
}
