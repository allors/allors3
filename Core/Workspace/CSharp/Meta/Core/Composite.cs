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

        private HashSet<AssociationType> associationTypes;
        private HashSet<RoleType> roleTypes;
        private HashSet<RoleType> workspaceRoleTypes;
        private HashSet<RoleType> databaseRoleTypes;
        private HashSet<MethodType> methodTypes;
        
        protected Composite(MetaPopulation metaPopulation, Guid id) : base(metaPopulation, id) => this.AssignedOrigin = Origin.Database;

        //public Dictionary<string, bool> Workspace => this.WorkspaceNames.ToDictionary(k => k, v => true);

        public override Origin Origin => this.AssignedOrigin;

        public Origin AssignedOrigin { get; set; }

        public abstract bool ExistClass { get; }

        IEnumerable<IClass> IComposite.Classes => this.Classes;
        public abstract IEnumerable<Class> Classes { get; }

        IEnumerable<IInterface> IComposite.DirectSupertypes => this.DirectSupertypes;
        public IEnumerable<Interface> DirectSupertypes => this.directSupertypes;

        IEnumerable<IInterface> IComposite.Supertypes => this.Supertypes;
        public IEnumerable<Interface> Supertypes => this.supertypes;

        IEnumerable<IAssociationType> IComposite.AssociationTypes => this.AssociationTypes;
        public IEnumerable<IAssociationType> AssociationTypes => this.associationTypes;

        IEnumerable<IRoleType> IComposite.RoleTypes => this.RoleTypes;
        public IEnumerable<RoleType> RoleTypes => this.roleTypes;

        IEnumerable<IRoleType> IComposite.WorkspaceRoleTypes => this.WorkspaceRoleTypes;
        public IEnumerable<RoleType> WorkspaceRoleTypes => this.workspaceRoleTypes;

        IEnumerable<IRoleType> IComposite.DatabaseRoleTypes => this.DatabaseRoleTypes;
        public IEnumerable<RoleType> DatabaseRoleTypes => this.workspaceRoleTypes;

        IEnumerable<IMethodType> IComposite.MethodTypes => this.MethodTypes;
        public IEnumerable<MethodType> MethodTypes => this.methodTypes;

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

            this.roleTypes = new HashSet<RoleType>(roleTypes);
            this.workspaceRoleTypes = new HashSet<RoleType>(roleTypes.Where(v => v.Origin == Origin.Workspace));
        }

        /// <summary>
        /// Derive association types.
        /// </summary>
        /// <param name="sharedAssociationTypes">The associations.</param>
        /// <param name="relationTypesByRoleObjectType">AssociationTypes grouped by the ObjectType of the Role.</param>
        internal void DeriveAssociationTypes(HashSet<AssociationType> sharedAssociationTypes, Dictionary<ObjectType, HashSet<AssociationType>> relationTypesByRoleObjectType)
        {
            sharedAssociationTypes.Clear();

            if (relationTypesByRoleObjectType.TryGetValue(this, out var classAssociationTypes))
            {
                sharedAssociationTypes.UnionWith(classAssociationTypes);
            }

            foreach (var superType in this.Supertypes)
            {
                if (relationTypesByRoleObjectType.TryGetValue(superType, out var interfaceAssociationTypes))
                {
                    sharedAssociationTypes.UnionWith(interfaceAssociationTypes);
                }
            }

            this.associationTypes = new HashSet<AssociationType>(sharedAssociationTypes);
        }

        /// <summary>
        /// Derive method types.
        /// </summary>
        /// <param name="sharedMethodTypes">
        ///     The method types.
        /// </param>
        /// <param name="methodTypeByClass"></param>
        internal void DeriveMethodTypes(HashSet<MethodType> sharedMethodTypes, Dictionary<Composite, HashSet<MethodType>> methodTypeByClass)
        {
            sharedMethodTypes.Clear();

            if (methodTypeByClass.TryGetValue(this, out var directMethodTypes))
            {
                sharedMethodTypes.UnionWith(directMethodTypes);
            }

            foreach (var superType in this.Supertypes)
            {
                if (methodTypeByClass.TryGetValue(superType, out var inheritedMethodTypes))
                {
                    sharedMethodTypes.UnionWith(inheritedMethodTypes);
                }
            }

            this.methodTypes = new HashSet<MethodType>(sharedMethodTypes);
        }
    }
}
