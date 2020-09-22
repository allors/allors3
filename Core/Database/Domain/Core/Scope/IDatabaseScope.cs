// <copyright file="IDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Meta;
    using Services;

    public partial interface IDatabaseScope : IDatabaseLifecycle
    {
        M M { get; }

        IBarcodeService BarcodeService { get; }

        ICacheService CacheService { get; }

        IDerivationService DerivationService { get; }

        IExtentService ExtentService { get; }

        IFetchService FetchService { get; }

        IMailService MailService { get; }

        IMetaService MetaService { get; }

        IPasswordService PasswordService { get; }

        ISingletonService SingletonService { get; }

        IStickyService StickyService { get; }

        ITimeService TimeService { get; }

        ITreeService TreeService { get; }
    }
}
