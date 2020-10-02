// <copyright file="IDatabaseInstance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Meta;
    using Services;

    public partial interface IDatabaseInstance : IDatabaseInstanceLifecycle
    {
        IDatabase Database { get; }

        MetaPopulation MetaPopulation { get; }

        M M { get; }

        IPrefetchPolicyCache PrefetchPolicyCache { get; }

        IPermissionsCache PermissionsCache { get; }

        IEffectivePermissionCache EffectivePermissionCache { get; }

        IWorkspaceEffectivePermissionCache WorkspaceEffectivePermissionCache { get; }

        ITemplateObjectCache TemplateObjectCache { get; }

        IBarcodeGenerator BarcodeGenerator { get; }

        IDerivationService DerivationService { get; }

        IPreparedExtentCache PreparedExtentCache { get; }

        IFetchService FetchService { get; }

        IMailService MailService { get; }

        IMetaCache MetaService { get; }

        IPasswordService PasswordService { get; }

        ISingletonService SingletonService { get; }

        IStickyService StickyService { get; }

        ITimeService TimeService { get; }

        ITreeCache TreeCache { get; }
    }
}
