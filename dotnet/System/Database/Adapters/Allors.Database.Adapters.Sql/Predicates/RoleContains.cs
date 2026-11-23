// <copyright file="RoleContains.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsPredicateRoleContainsSql type.
// </summary>

namespace Allors.Database.Adapters.Sql
{
    using Adapters;
    using Meta;

    internal sealed class RoleContains : Predicate
    {
        private readonly IObject allorsObject;
        private readonly IRoleType role;

        internal RoleContains(ExtentFiltered extent, IRoleType role, IObject allorsObject)
        {
            extent.CheckRole(role);
            PredicateAssertions.ValidateRoleContains(role, allorsObject);
            this.role = role;
            this.allorsObject = allorsObject;
        }

        internal override bool BuildWhere(ExtentStatement statement, string alias)
        {
            var schema = statement.Mapping;
            var innerAlias = statement.CreateAlias();
            statement.Append("\nEXISTS(\nSELECT 1\n");
            if ((this.role.IsMany && this.role.RelationType.AssociationType.IsMany) || !this.role.RelationType.ExistExclusiveDatabaseClasses)
            {
                statement.Append($"FROM {schema.TableNameForRelationByRelationType[this.role.RelationType]} {innerAlias}\n");
                statement.Append($"WHERE {innerAlias}.{Mapping.ColumnNameForAssociation}={alias}.{Mapping.ColumnNameForObject}\n");
                statement.Append($"AND {innerAlias}.{Mapping.ColumnNameForRole}={this.allorsObject.Strategy.ObjectId}\n");
            }
            else
            {
                // NOT EXISTS also matches objects that have no related row.
                var roleClass = ((IComposite)this.role.ObjectType).ExclusiveDatabaseClass;
                statement.Append($"FROM {schema.TableNameForObjectByClass[roleClass]} {innerAlias}\n");
                statement.Append($"WHERE {innerAlias}.{Mapping.ColumnNameForObject}={this.allorsObject.Strategy.ObjectId}\n");
                statement.Append($"AND {innerAlias}.{schema.ColumnNameByRelationType[this.role.RelationType]}={alias}.{Mapping.ColumnNameForObject}\n");
            }

            statement.Append(")\n");
            return this.Include;
        }

        internal override void Setup(ExtentStatement statement)
        {
            // extent.UseRole(role);
        }
    }
}
