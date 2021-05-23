// <copyright file="ITransactionLifecycle.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System;

    /// <summary>
    /// The Transaction state's lifecycle.
    /// </summary>
    public interface ITransactionLifecycle : IDisposable
    {
        void OnInit(ITransaction transaction);
    }
}
