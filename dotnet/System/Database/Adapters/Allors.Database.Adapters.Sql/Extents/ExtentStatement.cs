// <copyright file="ExtentStatement.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System.Collections.Generic;
    using Meta;

    internal abstract class ExtentStatement
    {
        private readonly List<IAssociationType> referenceAssociationInstances;
        private readonly List<IAssociationType> referenceAssociations;
        private readonly List<IRoleType> referenceRoleInstances;
        private readonly List<IRoleType> referenceRoles;

        // The aliases of the joins of this statement. The statement hands them out, as it does for its tables: an
        // alias built from the name of a role or of an association can equal another one, where the server
        // compares names without case or keeps only the start of a long name.
        private Dictionary<IAssociationType, string> aliasByAssociation;
        private Dictionary<IAssociationType, string> aliasByAssociationInstance;
        private Dictionary<IRoleType, string> aliasByRole;
        private Dictionary<IRoleType, string> aliasByRoleInstance;

        protected ExtentStatement(SqlExtent extent)
        {
            this.Extent = extent;

            this.referenceRoles = new List<IRoleType>();
            this.referenceAssociations = new List<IAssociationType>();

            this.referenceRoleInstances = new List<IRoleType>();
            this.referenceAssociationInstances = new List<IAssociationType>();
        }

        internal Mapping Mapping => this.Transaction.Database.Mapping;

        internal ExtentSort Sorter => this.Extent.Sorter;

        protected Transaction Transaction => this.Extent.Transaction;

        internal SqlExtent Extent { get; }

        internal abstract bool IsRoot { get; }

        protected IObjectType Type => this.Extent.ObjectType;

        internal void AddJoins(IObjectType rootClass, string alias)
        {
            foreach (var role in this.referenceRoles)
            {
                var relationType = role.RelationType;
                var association = relationType.AssociationType;

                if (!role.ObjectType.IsUnit)
                {
                    if ((association.IsMany && role.IsMany) || !relationType.ExistExclusiveDatabaseClasses)
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForRelationByRelationType[relationType] + " " + this.GetAlias(role));
                        this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(role) + "." + Mapping.RelationTableAssociationColumnNameFor(relationType));
                    }
                    else if (role.IsMany)
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjectByClass[((IComposite)role.ObjectType).ExclusiveDatabaseClass] + " " + this.GetAlias(role));
                        this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(role) + "." + this.Mapping.ColumnNameByRelationType[relationType]);
                    }
                }
            }

            foreach (var role in this.referenceRoleInstances)
            {
                var relationType = role.RelationType;

                if (!role.ObjectType.IsUnit && role.IsOne)
                {
                    if (!relationType.ExistExclusiveDatabaseClasses)
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(role));
                        this.Append(" ON " + this.GetJoinName(role) + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(role) + "." + Mapping.RelationTableRoleColumnNameFor(relationType) + " ");
                    }
                    else
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(role));
                        this.Append(" ON " + this.GetJoinName(role) + "." + Mapping.ColumnNameForObject + "=" + alias + "." + this.Mapping.ColumnNameByRelationType[relationType] + " ");
                    }
                }
            }

            foreach (var association in this.referenceAssociations)
            {
                var relationType = association.RelationType;
                var role = relationType.RoleType;

                if ((association.IsMany && role.IsMany) || !relationType.ExistExclusiveDatabaseClasses)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForRelationByRelationType[relationType] + " " + this.GetAlias(association));
                    this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(association) + "." + Mapping.RelationTableRoleColumnNameFor(relationType));
                }
                else if (!role.IsMany)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjectByClass[association.ObjectType.ExclusiveDatabaseClass] + " " + this.GetAlias(association));
                    this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(association) + "." + this.Mapping.ColumnNameByRelationType[relationType]);
                }
            }

            foreach (var association in this.referenceAssociationInstances)
            {
                var relationType = association.RelationType;
                var role = relationType.RoleType;

                if (!association.ObjectType.IsUnit && association.IsOne)
                {
                    if (!relationType.ExistExclusiveDatabaseClasses)
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                        this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(association) + "." + Mapping.RelationTableAssociationColumnNameFor(relationType) + " ");
                    }
                    else if (role.IsOne)
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                        this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" + this.GetAlias(association) + "." + Mapping.ColumnNameForObject + " ");
                    }
                    else
                    {
                        this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                        this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" + alias + "." + this.Mapping.ColumnNameByRelationType[relationType] + " ");
                    }
                }
            }
        }

        internal abstract string AddParameter(object obj);

        internal bool AddWhere(IObjectType rootClass, string alias)
        {
            var useWhere = !this.Extent.ObjectType.ExistExclusiveDatabaseClass;

            if (useWhere)
            {
                this.Append(" WHERE ( ");
                if (!this.Type.IsInterface)
                {
                    this.Append(" " + alias + "." + Mapping.ColumnNameForClass + "=" + this.AddParameter(this.Type.Id));
                }
                else
                {
                    var first = true;
                    foreach (var subClass in ((IInterface)this.Type).DatabaseClasses)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            this.Append(" OR ");
                        }

                        this.Append(" " + alias + "." + Mapping.ColumnNameForClass + "=" + this.AddParameter(subClass.Id));
                    }
                }

                this.Append(" ) ");
            }

            return useWhere;
        }

        internal abstract void Append(string part);

        internal abstract string CreateAlias();

        internal abstract ExtentStatement CreateChild(SqlExtent extent, IAssociationType association);

        internal abstract ExtentStatement CreateChild(SqlExtent extent, IRoleType role);

        // The alias of the join that gives the associations of the objects of this statement.
        internal string GetAlias(IAssociationType association) => this.GetOrCreateAlias(ref this.aliasByAssociation, association);

        // The alias of the join that gives the roles of the objects of this statement.
        internal string GetAlias(IRoleType role) => this.GetOrCreateAlias(ref this.aliasByRole, role);

        // The alias of the join that gives the class of the association.
        internal string GetJoinName(IAssociationType association) => this.GetOrCreateAlias(ref this.aliasByAssociationInstance, association);

        // The alias of the join that gives the class of the role.
        internal string GetJoinName(IRoleType role) => this.GetOrCreateAlias(ref this.aliasByRoleInstance, role);

        internal void UseAssociation(IAssociationType association)
        {
            if (!association.ObjectType.IsUnit && !this.referenceAssociations.Contains(association))
            {
                this.referenceAssociations.Add(association);
            }
        }

        internal void UseAssociationInstance(IAssociationType association)
        {
            if (!this.referenceAssociationInstances.Contains(association))
            {
                this.referenceAssociationInstances.Add(association);
            }
        }

        internal void UseRole(IRoleType role)
        {
            if (!role.ObjectType.IsUnit && !this.referenceRoles.Contains(role))
            {
                this.referenceRoles.Add(role);
            }
        }

        internal void UseRoleInstance(IRoleType role)
        {
            if (!this.referenceRoleInstances.Contains(role))
            {
                this.referenceRoleInstances.Add(role);
            }
        }

        private string GetOrCreateAlias<T>(ref Dictionary<T, string> aliasByType, T type)
        {
            aliasByType ??= new Dictionary<T, string>();
            if (!aliasByType.TryGetValue(type, out var alias))
            {
                alias = this.CreateAlias();
                aliasByType.Add(type, alias);
            }

            return alias;
        }
    }
}
