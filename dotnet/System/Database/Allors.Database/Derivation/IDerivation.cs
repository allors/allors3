// <copyright file="IDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable StyleCop.SA1121
namespace Allors.Database.Derivations
{
    using System;

    public interface IDerivation
    {
        Guid Id { get; }

        DateTime TimeStamp { get; }

        ITransaction Transaction { get; }

        IValidation Validation { get; }

        IAccumulatedChangeSet ChangeSet { get; }

        IValidation Derive();

        object this[string key] { get; set; }
    }
}
