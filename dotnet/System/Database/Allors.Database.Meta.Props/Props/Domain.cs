// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class Domain : IDomainBase
    {
        private readonly IMetaPopulationBase metaPopulation;

        private IList<Domain> directSuperdomains;
        private Domain[] structuralDerivedSuperdomains;

        private string name;

        internal Domain(MetaPopulation metaPopulation, Guid id)
        {
            this.metaPopulation = metaPopulation;

            this.Id = id;
            this.Tag = id.Tag();

            this.directSuperdomains = new List<Domain>();

            this.metaPopulation.OnDomainCreated(this);
        }

        public Guid Id { get; }

        public string Tag { get; }

        public string Name
        {
            get => this.name;

            set
            {
                this.metaPopulation.AssertUnlocked();
                this.name = value;
                this.metaPopulation.Stale();
            }
        }

        IEnumerable<IDomain> IDomain.DirectSuperdomains => this.directSuperdomains;

        /// <summary>
        /// Gets the domains this domain extends, as declared. Their order plays no part.
        /// </summary>
        public IEnumerable<Domain> DirectSuperdomains => this.directSuperdomains;

        /// <summary>
        /// Gets all domains this domain extends, nearest first: the order of <see cref="DomainLinearization"/> without the domain itself.
        /// </summary>
        public IEnumerable<IDomainBase> Superdomains => this.structuralDerivedSuperdomains;

        IMetaPopulationBase IMetaObjectBase.MetaPopulation => this.metaPopulation;
        IMetaPopulation IMetaObject.MetaPopulation => this.metaPopulation;
        Origin IMetaObject.Origin => Origin.Database;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        public string ValidationName
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    return "domain " + this.Name;
                }

                return "unknown domain";
            }
        }

        // A cycle is an error of MetaPopulation.StructuralDerive, which names the domains on the cycle.
        public void AddDirectSuperdomain(Domain superdomain) => this.directSuperdomains.Add(superdomain);

        public override bool Equals(object other) => this.Id.Equals((other as Domain)?.Id);

        public override int GetHashCode() => this.Id.GetHashCode();

        /// <summary>
        /// Compares the current state with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this state.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This state is less than <paramref name="obj"/>. Zero This state is equal to <paramref name="obj"/>. Greater than zero This state is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="other"/> is not the same type as this state. </exception>
        public int CompareTo(object other) => this.Id.CompareTo((other as Domain)?.Id);

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return this.Name;
            }

            return this.Tag.ToString();
        }

        internal void Bind() => this.directSuperdomains = this.directSuperdomains.ToArray();

        internal void StructuralDeriveSuperdomains() =>
            this.structuralDerivedSuperdomains = DomainLinearization.Linearize(this).Skip(1).Cast<Domain>().ToArray();

        /// <summary>
        /// Validates the domain.
        /// </summary>
        /// <param name="validationLog">The validation.</param>
        internal void Validate(ValidationLog validationLog)
        {
            this.ValidateIdentity(validationLog);

            if (string.IsNullOrEmpty(this.Name))
            {
                validationLog.AddError("domain has no name", this, ValidationKind.Required, "Domain.Name");
            }
            else
            {
                if (!char.IsLetter(this.Name[0]))
                {
                    var message = this.ValidationName + " should start with an alfabetical character";
                    validationLog.AddError(message, this, ValidationKind.Format, "Domain.Name");
                }

                for (var i = 1; i < this.Name.Length; i++)
                {
                    if (!char.IsLetter(this.Name[i]) && !char.IsDigit(this.Name[i]))
                    {
                        var message = this.ValidationName + " should only contain alfanumerical characters)";
                        validationLog.AddError(message, this, ValidationKind.Format, "Domain.Name");
                        break;
                    }
                }
            }

            if (this.Id == Guid.Empty)
            {
                validationLog.AddError(this.ValidationName + " has no id", this, ValidationKind.Required, "IMetaObject.Id");
            }
        }
    }
}
