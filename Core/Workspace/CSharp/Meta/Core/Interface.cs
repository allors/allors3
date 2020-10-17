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

    public sealed partial class Interface : Composite, IInterface
    {
        private string[] derivedWorkspaceNames;

        private HashSet<Composite> derivedDirectSubtypes;

        private HashSet<Composite> derivedSubtypes;
        private HashSet<Composite> derivedDatabaseSubtypes;

        private HashSet<Class> derivedClasses;

        private HashSet<Class> derivedDatabaseClasses;

        private Class derivedExclusiveClass;

        private Type clrType;

        internal Interface(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id) => metaPopulation.OnInterfaceCreated(this);

        public override string[] WorkspaceNames
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        #region Exist

        public bool ExistClasses
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedClasses.Count > 0;
            }
        }

        public bool ExistSubtypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedSubtypes.Count > 0;
            }
        }

        public override bool ExistClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedClasses.Count > 0;
            }
        }

        #endregion Exist

        /// <summary>
        /// Gets the subclasses.
        /// </summary>
        /// <value>The subclasses.</value>
        public override IEnumerable<Class> Classes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedClasses;
            }
        }

        /// <summary>
        /// Gets the sub types.
        /// </summary>
        /// <value>The super types.</value>
        public override IEnumerable<Composite> Subtypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedSubtypes;
            }
        }

        public override IEnumerable<Composite> DatabaseSubtypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDatabaseSubtypes;
            }
        }

        public override Class ExclusiveClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedExclusiveClass;
            }
        }

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
        public override bool IsAssignableFrom(IComposite objectType)
        {
            this.MetaPopulation.Derive();
            return this.Equals(objectType) || this.derivedSubtypes.Contains(objectType);
        }

        internal void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];

        internal void DeriveWorkspaceNames() =>
            this.derivedWorkspaceNames = this
                .RoleTypes.SelectMany(v => v.RelationType.WorkspaceNames)
                .Union(this.AssociationTypes.SelectMany(v=>v.RelationType.WorkspaceNames))
                .Union(this.MethodTypes.SelectMany(v=>v.WorkspaceNames))
                .ToArray();

        /// <summary>
        /// Derive direct sub type derivations.
        /// </summary>
        /// <param name="directSubtypes">The direct super types.</param>
        internal void DeriveDirectSubtypes(HashSet<Composite> directSubtypes)
        {
            directSubtypes.Clear();
            foreach (var inheritance in this.MetaPopulation.Inheritances.Where(inheritance => this.Equals(inheritance.Supertype)))
            {
                directSubtypes.Add(inheritance.Subtype);
            }

            this.derivedDirectSubtypes = new HashSet<Composite>(directSubtypes);
        }

        /// <summary>
        /// Derive subclasses.
        /// </summary>
        /// <param name="subClasses">The sub classes.</param>
        internal void DeriveSubclasses(HashSet<Class> subClasses)
        {
            subClasses.Clear();
            foreach (var subType in this.derivedSubtypes)
            {
                if (subType is IClass)
                {
                    subClasses.Add((Class)subType);
                }
            }

            this.derivedClasses = new HashSet<Class>(subClasses);
            this.derivedDatabaseClasses = new HashSet<Class>(subClasses.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive sub types.
        /// </summary>
        /// <param name="subTypes">The super types.</param>
        internal void DeriveSubtypes(HashSet<Composite> subTypes)
        {
            subTypes.Clear();
            this.DeriveSubtypesRecursively(this, subTypes);

            this.derivedSubtypes = new HashSet<Composite>(subTypes);
            this.derivedDatabaseSubtypes = new HashSet<Composite>(subTypes.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive exclusive sub classes.
        /// </summary>
        internal void DeriveExclusiveSubclass() => this.derivedExclusiveClass = this.derivedClasses.Count == 1 ? this.derivedClasses.First() : null;

        /// <summary>
        /// Derive super types recursively.
        /// </summary>
        /// <param name="type">The type .</param>
        /// <param name="subTypes">The super types.</param>
        private void DeriveSubtypesRecursively(ObjectType type, HashSet<Composite> subTypes)
        {
            foreach (var directSubtype in this.derivedDirectSubtypes)
            {
                if (!Equals(directSubtype, type))
                {
                    subTypes.Add(directSubtype);
                    if (directSubtype is IInterface)
                    {
                        ((Interface)directSubtype).DeriveSubtypesRecursively(this, subTypes);
                    }
                }
            }
        }

        public override IEnumerable<IClass> DatabaseClasses
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDatabaseClasses;
            }
        }
    }
}
