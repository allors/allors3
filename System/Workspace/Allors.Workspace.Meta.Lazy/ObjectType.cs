// <copyright file="ObjectType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;

    public abstract class ObjectType : MetaObjectBase, IObjectType, IComparable
    {

        private string pluralName;

        protected ObjectType(MetaPopulation metaPopulation, Guid id) : base(metaPopulation)
        {
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");
        }

        public Guid Id { get; }

        public string IdAsString { get; }

        public string SingularName { get; set; }

        public string PluralName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.pluralName))
                {
                    return this.pluralName;
                }

                return this.SingularName + RoleType.PluralSuffix;
            }

            set => this.pluralName = value;
        }

        public bool ExistAssignedPluralName => !string.IsNullOrEmpty(this.PluralName) && !this.PluralName.Equals(this.SingularName + "s");

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name .</value>
        public string Name => this.SingularName;

        public bool IsUnit => this is IUnit;

        public bool IsComposite => this is IComposite;

        public bool IsInterface => this is IInterface;

        public bool IsClass => this is IClass;

        public abstract Type ClrType { get; }

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The validation name.</value>
        public override string ValidationName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.SingularName))
                {
                    return "object type " + this.SingularName;
                }

                return "object type " + this.Id;
            }
        }

        public override bool Equals(object other) => this.Id.Equals((other as ObjectType)?.Id);

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
        public int CompareTo(object other) => this.Id.CompareTo((other as ObjectType)?.Id);

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.SingularName))
            {
                return this.SingularName;
            }

            return this.IdAsString;
        }
    }
}
