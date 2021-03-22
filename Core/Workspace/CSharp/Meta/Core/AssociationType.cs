// <copyright file="AssociationType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the AssociationType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    /// <summary>
    /// An <see cref="AssociationType"/> defines the association side of a relation.
    /// This is also called the 'active', 'controlling' or 'owning' side.
    /// AssociationTypes can only have composite <see cref="ObjectType"/>s.
    /// </summary>
    public sealed partial class AssociationType : OperandType, IAssociationType, IComparable
    {
        /// <summary>
        /// Used to create property names.
        /// </summary>
        private const string Where = "Where";

        private Composite objectType;

        internal AssociationType(RelationType relationType)
            : base(relationType.MetaPopulation)
        {
            this.RelationType = relationType;
            relationType.MetaPopulation.OnAssociationTypeCreated(this);
        }

        public override Origin Origin => this.RelationType.Origin;

        public RelationType RelationType { get; }

        IRelationType IAssociationType.RelationType => this.RelationType;

        IRoleType IAssociationType.RoleType => this.RoleType;

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <value>The role .</value>
        public RoleType RoleType => this.RelationType.RoleType;

        public Composite ObjectType
        {
            get => this.objectType;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.objectType = value;
                this.MetaPopulation.Stale();
            }
        }
        IObjectType IPropertyType.ObjectType => this.ObjectType;
        IComposite IAssociationType.ObjectType => this.ObjectType;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName => this.Name;

        public override Guid OperandId => this.RelationType.Id;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The name of the validation.</value>
        public override string ValidationName => "association type " + this.Name;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name .</value>
        public string Name => this.IsMany ? this.PluralName : this.SingularName;

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName => this.IsMany ? this.PluralName : this.SingularName;

        /// <summary>
        /// Gets the singular name when using <see cref="Where"/>.
        /// </summary>
        /// <value>The singular name when using <see cref="Where"/>.</value>
        public string SingularName => this.ObjectType.SingularName + Where + this.RoleType.SingularName;

        /// <summary>
        /// Gets the singular name when using <see cref="Where"/>.
        /// </summary>
        /// <value>The singular name when using <see cref="Where"/>.</value>
        public string SingularFullName => this.SingularName;

        /// <summary>
        /// Gets the plural name when using <see cref="Where"/>.
        /// </summary>
        /// <value>The plural name when using <see cref="Where"/>.</value>
        public string PluralName => this.ObjectType.PluralName + Where + this.RoleType.SingularName;

        /// <summary>
        /// Gets the plural name when using <see cref="Where"/>.
        /// </summary>
        /// <value>The plural name when using <see cref="Where"/>.</value>
        public string PluralFullName => this.PluralName;

        public bool IsMany
        {
            get
            {
                switch (this.RelationType.Multiplicity)
                {
                    case Multiplicity.ManyToOne:
                    case Multiplicity.ManyToMany:
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a multiplicity of one.
        /// </summary>
        /// <value><c>true</c> if this instance is one; otherwise, <c>false</c>.</value>
        public bool IsOne => !this.IsMany;

        public override bool Equals(object other) => this.RelationType.Id.Equals((other as AssociationType)?.RelationType.Id);

        public override int GetHashCode() => this.RelationType.Id.GetHashCode();

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="other"/>. Zero This instance is equal to <paramref name="other"/>. Greater than zero This instance is greater than <paramref name="other"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="other"/> is not the same type as this instance. </exception>
        public int CompareTo(object other) => this.RelationType.Id.CompareTo((other as AssociationType)?.RelationType.Id);

        ///// <summary>
        ///// Instantiate the value of the association on this object.
        ///// </summary>
        ///// <param name="strategy">
        ///// The strategy.
        ///// </param>
        ///// <returns>
        ///// The association value.
        ///// </returns>
        public object Get(IStrategy strategy) => strategy.GetCompositeAssociation(this);

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => $"{this.RoleType.ObjectType.Name}.{this.DisplayName}";

        /// <summary>
        /// Validates this object.
        /// </summary>
        /// <param name="validationLog">The validation information.</param>
        internal void Validate(ValidationLog validationLog)
        {
            if (this.ObjectType == null)
            {
                var message = this.ValidationName + " has no object type";
                validationLog.AddError(message, this, ValidationKind.Required, "AssociationType.IObjectType");
            }

            if (this.RelationType == null)
            {
                var message = this.ValidationName + " has no relation type";
                validationLog.AddError(message, this, ValidationKind.Required, "AssociationType.RelationType");
            }
        }
    }
}
