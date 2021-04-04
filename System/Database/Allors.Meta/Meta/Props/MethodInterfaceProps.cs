// <copyright file="MethodInterfaceProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodInterface type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class MethodInterfaceProps : MetaObjectProps
    {
        private readonly IMethodInterfaceBase associationInterface;

        internal MethodInterfaceProps(IMethodInterfaceBase relationInterface) => this.associationInterface = relationInterface;

        public override IMetaPopulation MetaPopulation => this.associationInterface.MetaPopulation;

        public override Origin Origin => this.associationInterface.Origin;
    }
}
