// <copyright file="Composite.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract partial class Composite : ObjectType, IComposite
    {
        internal HashSet<Interface> directSupertypes;
        internal HashSet<Interface> supertypes;
        
        // TODO:

        private HashSet<AssociationType> derivedAssociationTypes;
        private HashSet<RoleType> derivedRoleTypes;
        private HashSet<MethodType> derivedMethodTypes;

        private HashSet<AssociationType> derivedDatabaseAssociationTypes;
        private HashSet<RoleType> derivedDatabaseRoleTypes;

        private HashSet<RoleType> derivedWorkspaceRoleTypes;

        protected Composite(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id) => this.AssignedOrigin = Origin.Database;

        //public Dictionary<string, bool> Workspace => this.WorkspaceNames.ToDictionary(k => k, v => true);

        public override Origin Origin => this.AssignedOrigin;

        public Origin AssignedOrigin { get; set; }
        
        public abstract bool ExistClass { get; }

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
        public IEnumerable<Interface> DirectSupertypes => this.directSupertypes;

        /// <summary>
        /// Gets the super types.
        /// </summary>
        /// <value>The super types.</value>
        public IEnumerable<Interface> Supertypes => this.supertypes;

        /// <summary>
        /// Gets the associations.
        /// </summary>
        /// <value>The associations.</value>
        public IEnumerable<AssociationType> AssociationTypes
        {
            get
            {
                return this.derivedAssociationTypes;
            }
        }

        public IEnumerable<AssociationType> ExclusiveAssociationTypes => this.AssociationTypes.Where(associationType => this.Equals(associationType.RoleType.ObjectType)).ToArray();

        public IEnumerable<AssociationType> ExclusiveDatabaseAssociationTypes => this.ExclusiveAssociationTypes.Where(v => v.Origin == Origin.Database).ToArray();

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <value>The roles.</value>
        public IEnumerable<RoleType> RoleTypes => this.derivedRoleTypes;

        public IEnumerable<RoleType> UnitRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsUnit).ToArray();

        public IEnumerable<RoleType> UnitDatabaseRoleTypes => this.UnitRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        public IEnumerable<RoleType> CompositeRoleTypes => this.RoleTypes.Where(roleType => roleType.ObjectType.IsComposite).ToArray();

        public IEnumerable<RoleType> CompositeDatabaseRoleTypes => this.CompositeRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        public IEnumerable<RoleType> ExclusiveRoleTypes => this.RoleTypes.Where(roleType => this.Equals(roleType.AssociationType.ObjectType)).ToArray();

        public IEnumerable<RoleType> ExclusiveDatabaseRoleTypes => this.ExclusiveRoleTypes.Where(v => v.Origin == Origin.Database).ToArray();

        public IEnumerable<RoleType> SortedExclusiveRoleTypes => this.ExclusiveRoleTypes.OrderBy(v => v.Name);


        /// <summary>
        /// Gets the method types.
        /// </summary>
        /// <value>The method types.</value>
        public IEnumerable<MethodType> MethodTypes => this.derivedMethodTypes;

        public IEnumerable<MethodType> ExclusiveMethodTypes => this.MethodTypes.Where(methodType => this.Equals(methodType.Composite)).ToArray();

        public IEnumerable<MethodType> InheritedMethodTypes => this.MethodTypes.Except(this.ExclusiveMethodTypes);

        public IEnumerable<RoleType> InheritedRoleTypes => this.RoleTypes.Except(this.ExclusiveRoleTypes);

        public IEnumerable<AssociationType> InheritedAssociationTypes => this.AssociationTypes.Except(this.ExclusiveAssociationTypes);

        public IEnumerable<RoleType> InheritedDatabaseRoleTypes => this.InheritedRoleTypes.Where(v => v.Origin == Origin.Database);

        public IEnumerable<AssociationType> InheritedDatabaseAssociationTypes => this.InheritedAssociationTypes.Where(v => v.Origin == Origin.Database);
        
        public IEnumerable<IAssociationType> DatabaseAssociationTypes => this.derivedDatabaseAssociationTypes;

        public IEnumerable<IRoleType> DatabaseRoleTypes => this.derivedDatabaseRoleTypes;

        public IEnumerable<IRoleType> WorkspaceRoleTypes => this.derivedWorkspaceRoleTypes;


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
                    roleTypes.UnionWith(inheritedRoleTypes);
                }
            }

            this.derivedRoleTypes = new HashSet<RoleType>(roleTypes);
            this.derivedDatabaseRoleTypes = new HashSet<RoleType>(roleTypes.Where(v => v.Origin == Origin.Database));
            this.derivedWorkspaceRoleTypes = new HashSet<RoleType>(roleTypes.Where(v => v.Origin == Origin.Workspace));
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
            this.derivedDatabaseAssociationTypes = new HashSet<AssociationType>(associationTypes.Where(v => v.Origin == Origin.Database));
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
                    methodTypes.UnionWith(inheritedMethodTypes);
                }
            }

            this.derivedMethodTypes = new HashSet<MethodType>(methodTypes);
        }
    }
}
