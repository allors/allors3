// <copyright file="IDomainDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Domain.Derivations.Rules
{
    using System;
    using System.Collections.Generic;
    using Derivations.Rules;

    public interface IRule
    {
        Guid Id { get; }

        IPattern[] Patterns { get; }

        void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches);
    }
}
