// <copyright file="Composite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ObjectType type.</summary>

namespace Allors.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Composite : ObjectType, IComposite
    {
        private string[] workspaceNames;

        private bool assignedIsSynced;
        private bool isSynced;

        private HashSet<Interface> derivedDirectSupertypes;
        private HashSet<Interface> derivedSupertypes;

        private HashSet<AssociationType> derivedAssociationTypes;
        private HashSet<RoleType> derivedRoleTypes;
        private HashSet<MethodType> derivedMethodTypes;

        private HashSet<AssociationType> derivedDatabaseAssociationTypes;
        private HashSet<RoleType> derivedDatabaseRoleTypes;

        private IReadOnlyDictionary<string, AssociationType[]> derivedWorkspaceAssociationTypesByWorkspaceName;
        private IReadOnlyDictionary<string, RoleType[]> derivedWorkspaceRoleTypesByWorkspaceName;
        private IReadOnlyDictionary<string, MethodType[]> derivedWorkspaceMethodTypesByWorkspaceName;

        protected Composite(MetaPopulation metaPopulation, Guid id)
            : base(metaPopulation, id)
        {
        }

        public string[] WorkspaceNames
        {
            get => this.workspaceNames ?? Array.Empty<string>();

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.workspaceNames = value;
                this.MetaPopulation.Stale();
            }
        }

        public override Origin Origin => this.AssignedOrigin;

        public Origin AssignedOrigin { get; set; }

        public bool AssignedIsSynced
        {
            get => this.assignedIsSynced;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.assignedIsSynced = value;
                this.MetaPopulation.Stale();
            }
        }

        public bool IsSynced
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.isSynced;
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

        public bool ExistDirectSupertypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDirectSupertypes.Count > 0;
            }
        }

        public bool ExistSupertypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedSupertypes.Count > 0;
            }
        }

        public bool ExistAssociationTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedAssociationTypes.Count > 0;
            }
        }

        public bool ExistRoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedAssociationTypes.Count > 0;
            }
        }

        public bool ExistMethodTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedMethodTypes.Count > 0;
            }
        }

        /// <summary>
        /// Gets the exclusive concrete subclass.
        /// </summary>
        /// <value>The exclusive concrete subclass.</value>
        public abstract Class ExclusiveClass { get; }

        /// <summary>
        /// Gets the root classes.
        /// </summary>
        /// <value>The root classes.</value>
        public abstract IEnumerable<Class> Classes { get; }

        public abstract IEnumerable<IClass> DatabaseClasses { get; }

        /// <summary>
        /// Gets the direct super types.
        /// </summary>
        /// <value>The super types.</value>
        public IEnumerable<Interface> DirectSupertypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDirectSupertypes;
            }
        }

        /// <summary>
        /// Gets the super types.
        /// </summary>
        /// <value>The super types.</value>
        public IEnumerable<Interface> Supertypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedSupertypes;
            }
        }

        /// <summary>
        /// Gets the associations.
        /// </summary>
        /// <value>The associations.</value>
        public IEnumerable<AssociationType> AssociationTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedAssociationTypes;
            }
        }

        public IEnumerable<AssociationType> ExclusiveAssociationTypes => this.AssociationTypes.Where(associationType => this.Equals(associationType.RoleType.ObjectType)).ToArray();

        public IEnumerable<AssociationType> ExclusiveDatabaseAssociationTypes => this.ExclusiveAssociationTypes.Where(v => v.Origin == Origin.Remote).ToArray();

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public IEnumerable<RoleType> RoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedRoleTypes;
            }
        }

        public IEnumerable<RoleType> UnitRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsUnit).ToArray();

        public IEnumerable<RoleType> UnitDatabaseRoleTypes => this.UnitRoleTypes.Where(v => v.Origin == Origin.Remote).ToArray();

        public IEnumerable<RoleType> CompositeRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsComposite).ToArray();

        public IEnumerable<RoleType> CompositeDatabaseRoleTypes => this.CompositeRoleTypes.Where(v => v.Origin == Origin.Remote).ToArray();

        public IEnumerable<RoleType> ExclusiveRoleTypes => this.RoleTypes.Where(roleType => this.Equals(roleType.AssociationType.ObjectType)).ToArray();

        public IEnumerable<RoleType> ExclusiveDatabaseRoleTypes => this.ExclusiveRoleTypes.Where(v => v.Origin == Origin.Remote).ToArray();

        public IEnumerable<RoleType> SortedExclusiveRoleTypes => this.ExclusiveRoleTypes.OrderBy(v => v.Name);


        /// <summary>
        /// Gets the method types.
        /// </summary>
        /// <value>The method types.</value>
        public IEnumerable<MethodType> MethodTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedMethodTypes;
            }
        }

        public IEnumerable<MethodType> ExclusiveMethodTypes => this.MethodTypes.Where(methodType => this.Equals(methodType.Composite)).ToArray();

        public IEnumerable<MethodType> InheritedMethodTypes => this.MethodTypes.Except(this.ExclusiveMethodTypes);

        public IEnumerable<RoleType> InheritedRoleTypes => this.RoleTypes.Except(this.ExclusiveRoleTypes);

        public IEnumerable<AssociationType> InheritedAssociationTypes => this.AssociationTypes.Except(this.ExclusiveAssociationTypes);

        public IEnumerable<RoleType> InheritedDatabaseRoleTypes => this.InheritedRoleTypes.Where(v => v.Origin == Origin.Remote);

        public IEnumerable<AssociationType> InheritedDatabaseAssociationTypes => this.InheritedAssociationTypes.Where(v => v.Origin == Origin.Remote);

        #region Workspace

        public IEnumerable<Composite> RelatedComposites
        {
            get
            {
                this.MetaPopulation.Derive();
                return this
                    .Supertypes
                    .Union(this.RoleTypes.Where(m => m.ObjectType.IsComposite).Select(v => (Composite)v.ObjectType))
                    .Union(this.AssociationTypes.Select(v => v.ObjectType)).Distinct()
                    .Except(new[] { this }).ToArray();
            }
        }

        public IEnumerable<RoleType> ExclusiveCompositeRoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.ExclusiveRoleTypes.Where(roleType => roleType.ObjectType.IsComposite);
            }
        }

        public abstract IEnumerable<Composite> Subtypes { get; }

        public abstract IEnumerable<Composite> DatabaseSubtypes { get; }

        public IEnumerable<RoleType> ExclusiveRoleTypesWithDatabaseOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.HasDatabaseOrigin);

        public IEnumerable<RoleType> ExclusiveRoleTypesWithWorkspaceOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.HasWorkspaceOrigin);

        public IEnumerable<RoleType> ExclusiveRoleTypesWithSessionOrigin => this.ExclusiveRoleTypes.Where(roleType => roleType.RelationType.HasSessionOrigin);

        public IEnumerable<AssociationType> ExclusiveAssociationTypesWithDatabaseOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.HasDatabaseOrigin);

        public IEnumerable<AssociationType> ExclusiveAssociationTypesWithWorkspaceOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.HasWorkspaceOrigin);

        public IEnumerable<AssociationType> ExclusiveAssociationTypesWithSessionOrigin => this.ExclusiveAssociationTypes.Where(roleType => roleType.RelationType.HasSessionOrigin);

        #endregion Workspace

        public IEnumerable<IAssociationType> DatabaseAssociationTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDatabaseAssociationTypes;
            }
        }

        public IEnumerable<IRoleType> DatabaseRoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDatabaseRoleTypes;
            }
        }

        public bool ExistDatabaseClass => this.DatabaseClasses.Any();

        public bool ExistExclusiveDatabaseClass => this.DatabaseClasses.Count() == 1;

        public IClass ExclusiveDatabaseClass => this.ExistExclusiveDatabaseClass ? this.DatabaseClasses.Single() : null;

        public IReadOnlyDictionary<string, AssociationType[]> WorkspaceAssociationTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceAssociationTypesByWorkspaceName;
            }
        }

        public IReadOnlyDictionary<string, RoleType[]> WorkspaceRoleTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceRoleTypesByWorkspaceName;
            }
        }

        public IReadOnlyDictionary<string, MethodType[]> WorkspaceMethodTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceMethodTypesByWorkspaceName;
            }
        }

        public bool ExistSupertype(IInterface @interface)
        {
            this.MetaPopulation.Derive();
            return this.derivedSupertypes.Contains(@interface);
        }

        public bool ExistAssociationType(IAssociationType associationType)
        {
            this.MetaPopulation.Derive();
            return this.derivedAssociationTypes.Contains(associationType);
        }

        public bool ExistRoleType(IRoleType roleType)
        {
            this.MetaPopulation.Derive();
            return this.derivedRoleTypes.Contains(roleType);
        }

        /// <summary>
        /// Contains this concrete class.
        /// </summary>
        /// <param name="objectType">
        /// The concrete class.
        /// </param>
        /// <returns>
        /// True if this contains the concrete class.
        /// </returns>
        public abstract bool IsAssignableFrom(IComposite objectType);

        /// <summary>
        /// Derive direct super type derivations.
        /// </summary>
        /// <param name="directSupertypes">The direct super types.</param>
        internal void DeriveDirectSupertypes(HashSet<Interface> directSupertypes)
        {
            directSupertypes.Clear();
            foreach (var inheritance in this.MetaPopulation.Inheritances.Where(inheritance => this.Equals(inheritance.Subtype)))
            {
                directSupertypes.Add(inheritance.Supertype);
            }

            this.derivedDirectSupertypes = new HashSet<Interface>(directSupertypes);
        }

        /// <summary>
        /// Derive super types.
        /// </summary>
        /// <param name="superTypes">The super types.</param>
        internal void DeriveSupertypes(HashSet<Interface> superTypes)
        {
            superTypes.Clear();

            this.DeriveSupertypesRecursively(this, superTypes);

            this.derivedSupertypes = new HashSet<Interface>(superTypes);
        }

        /// <summary>
        /// Derive role types.
        /// </summary>
        /// <param name="roleTypes">The role types.</param>
        /// <param name="roleTypesByAssociationObjectType">RoleTypes grouped by the ObjectType of the Association.</param>
        internal void DeriveRoleTypes(HashSet<RoleType> roleTypes, Dictionary<Composite, HashSet<RoleType>> roleTypesByAssociationObjectType)
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
                    if (this.IsInterface)
                    {
                        roleTypes.UnionWith(inheritedRoleTypes);
                    }
                    else
                    {
                        var roleClasses = inheritedRoleTypes.Select(v => v.RelationType.RoleClassBy((Class)this));
                        roleTypes.UnionWith(roleClasses);
                    }
                }
            }

            this.derivedRoleTypes = new HashSet<RoleType>(roleTypes);
            this.derivedDatabaseRoleTypes = new HashSet<RoleType>(roleTypes.Where(v => v.Origin == Origin.Remote));
            this.derivedWorkspaceRoleTypesByWorkspaceName = this.WorkspaceNames
                .ToDictionary(v => v, v => roleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).ToArray());

        }

        /// <summary>
        /// Derive method types.
        /// </summary>
        /// <param name="methodTypes">
        ///     The method types.
        /// </param>
        /// <param name="methodTypeByClass"></param>
        internal void DeriveMethodTypes(HashSet<MethodType> methodTypes, Dictionary<Composite, HashSet<MethodType>> methodTypeByClass)
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
                    if (this.IsInterface)
                    {
                        methodTypes.UnionWith(inheritedMethodTypes);
                    }
                    else
                    {
                        var methodClasses = inheritedMethodTypes.Select(v => v.MethodClassBy((Class)this));
                        methodTypes.UnionWith(methodClasses);
                    }
                }
            }

            this.derivedMethodTypes = new HashSet<MethodType>(methodTypes);
            this.derivedWorkspaceMethodTypesByWorkspaceName = this.WorkspaceNames
                .ToDictionary(v => v, v => methodTypes.Where(w => w.WorkspaceNames.Contains(v)).ToArray());

        }

        /// <summary>
        /// Derive association types.
        /// </summary>
        /// <param name="associationTypes">The associations.</param>
        /// <param name="relationTypesByRoleObjectType">AssociationTypes grouped by the ObjectType of the Role.</param>
        internal void DeriveAssociationTypes(HashSet<AssociationType> associationTypes, Dictionary<ObjectType, HashSet<AssociationType>> relationTypesByRoleObjectType)
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

            this.derivedAssociationTypes = new HashSet<AssociationType>(associationTypes);
            this.derivedDatabaseAssociationTypes = new HashSet<AssociationType>(associationTypes.Where(v => v.Origin == Origin.Remote));
            this.derivedWorkspaceAssociationTypesByWorkspaceName = this.WorkspaceNames
                .ToDictionary(v => v, v => associationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)).ToArray());
        }

        internal void DeriveIsSynced() => this.isSynced = this.assignedIsSynced || this.derivedSupertypes.Any(v => v.assignedIsSynced);

        /// <summary>
        /// Derive super types recursively.
        /// </summary>
        /// <param name="type">The type .</param>
        /// <param name="superTypes">The super types.</param>
        private void DeriveSupertypesRecursively(ObjectType type, HashSet<Interface> superTypes)
        {
            foreach (var directSupertype in this.derivedDirectSupertypes)
            {
                if (!Equals(directSupertype, type))
                {
                    superTypes.Add(directSupertype);
                    directSupertype.DeriveSupertypesRecursively(type, superTypes);
                }
            }
        }
    }
}
