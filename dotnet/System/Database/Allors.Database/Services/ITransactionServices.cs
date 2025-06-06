// <copyright file="ITransactionServices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database
{
    using System;

    /// <summary>
    /// The Transaction Services
    /// </summary>
    public interface ITransactionServices : IDisposable
    {
        void OnInit(ITransaction transaction);

        T Get<T>();
    }
}
