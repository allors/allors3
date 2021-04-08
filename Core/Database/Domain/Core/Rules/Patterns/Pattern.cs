// <copyright file="IPattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Derivations
{
    using System;
    using Data;
    using Meta;

    public abstract class Pattern : IPattern
    {
        public IPropertyType[] Steps
        {
            private get => throw new NotSupportedException();

            set => this.Path = new Path(value);
        }

        public Path Path { get; set; }

        public IComposite OfType { get; set; }

        public abstract IComposite ObjectType { get; }
    }
}
