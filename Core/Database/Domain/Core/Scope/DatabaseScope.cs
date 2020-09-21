// <copyright file="DatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors
{
    using Meta;
    using Services;

    public abstract partial class DatabaseScope : IDatabaseScope
    {
        public M M { get; protected set; }

        public IBarcodeService BarcodeService { get; protected set; }

        public ICacheService CacheService { get; protected set; }

        public IDerivationService DerivationService { get; protected set; }

        public IExtentService ExtentService { get; protected set; }

        public IFetchService FetchService { get; protected set; }

        public IMailService MailService { get; protected set; }

        public IPasswordService PasswordService { get; protected set; }

        public ISingletonService SingletonService { get; protected set; }

        public IStickyService StickyService { get; protected set; }

        public ITimeService TimeService { get; protected set; }

        public ITreeService TreeService { get; protected set; }

        public abstract void OnInit(IDatabase database);

        public abstract ISessionScope CreateSessionScope();
    }
}
