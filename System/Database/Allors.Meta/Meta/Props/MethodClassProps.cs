// <copyright file="MethodClassProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodClass type.</summary>

namespace Allors.Database.Meta
{
    public sealed partial class MethodClassProps : MetaObjectProps
    {
        private readonly IMethodClassBase associationClass;

        internal MethodClassProps(IMethodClassBase relationClass) => this.associationClass = relationClass;

        public override IMetaPopulation MetaPopulation => this.associationClass.MetaPopulation;

        public override Origin Origin => this.associationClass.Origin;
    }
}
