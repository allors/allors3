// <copyright file="MethodType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract partial class MethodType : OperandType, IMethodType
    {
        private static readonly IReadOnlyDictionary<Class, MethodClass> EmptyMethodClassByAssociationTypeClass = new ReadOnlyDictionary<Class, MethodClass>(new Dictionary<Class, MethodClass>());

        private IReadOnlyDictionary<Class, MethodClass> derivedMethodClassByClass;

        protected MethodType(MetaPopulation metaPopulation) : base(metaPopulation)
        {
        }

        public abstract Guid Id { get; }
        public abstract string IdAsString { get; }

        public override Origin Origin => Origin.Local;

        IComposite IMethodType.ObjectType => this.Composite;

        public abstract Composite Composite { get; }

        public abstract string Name { get; set; }

        public override string DisplayName => this.Name;

        public string FullName => this.Composite != null ? this.Composite.Name + this.Name : this.Name;

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

        internal void DeriveMethodClasses()
        {
            if (this.Composite is Interface @interface)
            {
                this.derivedMethodClassByClass = @interface.Classes.ToDictionary(v => v, v =>
                {
                    if (!this.derivedMethodClassByClass.TryGetValue(v, out var methodClass))
                    {
                        methodClass = new MethodClass(v, (MethodInterface)this);
                    }

                    return methodClass;
                });
            }
            else
            {
                this.derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;
            }
        }

        /// <summary>
        /// Validates the instance.
        /// </summary>
        /// <param name="validationLog">The validation.</param>
        protected internal void Validate(ValidationLog validationLog)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                var message = this.ValidationName + " has no name";
                validationLog.AddError(message, this, ValidationKind.Required, "MethodType.Name");
            }
        }

        public bool Equals(IMetaIdentity other) => this.Id.Equals(other?.Id);
    }
}
