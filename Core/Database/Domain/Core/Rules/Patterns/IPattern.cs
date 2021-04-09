// <copyright file="IDomainDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using Data;
    using Meta;

    public interface IPattern
    {
        Path Path { get; }

        IComposite OfType { get; }

        IComposite ObjectType { get; }
    }
}