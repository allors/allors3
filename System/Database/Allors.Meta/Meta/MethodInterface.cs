// <copyright file="MethodInterface.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public sealed partial class MethodInterface : OperandType, IMethodInterfaceBase, IComparable
    {
        private static readonly IReadOnlyDictionary<IClass, IMethodClassBase> EmptyMethodClassByAssociationTypeClass = new ReadOnlyDictionary<IClass, IMethodClassBase>(new Dictionary<IClass, IMethodClassBase>());

        private IReadOnlyDictionary<IClass, IMethodClassBase> derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;

        //public Dictionary<string, bool> Workspace => this.WorkspaceNames.ToDictionary(k => k, v => true);

        private string[] assignedWorkspaceNames;
        private string[] derivedWorkspaceNames;

        private string name;

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

        public IReadOnlyDictionary<IClass, IMethodClassBase> MethodClassByClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedMethodClassByClass;
            }
        }

        IMethodClass IMethodType.MethodClassBy(IClass @class) => this.MethodClassByClass[@class];
        IMethodClassBase IMethodTypeBase.MethodClassBy(IClass @class) => this.MethodClassByClass[@class];

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => this.Name;

        void IMethodTypeBase.DeriveMethodClasses()
        {
            if (this.Composite is IInterfaceBase @interface)
            {
                this.derivedMethodClassByClass = @interface.Classes.ToDictionary(v => (IClass)v, v =>
                {
                    if (!this.derivedMethodClassByClass.TryGetValue(v, out var methodClass))
                    {
                        methodClass = new MethodClass(v, this);
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

        public override bool Equals(object other) => this.Id.Equals((other as IMethodTypeBase)?.Id);

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
        public int CompareTo(object other) => this.Id.CompareTo((other as IMethodTypeBase)?.Id);

        public MethodInterface(Interface @interface, Guid id) : base(@interface.MetaPopulation)
        {
            this.Interface = @interface;
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");

            this.MetaPopulation.OnMethodInterfaceCreated(this);
        }

        public Guid Id { get; }

        public string IdAsString { get; }

        public Interface Interface { get; }
        public ICompositeBase Composite => this.Interface;

        public string[] AssignedWorkspaceNames
        {
            get => this.assignedWorkspaceNames;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.assignedWorkspaceNames = value;
                this.MetaPopulation.Stale();
            }
        }

        public string[] WorkspaceNames
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        public string Name
        {
            get => this.name;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.name = value;
                this.MetaPopulation.Stale();
            }
        }

        public string FullName => $"{this.Composite.Name}{this.Name}";

        public void DeriveWorkspaceNames()
        {
            this.derivedWorkspaceNames = this.assignedWorkspaceNames != null
                ? this.assignedWorkspaceNames.Intersect(this.Composite.Classes.SelectMany(v => v.WorkspaceNames)).ToArray()
                : Array.Empty<string>();

            foreach (var methodClass in this.MethodClassByClass.Values)
            {
                methodClass.ResetDerivedWorkspaceNames();
            }
        }
    }
}
