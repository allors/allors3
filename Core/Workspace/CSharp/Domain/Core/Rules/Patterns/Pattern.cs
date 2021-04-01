// <copyright file="Pattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDerivation type.</summary>

namespace Allors.Workspace
{
    using Derivations;
    using Meta;

    public abstract class Pattern
    {
        public IPropertyType[] Steps { get; set; }

        public IComposite OfType { get; set; }
    }
}
