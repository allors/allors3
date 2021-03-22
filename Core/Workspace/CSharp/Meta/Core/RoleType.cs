// <copyright file="RoleType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    public abstract partial class RoleType : OperandType, IRoleType, IComparable
    {
        /// <summary>
        /// The maximum size value.
        /// </summary>
        public const int MaximumSize = -1;

        public const string PluralSuffix = "s";

        protected RoleType(RelationType relationType) : base(relationType.MetaPopulation) => this.RelationType = relationType;

        public bool ExistDefault => this.Default != null;

        IRoleDefault IRoleType.Default => this.Default;
        public abstract RoleDefault Default { get; }

        public override Origin Origin => this.RelationType.Origin;

        public RelationType RelationType { get; }
        IRelationType IRoleType.RelationType => this.RelationType;

        /// <summary>
        /// Gets the association.
        /// </summary>
        /// <value>The association.</value>
        public AssociationType AssociationType => this.RelationType.AssociationType;
        IAssociationType IRoleType.AssociationType => this.AssociationType;

        IComposite IRoleType.AssociationTypeComposite => this.AssociationTypeComposite;
        public abstract Composite AssociationTypeComposite { get; }

        public abstract ObjectType ObjectType { get; set; }

        IObjectType IPropertyType.ObjectType => this.ObjectType;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name .</value>
        public string Name => this.IsMany ? this.PluralName : this.SingularName;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName => this.Name;

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName => this.IsMany ? this.PluralFullName : this.SingularFullName;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The validation name.</value>
        public override string ValidationName => "RoleType: " + this.RelationType.Name;

        public abstract string SingularName { get; set; }

        /// <summary>
        /// Gets the full singular name.
        /// </summary>
        /// <value>The full singular name.</value>
        public string SingularFullName => this.RelationType.AssociationType.ObjectType + this.SingularName;

        public abstract string PluralName { get; set; }

        /// <summary>
        /// Gets the full plural name.
        /// </summary>
        /// <value>The full plural name.</value>
        public string PluralFullName => this.RelationType.AssociationType.ObjectType + this.PluralName;

        public bool IsMany
        {
            get
            {
                switch (this.RelationType.Multiplicity)
                {
                    case Multiplicity.OneToMany:
                    case Multiplicity.ManyToMany:
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this state has a multiplicity of one.
        /// </summary>
        /// <value><c>true</c> if this state is one; otherwise, <c>false</c>.</value>
        public bool IsOne => !this.IsMany;

        public abstract int? Size { get; set; }
        public abstract int? Precision { get; set; }
        public abstract int? Scale { get; set; }

        public abstract bool IsRequired { get; set; }
        public abstract bool IsUnique { get; set; }
        public abstract string MediaType { get; set; }

        ///// <summary>
        ///// Instantiate the value of the role on this object.
        ///// </summary>
        ///// <param name="strategy">
        ///// The strategy.
        ///// </param>
        ///// <returns>
        ///// The role value.
        ///// </returns>
        public object Get(IStrategy strategy) => strategy.GetRole(this.RelationType.RoleType);

        ///// <summary>
        ///// Set the value of the role on this object.
        ///// </summary>
        ///// <param name="strategy">
        ///// The strategy.
        ///// </param>
        ///// <param name="value">
        ///// The role value.
        ///// </param>
        public void Set(IStrategy strategy, object value) => strategy.SetRole(this.RelationType.RoleType, value);

        public override bool Equals(object other) => this.RelationType.Id.Equals((other as RoleType)?.RelationType.Id);

        public override int GetHashCode() => this.RelationType.Id.GetHashCode();

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="other"/> is not the same type as this instance. </exception>
        public int CompareTo(object other) => this.RelationType.Id.CompareTo((other as RoleType)?.RelationType.Id);

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => $"{this.AssociationType.ObjectType.Name}.{this.Name}";

        /// <summary>
        /// Derive multiplicity, scale and size.
        /// </summary>
        internal void DeriveScaleAndSize()
        {
            if (this.ObjectType is IUnit unitType)
            {
                switch (unitType.UnitTag)
                {
                    case UnitTags.String:
                        if (!this.Size.HasValue)
                        {
                            this.Size = 256;
                        }

                        this.Scale = null;
                        this.Precision = null;

                        break;

                    case UnitTags.Binary:
                        if (!this.Size.HasValue)
                        {
                            this.Size = MaximumSize;
                        }

                        this.Scale = null;
                        this.Precision = null;

                        break;

                    case UnitTags.Decimal:
                        if (!this.Precision.HasValue)
                        {
                            this.Precision = 19;
                        }

                        if (!this.Scale.HasValue)
                        {
                            this.Scale = 2;
                        }

                        this.Size = null;

                        break;

                    default:
                        this.Size = null;
                        this.Scale = null;
                        this.Precision = null;

                        break;
                }
            }
            else
            {
                this.Size = null;
                this.Scale = null;
                this.Precision = null;
            }
        }

        /// <summary>
        /// Validates the instance.
        /// </summary>
        /// <param name="validationLog">The validation.</param>
        public void Validate(ValidationLog validationLog)
        {
            if (this.ObjectType == null)
            {
                var message = this.ValidationName + " has no IObjectType";
                validationLog.AddError(message, this, ValidationKind.Required, "RoleType.IObjectType");
            }

            if (!string.IsNullOrEmpty(this.SingularName) && this.SingularName.Length < 2)
            {
                var message = this.ValidationName + " should have an assigned singular name with at least 2 characters";
                validationLog.AddError(message, this, ValidationKind.MinimumLength, "RoleType.SingularName");
            }

            if (!string.IsNullOrEmpty(this.PluralName) && this.PluralName.Length < 2)
            {
                var message = this.ValidationName + " should have an assigned plural role name with at least 2 characters";
                validationLog.AddError(message, this, ValidationKind.MinimumLength, "RoleType.PluralName");
            }
        }
    }
}
