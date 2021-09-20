// <copyright file="IDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable StyleCop.SA1121
namespace Allors.Database.Domain.Derivations.Legacy
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Derivations;
    using Object = Object;

    public interface ILegacyDerivation : IDerivation
    {
        object this[string name] { get; set; }

        ISet<Object> DerivedObjects { get; }

        ILegacyCycle LegacyCycle { get; }

        void Mark(Object @object);

        void Mark(params Object[] objects);
    }
}
