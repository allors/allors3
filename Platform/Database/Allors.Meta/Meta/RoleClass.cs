// <copyright file="ConcreteRoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Meta
{
    using System;

    public sealed partial class RoleClass : RoleType, IRoleClass
    {
        private ObjectType objectType;

        private string pluralName;
        private int? precision;
        private int? scale;
        private string singularName;
        private int? size;
        private string mediaType;
        private bool? isRequired;
        private bool? isUnique;

        public RoleClass(Class associationTypeClass, RelationType relationType, RoleInterface roleInterface = null) : base(relationType)
        {
            this.AssociationTypeClass = associationTypeClass;
            this.RoleInterface = roleInterface;

            this.MetaPopulation.OnRoleClassCreated(this);
        }

        public Class AssociationTypeClass { get; }

        public bool ExistRoleInterface => this.RoleInterface != null;

        public RoleInterface RoleInterface { get; }
        IRoleInterface IRoleClass.RoleInterface => this.RoleInterface;

        IClass IRoleClass.AssociationTypeClass => this.AssociationTypeClass;

        public override ObjectType ObjectType
        {
            get => this.objectType;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.objectType = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string SingularName
        {
            get => this.singularName ?? this.RoleInterface?.SingularName;

            set
            {
                if (this.ExistRoleInterface)
                {
                    throw new ArgumentException("SingularName is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.singularName = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string PluralName
        {
            get => this.pluralName ?? this.RoleInterface?.PluralName;

            set
            {
                if (this.ExistRoleInterface)
                {
                    throw new ArgumentException("PluralName is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.pluralName = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Size
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.size ?? this.RoleInterface?.Size;
            }

            set
            {
                // TODO: Should we allow smaller sizes?
                if (this.ExistRoleInterface)
                {
                    throw new ArgumentException("Size is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.size = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Precision
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.precision ?? this.RoleInterface?.Precision;
            }

            set
            {
                // TODO: Should we allow smaller precision?
                if (this.ExistRoleInterface)
                {
                    throw new ArgumentException("SingularName is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.precision = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Scale
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.scale ?? this.RoleInterface?.Scale;
            }

            set
            {
                // TODO: Should we allow smaller precision?
                if (this.ExistRoleInterface)
                {
                    throw new ArgumentException("Scale is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.scale = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string MediaType
        {
            get => this.mediaType ?? this.RoleInterface?.MediaType;
            set => this.mediaType = value;
        }

        public override bool IsRequired
        {
            get => this.isRequired ?? this.RoleInterface?.IsRequired ?? false;
            set => this.isRequired = value;
        }

        public override bool IsUnique
        {
            get => this.isUnique ?? this.RoleInterface?.IsUnique ?? false;
            set => this.isUnique = value;
        }
    }
}
