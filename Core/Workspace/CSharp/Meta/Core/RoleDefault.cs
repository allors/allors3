// <copyright file="ConcreteRoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    public sealed partial class RoleDefault : RoleType, IRoleDefault
    {
        private ObjectType objectType;
        private string singularName;
        private string pluralName;
        private int? size;
        private int? precision;
        private int? scale;
        private bool isRequired;
        private bool isUnique;
        private string mediaType;

        public RoleDefault(RelationType relationType) : base(relationType)
        {
        }

        public override RoleDefault Default => null;

        public override Composite AssociationTypeComposite => this.AssociationType.ObjectType;

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
            get => this.singularName;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.singularName = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string PluralName
        {
            get => this.pluralName;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.pluralName = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Size
        {
            get => this.size;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.size = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Precision
        {
            get => this.precision;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.precision = value;
                this.MetaPopulation.Stale();
            }
        }

        public override int? Scale
        {
            get => this.scale;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.scale = value;
                this.MetaPopulation.Stale();
            }
        }

        public override bool IsRequired
        {
            get => this.isRequired;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.isRequired = value;
                this.MetaPopulation.Stale();
            }
        }

        public override bool IsUnique
        {
            get => this.isUnique;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.isUnique = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string MediaType
        {
            get => this.mediaType;
            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.mediaType = value;
                this.MetaPopulation.Stale();
            }
        }

        public override Guid OperandId => this.RelationType.Id;
    }
}
