// <copyright file="ConcreteRoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
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

        public RoleClass(RelationType relationType, Class associationTypeComposite, RoleDefault @default = null) : base(relationType)
        {
            this.AssociationTypeComposite = associationTypeComposite;
            this.Default = @default;

            this.MetaPopulation.OnRoleClassCreated(this);
        }

        public override RoleDefault Default { get; }
        IRoleDefault IRoleType.Default => this.Default;

        public override Composite AssociationTypeComposite { get; }
        IComposite IRoleType.AssociationTypeComposite => this.AssociationTypeComposite;

        public override ObjectType ObjectType
        {
            get => this.objectType ?? this.Default?.ObjectType;

            set
            {
                if (this.ExistDefault)
                {
                    throw new ArgumentException("ObjectType is readonly when ExistRoleInterface");
                }

                this.MetaPopulation.AssertUnlocked();
                this.objectType = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string SingularName
        {
            get => this.singularName ?? this.Default?.SingularName;

            set
            {
                if (this.ExistDefault)
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
            get => this.pluralName ?? this.Default?.PluralName;

            set
            {
                if (this.ExistDefault)
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
                return this.size ?? this.Default?.Size;
            }

            set
            {
                // TODO: Should we allow smaller sizes?
                if (this.ExistDefault)
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
                return this.precision ?? this.Default?.Precision;
            }

            set
            {
                // TODO: Should we allow smaller precision?
                if (this.ExistDefault)
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
                return this.scale ?? this.Default?.Scale;
            }

            set
            {
                // TODO: Should we allow smaller precision?
                if (this.ExistDefault)
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
            get => this.mediaType ?? this.Default?.MediaType;
            set => this.mediaType = value;
        }

        public override bool IsRequired
        {
            get => this.isRequired ?? this.Default?.IsRequired ?? false;
            set => this.isRequired = value;
        }

        public override bool IsUnique
        {
            get => this.isUnique ?? this.Default?.IsUnique ?? false;
            set => this.isUnique = value;
        }
    }
}
