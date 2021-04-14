// <copyright file="Class.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;

    public abstract partial class Class : Composite, IClass
    {
        private readonly Class[] classes;
        private Type clrType;

        internal Class(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id) => this.classes = new[] { this };

        public MetaPopulation M => this.MetaPopulation;

        public override IEnumerable<Class> Classes => this.classes;

        public override IEnumerable<IClass> DatabaseClasses => this.Origin == Origin.Database ? this.classes : Array.Empty<Class>();

        public override bool ExistClass => true;

        public override Type ClrType => this.clrType;
        
        public override bool IsAssignableFrom(IComposite objectType) => this.Equals(objectType);

        internal void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];
    }
}
