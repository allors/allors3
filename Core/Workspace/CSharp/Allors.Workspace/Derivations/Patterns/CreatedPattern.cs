// <copyright file="CreatedPattern.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Workspace
{
    using Allors.Workspace.Meta;

    public class CreatedPattern : Pattern
    {
        public CreatedPattern(IComposite composite) => this.Composite = composite;

        public IComposite Composite { get; set; }
    }
}
