// <copyright file="Class.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class Class : Composite, IClass
    {
        private readonly Class[] classes;

        private Type clrType;

        internal Class(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id)
        {
            this.classes = new[] { this };
            metaPopulation.OnClassCreated(this);
        }

        // TODO: Review
        public RoleType[] DelegatedAccessRoleTypes { get; set; }

        public override IEnumerable<Class> Classes => this.classes;

        public override IEnumerable<IClass> DatabaseClasses => this.Origin == Origin.Remote ? this.classes : Array.Empty<Class>();

        public override bool ExistClass => true;

        public override Class ExclusiveClass => this;

        public override Type ClrType => this.clrType;

        public override IEnumerable<Composite> Subtypes => new[] { this };

        public override IEnumerable<Composite> DatabaseSubtypes => this.Origin == Origin.Remote ? this.Subtypes : Array.Empty<Composite>();

        public override bool IsAssignableFrom(IComposite objectType) => this.Equals(objectType);

        internal void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];
    }
}
