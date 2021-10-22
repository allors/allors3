// <copyright file="Interface.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Interface : Composite, IInterfaceBase
    {
        private string[] derivedWorkspaceNames;

        private HashSet<ICompositeBase> structuralDerivedDirectSubtypes;
        private HashSet<ICompositeBase> structuralDerivedSubtypes;
        private HashSet<ICompositeBase> structuralDerivedDatabaseSubtypes;
        private HashSet<IClassBase> structuralDerivedClasses;
        private HashSet<IClassBase> structuralDerivedDatabaseClasses;
        private IClassBase structuralDerivedExclusiveClass;

        private Type clrType;

        internal Interface(IMetaPopulationBase metaPopulation, Guid id, string tag) : base(metaPopulation, id, tag) => metaPopulation.OnInterfaceCreated(this);

        public MetaPopulation M => (MetaPopulation)this.MetaPopulation;

        public override IEnumerable<string> WorkspaceNames
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        public bool ExistClasses => this.structuralDerivedClasses.Count > 0;

        public bool ExistSubtypes => this.structuralDerivedSubtypes.Count > 0;

        public override bool ExistClass => this.structuralDerivedClasses.Count > 0;

        /// <summary>
        /// Gets the subclasses.
        /// </summary>
        /// <value>The subclasses.</value>
        public override IEnumerable<IClassBase> Classes => this.structuralDerivedClasses;

        /// <summary>
        /// Gets the sub types.
        /// </summary>
        /// <value>The super types.</value>
        IEnumerable<IComposite> IInterface.Subtypes => this.Subtypes;
        public override IEnumerable<ICompositeBase> Subtypes => this.structuralDerivedSubtypes;

        public override IEnumerable<ICompositeBase> DatabaseSubtypes => this.structuralDerivedDatabaseSubtypes;

        public IEnumerable<Interface> Subinterfaces => this.Subtypes.OfType<Interface>();

        public override IClassBase ExclusiveClass => this.structuralDerivedExclusiveClass;

        public override Type ClrType => this.clrType;

        /// <summary>
        /// Contains this concrete class.
        /// </summary>
        /// <param name="objectType">
        /// The concrete class.
        /// </param>
        /// <returns>
        /// True if this contains the concrete class.
        /// </returns>
        public override bool IsAssignableFrom(IComposite objectType) => this.Equals(objectType) || this.structuralDerivedSubtypes.Contains(objectType);

        public override void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];

        public void DeriveWorkspaceNames() =>
            this.derivedWorkspaceNames = this
                .RoleTypes.SelectMany(v => v.RelationType.WorkspaceNames)
                .Union(this.AssociationTypes.SelectMany(v => v.RelationType.WorkspaceNames))
                .Union(this.MethodTypes.SelectMany(v => v.WorkspaceNames))
                .ToArray();

        /// <summary>
        /// Derive direct sub type derivations.
        /// </summary>
        /// <param name="directSubtypes">The direct super types.</param>
        public void StructuralDeriveDirectSubtypes(HashSet<ICompositeBase> directSubtypes)
        {
            directSubtypes.Clear();
            foreach (var inheritance in this.MetaPopulation.Inheritances.Where(inheritance => this.Equals(inheritance.Supertype)))
            {
                directSubtypes.Add(inheritance.Subtype);
            }

            this.structuralDerivedDirectSubtypes = new HashSet<ICompositeBase>(directSubtypes);
        }

        /// <summary>
        /// Derive subclasses.
        /// </summary>
        /// <param name="subClasses">The sub classes.</param>
        public void StructuralDeriveSubclasses(HashSet<IClassBase> subClasses)
        {
            subClasses.Clear();
            foreach (var subType in this.structuralDerivedSubtypes)
            {
                if (subType is IClass)
                {
                    subClasses.Add((Class)subType);
                }
            }

            this.structuralDerivedClasses = new HashSet<IClassBase>(subClasses);
            this.structuralDerivedDatabaseClasses = new HashSet<IClassBase>(subClasses.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive sub types.
        /// </summary>
        /// <param name="subTypes">The super types.</param>
        public void StructuralDeriveSubtypes(HashSet<ICompositeBase> subTypes)
        {
            subTypes.Clear();
            this.StructuralDeriveSubtypesRecursively(this, subTypes);

            this.structuralDerivedSubtypes = new HashSet<ICompositeBase>(subTypes);
            this.structuralDerivedDatabaseSubtypes = new HashSet<ICompositeBase>(subTypes.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive exclusive sub classes.
        /// </summary>
        public void StructuralDeriveExclusiveSubclass() => this.structuralDerivedExclusiveClass = this.structuralDerivedClasses.Count == 1 ? this.structuralDerivedClasses.First() : null;

        /// <summary>
        /// Derive super types recursively.
        /// </summary>
        /// <param name="type">The type .</param>
        /// <param name="subTypes">The super types.</param>
        public void StructuralDeriveSubtypesRecursively(IObjectTypeBase type, HashSet<ICompositeBase> subTypes)
        {
            foreach (var directSubtype in this.structuralDerivedDirectSubtypes)
            {
                if (!Equals(directSubtype, type))
                {
                    subTypes.Add(directSubtype);
                    if (directSubtype is IInterface)
                    {
                        ((Interface)directSubtype).StructuralDeriveSubtypesRecursively(this, subTypes);
                    }
                }
            }
        }

        public override IEnumerable<IClass> DatabaseClasses => this.structuralDerivedDatabaseClasses;
    }
}
