// <copyright file="MethodType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public abstract partial class MethodType : OperandType, IMethodType, IComparable
    {
        private static readonly IReadOnlyDictionary<Class, MethodClass> EmptyMethodClassByAssociationTypeClass = new ReadOnlyDictionary<Class, MethodClass>(new Dictionary<Class, MethodClass>());

        private IReadOnlyDictionary<Class, MethodClass> derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;

        private string[] assignedWorkspaceNames;
        private string[] derivedWorkspaceNames;

        protected MethodType(MetaPopulation metaPopulation) : base(metaPopulation)
        {
        }

        //public Dictionary<string, bool> Workspace => this.WorkspaceNames.ToDictionary(k => k, v => true);

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

        public abstract Guid Id { get; }
        public abstract string IdAsString { get; }

        public override Origin Origin => Origin.Database;

        IComposite IMethodType.ObjectType => this.Composite;

        public abstract Composite Composite { get; }

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

        public IReadOnlyDictionary<Class, MethodClass> MethodClassByClass
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedMethodClassByClass;
            }
        }

        public MethodClass MethodClassBy(Class @class) =>
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

        internal void DeriveWorkspaceNames()
        {
            this.derivedWorkspaceNames = this.assignedWorkspaceNames?.Length > 0 ?
                (this.Composite switch
                {
                    Interface @interface => @interface.Classes.SelectMany(v => v.WorkspaceNames).ToArray(),
                    Class @class => @class.WorkspaceNames,
                    _ => Array.Empty<string>()
                }).Intersect(this.assignedWorkspaceNames).ToArray()
                : Array.Empty<string>();

            foreach (var methodClass in this.MethodClassByClass.Values)
            {
                methodClass.DeriveWorkspaceNames();
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
