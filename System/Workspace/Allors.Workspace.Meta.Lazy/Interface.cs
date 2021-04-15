// <copyright file="Interface.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class Interface : Composite, IInterface
    {
        internal HashSet<Composite> directSubtypes;
        internal HashSet<Composite> subtypes;
        internal HashSet<Class> classes;

        private Type clrType;

        protected Interface(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id) { }

        public MetaPopulation M => this.MetaPopulation;

        IEnumerable<IComposite> IInterface.DirectSubtypes => this.DirectSubtypes;
        public IEnumerable<Composite> DirectSubtypes => this.directSubtypes;

        IEnumerable<IComposite> IInterface.Subtypes => this.Subtypes;
        public IEnumerable<Composite> Subtypes => this.subtypes;

        /// <summary>
        /// Gets the subclasses.
        /// </summary>
        /// <value>The subclasses.</value>
        public override IEnumerable<Class> Classes => this.classes;

        public override Type ClrType => this.clrType;

        public override bool ExistClass => this.classes.Count > 0;

        /// <summary>
        /// Contains this concrete class.
        /// </summary>
        /// <param name="objectType">
        /// The concrete class.
        /// </param>
        /// <returns>
        /// True if this contains the concrete class.
        /// </returns>
        public override bool IsAssignableFrom(IComposite objectType) => this.Equals(objectType) || this.subtypes.Contains(objectType);

        internal void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];
    }
}
