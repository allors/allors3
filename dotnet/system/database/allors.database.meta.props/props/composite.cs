// <copyright file="Composite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ObjectType type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Composite : ObjectType, ICompositeBase
    {
        private readonly IMetaPopulationBase metaPopulation;

        private HashSet<IInterfaceBase> structuralDerivedDirectSupertypes;
        private HashSet<IInterfaceBase> structuralDerivedSupertypes;

        private HashSet<IAssociationTypeBase> structuralDerivedAssociationTypes;
        private HashSet<IAssociationTypeBase> structuralDerivedDatabaseAssociationTypes;
        private HashSet<IRoleTypeBase> structuralDerivedRoleTypes;
        private HashSet<IRoleTypeBase> structuralDerivedDatabaseRoleTypes;

        private HashSet<IMethodTypeBase> structuralDerivedMethodTypes;

        private bool? assignedIsRelationship;
        private bool isRelationship;

        protected Composite(IMetaPopulationBase metaPopulation, Guid id, string tag) : base(metaPopulation, id, tag)
        {
            this.metaPopulation = metaPopulation;
            this.AssignedOrigin = Origin.Database;
        }

        public override Origin Origin => this.AssignedOrigin;

        public Origin AssignedOrigin { get; set; }

        public bool? AssignedIsRelationship
        {
            get => this.assignedIsRelationship;

            set
            {
                this.metaPopulation.AssertUnlocked();
                this.assignedIsRelationship = value;
                this.metaPopulation.Stale();
            }
        }

        public bool IsRelationship
        {
            get
            {
                this.metaPopulation.Derive();
                return this.isRelationship;
            }
        }

        public bool ExistExclusiveClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.ExclusiveClass != null;
            }
        }

        public abstract bool ExistClass { get; }

        /// <summary>
        /// Gets the exclusive concrete subclass.
        /// </summary>
        /// <value>The exclusive concrete subclass.</value>
        public abstract IClassBase ExclusiveClass { get; }

        /// <summary>
        /// Gets the root classes.
        /// </summary>
        /// <value>The root classes.</value>
        public abstract IEnumerable<IClassBase> Classes { get; }

        public abstract IEnumerable<IClass> DatabaseClasses { get; }

        public IEnumerable<IInterfaceBase> DirectSupertypes => this.structuralDerivedDirectSupertypes;

        IEnumerable<IInterface> IComposite.Supertypes => this.Supertypes;

        /// <summary>
        /// Gets the super types.
        /// </summary>
        /// <value>The super types.</value>
        public IEnumerable<IInterfaceBase> Supertypes => this.structuralDerivedSupertypes;

        IEnumerable<IAssociationType> ICompositeBase.AssociationTypes => this.AssociationTypes;
        public IEnumerable<IAssociationTypeBase> AssociationTypes => this.structuralDerivedAssociationTypes;

        public IEnumerable<IAssociationTypeBase> ExclusiveAssociationTypes => this.AssociationTypes.Where(associationType => this.Equals(associationType.RoleType.ObjectType)).ToArray();

        IEnumerable<IAssociationType> IComposite.ExclusiveDatabaseAssociationTypes => this.ExclusiveDatabaseAssociationTypes;
        public IEnumerable<IAssociationTypeBase> ExclusiveDatabaseAssociationTypes => this.ExclusiveAssociationTypes.Where(v => v.Origin == Origin.Database).ToArray();

        IEnumerable<IRoleType> ICompositeBase.RoleTypes => this.RoleTypes;
        public IEnumerable<IRoleTypeBase> RoleTypes => this.structuralDerivedRoleTypes;

        public IEnumerable<IRoleTypeBase> ExclusiveRoleTypes => this.RoleTypes.Where(roleType => this.Equals(roleType.AssociationType.ObjectType)).ToArray();

        IEnumerable<IRoleType> IComposite.ExclusiveDatabaseRoleTypes => this.ExclusiveDatabaseRoleTypes;
        public IEnumerable<IRoleTypeBase> ExclusiveDatabaseRoleTypes => this.ExclusiveRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        IEnumerable<IMethodType> IComposite.MethodTypes => this.MethodTypes;

        /// <summary>
        /// Gets the method types.
        /// </summary>
        /// <value>The method types.</value>
        public IEnumerable<IMethodTypeBase> MethodTypes => this.structuralDerivedMethodTypes;

        IEnumerable<IMethodType> IComposite.ExclusiveMethodTypes => this.ExclusiveMethodTypes;
        public IEnumerable<IMethodTypeBase> ExclusiveMethodTypes => this.MethodTypes.Where(methodType => this.Equals(methodType.ObjectType)).ToArray();

        IEnumerable<IMethodType> IComposite.InheritedMethodTypes => this.InheritedMethodTypes;
        public IEnumerable<IMethodTypeBase> InheritedMethodTypes => this.MethodTypes.Except(this.ExclusiveMethodTypes);

        IEnumerable<IRoleType> IComposite.InheritedRoleTypes => this.InheritedRoleTypes;
        public IEnumerable<IRoleTypeBase> InheritedRoleTypes => this.RoleTypes.Except(this.ExclusiveRoleTypes);

        IEnumerable<IAssociationType> IComposite.InheritedAssociationTypes => this.InheritedAssociationTypes;
        public IEnumerable<IAssociationTypeBase> InheritedAssociationTypes => this.AssociationTypes.Except(this.ExclusiveAssociationTypes);

        public IEnumerable<IRoleTypeBase> InheritedDatabaseRoleTypes => this.InheritedRoleTypes.Where(v => v.Origin == Origin.Database);

        public IEnumerable<IAssociationTypeBase> InheritedDatabaseAssociationTypes => this.InheritedAssociationTypes.Where(v => v.Origin == Origin.Database);

        #region Workspace

        public IEnumerable<IRoleTypeBase> ExclusiveCompositeRoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.ExclusiveRoleTypes.Where(roleType => roleType.ObjectType.IsComposite);
            }
        }

        IEnumerable<IComposite> IComposite.Subtypes => this.Subtypes;
        public abstract IEnumerable<ICompositeBase> Subtypes { get; }

        public abstract IEnumerable<ICompositeBase> DatabaseSubtypes { get; }

        public IEnumerable<IRoleTypeBase> ExclusiveRoleTypesWithDatabaseOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.Origin == Origin.Database);

        public IEnumerable<IRoleTypeBase> ExclusiveRoleTypesWithSessionOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.Origin == Origin.Session);

        public IEnumerable<IAssociationTypeBase> ExclusiveAssociationTypesWithDatabaseOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.Origin == Origin.Database);

        public IEnumerable<IAssociationTypeBase> ExclusiveAssociationTypesWithSessionOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.Origin == Origin.Session);

        #endregion Workspace

        public IEnumerable<IAssociationType> DatabaseAssociationTypes => this.structuralDerivedDatabaseAssociationTypes;

        public IEnumerable<IRoleType> DatabaseRoleTypes => this.structuralDerivedDatabaseRoleTypes;

        public bool ExistDatabaseClass => this.DatabaseClasses.Any();

        public bool ExistExclusiveDatabaseClass => this.DatabaseClasses.Count() == 1;

        public IClass ExclusiveDatabaseClass => this.ExistExclusiveDatabaseClass ? this.DatabaseClasses.Single() : null;

        IEnumerable<IClass> IComposite.Classes => this.Classes;

        public bool ExistSupertype(IInterface @interface) => this.structuralDerivedSupertypes.Contains(@interface);

        public bool ExistAssociationType(IAssociationType associationType) => this.structuralDerivedAssociationTypes.Contains(associationType);

        public bool ExistRoleType(IRoleType roleType) => this.structuralDerivedRoleTypes.Contains(roleType);

        public abstract bool IsAssignableFrom(IComposite objectType);

        public abstract void Bind(Dictionary<string, Type> typeByName);

        public void DeriveIsRelationship() =>
            this.isRelationship = this.assignedIsRelationship ?? this.Supertypes.Any(v => v.AssignedIsRelationship == true);

        /// <summary>
        /// Derive direct super type derivations.
        /// </summary>
        /// <param name="directSupertypes">The direct super types.</param>
        public void StructuralDeriveDirectSupertypes(HashSet<IInterfaceBase> directSupertypes)
        {
            directSupertypes.Clear();
            foreach (var inheritance in this.MetaPopulation.Inheritances.Where(inheritance => this.Equals(inheritance.Subtype)))
            {
                directSupertypes.Add(inheritance.Supertype);
            }

            this.structuralDerivedDirectSupertypes = new HashSet<IInterfaceBase>(directSupertypes);
        }

        /// <summary>
        /// Derive super types.
        /// </summary>
        /// <param name="superTypes">The super types.</param>
        public void StructuralDeriveSupertypes(HashSet<IInterfaceBase> superTypes)
        {
            superTypes.Clear();

            this.StructuralDeriveSupertypesRecursively(this, superTypes);

            this.structuralDerivedSupertypes = new HashSet<IInterfaceBase>(superTypes);
        }

        /// <summary>
        /// Derive role types.
        /// </summary>
        /// <param name="roleTypes">The role types.</param>
        /// <param name="roleTypesByAssociationObjectType">RoleTypes grouped by the ObjectType of the Association.</param>
        public void StructuralDeriveRoleTypes(HashSet<IRoleTypeBase> roleTypes, Dictionary<ICompositeBase, HashSet<IRoleTypeBase>> roleTypesByAssociationObjectType)
        {
            roleTypes.Clear();

            if (roleTypesByAssociationObjectType.TryGetValue(this, out var directRoleTypes))
            {
                roleTypes.UnionWith(directRoleTypes);
            }

            foreach (var superType in this.Supertypes)
            {
                if (roleTypesByAssociationObjectType.TryGetValue(superType, out var inheritedRoleTypes))
                {
                    roleTypes.UnionWith(inheritedRoleTypes);
                }
            }

            this.structuralDerivedRoleTypes = new HashSet<IRoleTypeBase>(roleTypes);
            this.structuralDerivedDatabaseRoleTypes = new HashSet<IRoleTypeBase>(roleTypes.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive association types.
        /// </summary>
        /// <param name="associationTypes">The associations.</param>
        /// <param name="relationTypesByRoleObjectType">AssociationTypes grouped by the ObjectType of the Role.</param>
        public void StructuralDeriveAssociationTypes(HashSet<IAssociationTypeBase> associationTypes, Dictionary<IObjectTypeBase, HashSet<IAssociationTypeBase>> relationTypesByRoleObjectType)
        {
            associationTypes.Clear();

            if (relationTypesByRoleObjectType.TryGetValue(this, out var classAssociationTypes))
            {
                associationTypes.UnionWith(classAssociationTypes);
            }

            foreach (var superType in this.Supertypes)
            {
                if (relationTypesByRoleObjectType.TryGetValue(superType, out var interfaceAssociationTypes))
                {
                    associationTypes.UnionWith(interfaceAssociationTypes);
                }
            }

            this.structuralDerivedAssociationTypes = new HashSet<IAssociationTypeBase>(associationTypes);
            this.structuralDerivedDatabaseAssociationTypes = new HashSet<IAssociationTypeBase>(associationTypes.Where(v => v.Origin == Origin.Database));
        }

        /// <summary>
        /// Derive method types.
        /// </summary>
        /// <param name="methodTypes">
        ///     The method types.
        /// </param>
        /// <param name="methodTypeByClass"></param>
        public void StructuralDeriveMethodTypes(HashSet<IMethodTypeBase> methodTypes, Dictionary<ICompositeBase, HashSet<IMethodTypeBase>> methodTypeByClass)
        {
            methodTypes.Clear();

            if (methodTypeByClass.TryGetValue(this, out var directMethodTypes))
            {
                methodTypes.UnionWith(directMethodTypes);
            }

            foreach (var superType in this.Supertypes)
            {
                if (methodTypeByClass.TryGetValue(superType, out var inheritedMethodTypes))
                {
                    methodTypes.UnionWith(inheritedMethodTypes);
                }
            }

            this.structuralDerivedMethodTypes = new HashSet<IMethodTypeBase>(methodTypes);
        }

        /// <summary>
        /// Derive super types recursively.
        /// </summary>
        /// <param name="type">The type .</param>
        /// <param name="superTypes">The super types.</param>
        public void StructuralDeriveSupertypesRecursively(IObjectTypeBase type, HashSet<IInterfaceBase> superTypes)
        {
            foreach (var directSupertype in this.DirectSupertypes)
            {
                if (!Equals(directSupertype, type))
                {
                    superTypes.Add(directSupertype);
                    directSupertype.StructuralDeriveSupertypesRecursively(type, superTypes);
                }
            }
        }
    }
}
