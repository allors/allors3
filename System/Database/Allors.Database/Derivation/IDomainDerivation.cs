// <copyright file="IDomainDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors
{
    using System;
    using System.Collections.Generic;

    public interface IDomainDerivation
    {
        Guid Id { get; }

        Pattern[] Patterns { get; }

        void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches);
    }
}
