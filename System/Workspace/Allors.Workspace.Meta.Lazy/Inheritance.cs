// <copyright file="Inheritance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Inheritance type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Linq;

    public sealed partial class Inheritance : MetaObjectBase, IComparable
    {
        public Inheritance(MetaPopulation metaPopulation) : base(metaPopulation) { }

        public Composite Subtype { get; set; }

        public Interface Supertype { get; set; }

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
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="otherObject">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="otherObject"/> is not the same type as this instance. </exception>
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
    }
}
