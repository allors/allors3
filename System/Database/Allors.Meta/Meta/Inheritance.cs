// <copyright file="Inheritance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Inheritance type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Linq;

    public sealed partial class Inheritance : MetaObjectBase, IComparable
    {
        private Composite subtype;

        private Interface supertype;

        internal Inheritance(MetaPopulation metaPopulation) : base(metaPopulation) => metaPopulation.OnInheritanceCreated(this);

        public Composite Subtype
        {
            get => this.subtype;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.subtype = value;
                this.MetaPopulation.Stale();
            }
        }

        public Interface Supertype
        {
            get => this.supertype;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.supertype = value;
                this.MetaPopulation.Stale();
            }
        }

        public override Origin Origin => this.Subtype.AssignedOrigin;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        public override string ValidationName
        {
            get
            {
                if (this.Supertype != null && this.Subtype != null)
                {
                    return "inheritance " + this.Subtype + "::" + this.Supertype;
                }

                return "unknown inheritance";
            }
        }

        public override bool Equals(object other) => this.Subtype.Id.Equals((other as Inheritance)?.Subtype.Id) && this.Supertype.Id.Equals((other as Inheritance)?.Supertype.Id);

        public override int GetHashCode() => this.Subtype.Id.GetHashCode() ^ this.Supertype.Id.GetHashCode();

        /// <summary>
        /// Compares the current state with another object of the same type.
        /// </summary>
        /// <param name="otherObject">An object to compare with this state.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This state is less than <paramref name="obj"/>. Zero This state is equal to <paramref name="obj"/>. Greater than zero This state is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="otherObject"/> is not the same type as this state. </exception>
        public int CompareTo(object otherObject)
        {
            var other = otherObject as Inheritance;
            return string.CompareOrdinal($"{this.Subtype.Id}{this.Supertype.Id}", $"{other?.Subtype.Id}{other?.Supertype.Id}");
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => (this.Subtype != null ? this.Subtype.Name : string.Empty) + "::" + (this.Supertype != null ? this.Supertype.Name : string.Empty);

        /// <summary>
        /// Validates this state.
        /// </summary>
        /// <param name="validationLog">The validation.</param>
        protected internal void Validate(ValidationLog validationLog)
        {
            if (this.Subtype != null && this.Supertype != null)
            {
                if (this.MetaPopulation.Inheritances.Count(inheritance => this.Subtype.Equals(inheritance.Subtype) && this.Supertype.Equals(inheritance.Supertype)) != 1)
                {
                    var message = "name of " + this.ValidationName + " is already in use";
                    validationLog.AddError(message, this, ValidationKind.Unique, "Inheritance.Supertype");
                }

                IObjectType tempQualifier = this.Supertype;
                if (tempQualifier is IClass)
                {
                    var message = this.ValidationName + " can not have a concrete superclass";
                    validationLog.AddError(message, this, ValidationKind.Hierarchy, "Inheritance.Supertype");
                }
            }
            else
            {
                if (this.Supertype == null)
                {
                    var message = this.ValidationName + " has a missing Supertype";
                    validationLog.AddError(message, this, ValidationKind.Unique, "Inheritance.Supertype");
                }
                else
                {
                    var message = this.ValidationName + " has a missing Subtype";
                    validationLog.AddError(message, this, ValidationKind.Unique, "Inheritance.Supertype");
                }
            }
        }
    }
}
