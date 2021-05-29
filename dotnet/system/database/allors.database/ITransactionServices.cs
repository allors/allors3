// <copyright file="ITransactionServices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System;
    using Derivations;

    /// <summary>
    /// The Transaction Services
    /// </summary>
    public interface ITransactionServices : IDisposable
    {
        void OnInit(ITransaction transaction);

        IDerive Derive { get; }
    }
}
