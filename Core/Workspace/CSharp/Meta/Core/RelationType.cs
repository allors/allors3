// <copyright file="RelationType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RelationType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// A <see cref="RelationType"/> defines the state and behavior for
    /// a set of <see cref="AssociationType"/>s and <see cref="RoleType"/>s.
    /// </summary>
    public sealed partial class RelationType : MetaObjectBase, IRelationType, IComparable
    {
        private Multiplicity multiplicity;

        public RelationType(Composite associationTypeComposite, Guid id, Func<RelationType, AssociationType> associationTypeFactory, Func<RelationType, RoleType> roleTypeFactory)
            : base(associationTypeComposite.MetaPopulation)
        {
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");
            this.AssignedOrigin = Origin.Database;

            this.AssociationType = associationTypeFactory(this);
            this.AssociationType.ObjectType = associationTypeComposite;

            this.RoleType = roleTypeFactory(this);
        }
        
        public Guid Id { get; }

        public string IdAsString { get; }
        
        public override Origin Origin => this.AssignedOrigin;

        public Origin AssignedOrigin { get; set; }

        public bool IsDerived { get; set; }

        public bool IsSynced { get; set; }

        public Multiplicity AssignedMultiplicity { get; set; }

        public Multiplicity Multiplicity => this.multiplicity;

        IAssociationType IRelationType.AssociationType => this.AssociationType;

        public AssociationType AssociationType { get; set; }

        IRoleType IRelationType.RoleType => this.RoleType;

        public RoleType RoleType { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name  .</value>
        public string Name => this.AssociationType.ObjectType + this.RoleType.SingularName;

        /// <summary>
        /// Gets the name of the reverse.
        /// </summary>
        /// <value>The name of the reverse.</value>
        public string ReverseName => this.RoleType.SingularName + this.AssociationType.ObjectType;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The validation name.</value>
        public override string ValidationName => "relation type" + this.Name;

        public override bool Equals(object other) => this.Id.Equals((other as RelationType)?.Id);

        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="other"/> is not the same type as this instance. </exception>
        public int CompareTo(object other) => this.Id.CompareTo((other as RelationType)?.Id);

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            try
            {
                return this.Name;
            }
            catch
            {
                return this.IdAsString;
            }
        }
        
        internal void DeriveMultiplicity()
        {
            if (this.RoleType?.ObjectType != null && this.RoleType.ObjectType.IsUnit)
            {
                this.multiplicity = Multiplicity.OneToOne;
            }
            else
            {
                this.multiplicity = this.AssignedMultiplicity;
            }
        }
    }
}
