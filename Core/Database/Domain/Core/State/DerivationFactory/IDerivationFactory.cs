// <copyright file="IDerivationFactory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using Allors.Domain;

    public interface IDerivationFactory
    {
        IDerivation CreateDerivation(ISession session);
    }
}
