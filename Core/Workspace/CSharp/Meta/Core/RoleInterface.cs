// <copyright file="RoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    public sealed partial class RoleInterface : RoleType, IRoleInterface
    {
        public RoleInterface(RelationType relationType, Composite associationTypeComposite, RoleDefault @default = null) : base(relationType)
        {
            this.AssociationTypeComposite = associationTypeComposite;
            this.Default = @default;

            this.MetaPopulation.OnRoleInterfaceCreated(this);
        }

        public override RoleDefault Default { get; }
        IRoleDefault IRoleType.Default => this.Default;

        public override Composite AssociationTypeComposite { get; }
        IComposite IRoleType.AssociationTypeComposite => this.AssociationTypeComposite;

        public override int? Size
        {
            get => this.Default.Size;
            set => throw new NotSupportedException();
        }

        public override int? Precision
        {
            get => this.Default.Precision;
            set => throw new NotSupportedException();
        }

        public override int? Scale
        {
            get => this.Default.Scale;
            set => throw new NotSupportedException();
        }

        public override bool IsRequired
        {
            get => this.Default.IsRequired;
            set => throw new NotSupportedException();
        }

        public override bool IsUnique
        {
            get => this.Default.IsUnique;
            set => throw new NotSupportedException();
        }

        public override string MediaType
        {
            get => this.Default.MediaType;
            set => throw new NotSupportedException();
        }

        public override string SingularName
        {
            get => this.Default.SingularName;
            set => throw new NotSupportedException();
        }

        public override string PluralName
        {
            get => this.Default.PluralName;
            set => throw new NotSupportedException();
        }

        public override ObjectType ObjectType
        {
            get => this.Default.ObjectType;
            set => throw new NotSupportedException();
        }

        public override Guid OperandId => this.RelationType.Id;
    }
}
