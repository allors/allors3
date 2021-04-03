// <copyright file="MethodType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract partial class MethodType : OperandType, IMethodTypeBase, IComparable
    {
        private static readonly IReadOnlyDictionary<IClassBase, IMethodClassBase> EmptyMethodClassByAssociationTypeClass = new ReadOnlyDictionary<IClassBase, IMethodClassBase>(new Dictionary<IClassBase, IMethodClassBase>());

        private IReadOnlyDictionary<IClassBase, IMethodClassBase> derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;
        
        protected MethodType(IMetaPopulationBase metaPopulation) : base(metaPopulation)
        {
        }

        //public Dictionary<string, bool> Workspace => this.WorkspaceNames.ToDictionary(k => k, v => true);
        
        public abstract Guid Id { get; }
        public abstract string IdAsString { get; }

        public override Origin Origin => Origin.Database;

        IComposite IMethodType.ObjectType => this.Composite;

        public abstract ICompositeBase Composite { get; }

        public abstract string[] WorkspaceNames { get; }

        public abstract string Name { get; set; }

        public override string DisplayName => this.Name;

        public abstract string FullName { get; }

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

        public IReadOnlyDictionary<IClassBase, IMethodClassBase> MethodClassByClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedMethodClassByClass;
            }
        }

        public IMethodClassBase MethodClassBy(IClassBase @class) =>
            this switch
            {
                MethodClass methodClass => methodClass,
                MethodInterface methodInterface => this.MethodClassByClass[@class],
                _ => null,
            };

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => this.Name;

        void IMethodTypeBase.DeriveMethodClasses()
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

        public abstract void DeriveWorkspaceNames();

        /// <summary>
        /// Validates the state.
        /// </summary>
        /// <param name="validationLog">The validation.</param>
        void IMethodTypeBase.Validate(ValidationLog validationLog)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                var message = this.ValidationName + " has no name";
                validationLog.AddError(message, this, ValidationKind.Required, "MethodType.Name");
            }
        }

        public override bool Equals(object other) => this.Id.Equals((other as MethodType)?.Id);

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
        public int CompareTo(object other) => this.Id.CompareTo((other as MethodType)?.Id);
    }
}
