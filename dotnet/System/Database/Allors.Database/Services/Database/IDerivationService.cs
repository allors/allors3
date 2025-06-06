// <copyright file="IDerivationFactory.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Derivations
{
    using Database;

    public interface IDerivationService
    {
        IDerivation CreateDerivation(ITransaction transaction, bool continueOnError = false);
    }
}
