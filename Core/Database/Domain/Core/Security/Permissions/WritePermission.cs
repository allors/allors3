// <copyright file="Permission.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Text;

    using Allors.Meta;

    public partial class WritePermission
    {
        public ObjectType ConcreteClass
        {
            get => (ObjectType)this.Strategy.Session.Database.MetaPopulation.Find(this.ClassPointer);

            set
            {
                if (value == null)
                {
                    this.RemoveClassPointer();
                }
                else
                {
                    this.ClassPointer = value.Id;
                }
            }
        }

        public bool ExistConcreteClass => this.ConcreteClass != null;

        public bool ExistOperandType => this.ExistRelationTypePointer;

        public bool ExistOperation => true;

        public OperandType OperandType => this.RelationType.RoleType;

        public RelationType RelationType
        {
            get => (RelationType)this.Strategy.Session.Database.MetaPopulation.Find(this.RelationTypePointer);

            set
            {
                if (value == null)
                {
                    this.RemoveRelationTypePointer();
                }
                else
                {
                    this.RelationTypePointer = value.Id;
                }
            }
        }

        public Operations Operation => Operations.Write;

        public void CoreOnPreDerive(ObjectOnPreDerive method)
        {
            var (iteration, changeSet, derivedObjects) = method;

            if (changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            {
                foreach (Role role in this.RolesWherePermission)
                {
                    iteration.AddDependency(role, this);
                    iteration.Mark(role);
                }

                this.Strategy.Session.ClearCache<PermissionCache>();
            }
        }

        public override string ToString()
        {
            var toString = new StringBuilder();
            if (this.ExistOperation)
            {
                var operation = this.Operation;
                toString.Append(operation);
            }
            else
            {
                toString.Append("[missing operation]");
            }

            toString.Append(" for ");

            toString.Append(this.ExistOperandType ? this.OperandType.GetType().Name + ":" + this.OperandType : "[missing operand]");

            return toString.ToString();
        }
    }
}
