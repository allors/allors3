// <copyright file="MethodClassProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MethodClass type.</summary>

namespace Allors.Database.Meta
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class CompositeProps : ObjectTypeProps
    {
        public IEnumerable<IInterface> Supertypes => this.AsComposite.Supertypes;

        public IEnumerable<IClass> Classes => this.AsComposite.Classes;

        public bool ExistExclusiveClass => this.AsComposite.ExistExclusiveClass;

        public IEnumerable<IInterface> DirectSupertypes => this.AsComposite.DirectSupertypes;

        public IEnumerable<IComposite> Subtypes => this.AsComposite.Subtypes;

        public IEnumerable<IComposite> RelatedComposites =>
            this
                .Supertypes
                .Union(this.RoleTypes.Where(m => m.ObjectType.IsComposite).Select(v => (IComposite)v.ObjectType))
                .Union(this.AssociationTypes.Select(v => v.ObjectType)).Distinct()
                .Except(new[] { this.AsComposite }).ToArray();

        public IEnumerable<IAssociationType> AssociationTypes => this.AsComposite.AssociationTypes;

        public IEnumerable<IAssociationType> ExclusiveAssociationTypes => this.AsComposite.ExclusiveAssociationTypes;

        public IEnumerable<IRoleType> RoleTypes => this.AsComposite.RoleTypes;

        public IEnumerable<IRoleType> ExclusiveRoleTypes => this.AsComposite.ExclusiveRoleTypes;

        public IEnumerable<IAssociationType> DatabaseAssociationTypes => this.AsComposite.DatabaseAssociationTypes;

        public IEnumerable<IRoleType> DatabaseRoleTypes => this.AsComposite.DatabaseRoleTypes;

        public IEnumerable<IMethodType> MethodTypes => this.AsComposite.MethodTypes;

        public bool ExistDatabaseClass => this.AsComposite.ExistDatabaseClass;

        public IEnumerable<IClass> DatabaseClasses => this.AsComposite.DatabaseClasses;

        public bool ExistExclusiveDatabaseClass => this.AsComposite.ExistExclusiveDatabaseClass;

        public IClass ExclusiveDatabaseClass => this.AsComposite.ExclusiveDatabaseClass;

        public IReadOnlyDictionary<string, IEnumerable<IAssociationType>> WorkspaceAssociationTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.AssociationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IAssociationType>> WorkspaceExclusiveAssociationTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.ExclusiveAssociationTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceRoleTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.RoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceCompositeRoleTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.RoleTypes.Where(w => w.ObjectType.IsComposite && w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceExclusiveRoleTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.ExclusiveRoleTypes.Where(w => w.RelationType.WorkspaceNames.Contains(v)));
            }
        }


        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceExclusiveRoleTypesWithDatabaseOriginByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.ExclusiveRoleTypes.Where(w => w.Origin == Origin.Database && w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceExclusiveRoleTypesWithWorkspaceOrSessionOriginByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.ExclusiveRoleTypes.Where(w => (w.Origin == Origin.Workspace || w.Origin == Origin.Session) && w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IRoleType>> WorkspaceExclusiveCompositeRoleTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v,
                        v => this.ExclusiveRoleTypes.Where(w => w.ObjectType.IsComposite && w.RelationType.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IMethodType>> WorkspaceMethodTypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.MethodTypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IInterface>> WorkspaceDirectSupertypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.DirectSupertypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IInterface>> WorkspaceSupertypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.Supertypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IComposite>> WorkspaceSubtypesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.Subtypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<IComposite>> WorkspaceRelatedCompositesByWorkspaceName
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.RelatedComposites.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public bool IsSynced => this.AsComposite.IsSynced;

        #region As
        protected abstract ICompositeBase AsComposite { get; }
        #endregion
    }
}
