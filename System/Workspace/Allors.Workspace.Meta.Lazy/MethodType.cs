// <copyright file="MethodType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Linq;

    public sealed class MethodType : OperandType, IMethodType, IComparable
    {
        public MethodType(Composite objectType, Guid id) : base(objectType.MetaPopulation)
        {
            this.Composite = objectType;
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");
        }

        public Guid Id { get; }

        public string IdAsString { get; }

        public Composite Composite { get; }

        public string Name { get; set; }

        public string FullName => $"{this.Composite.SingularName}{this.Name}";

        public override Guid OperandId => this.Id;

        public override Origin Origin => Origin.Database;

        IComposite IMethodType.ObjectType => this.Composite;

        public override string DisplayName => this.Name;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The validation name.</value>
        public override string ValidationName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    return "method type " + this.Name;
                }

                return "unknown method type";
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => this.Name;

        public override bool Equals(object other) => this.Id.Equals((other as MethodType)?.Id);

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
        public int CompareTo(object other) => this.Id.CompareTo((other as MethodType)?.Id);
    }
}
