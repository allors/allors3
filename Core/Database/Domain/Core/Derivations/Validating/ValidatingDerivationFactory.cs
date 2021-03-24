// <copyright file="DefaultDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Validating
{
    using Database;
    using Domain;
    using Domain.Derivations.Validating;

    public class ValidatingDerivationFactory : IDerivationFactory
    {
        public IDerivation CreateDerivation(ITransaction transaction) => new ValidatingDerivation(transaction);
    }
}
