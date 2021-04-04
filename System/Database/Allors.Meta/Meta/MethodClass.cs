// <copyright file="MethodClass.cs" company="Allors bvba">
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
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed partial class MethodClass : IMethodClassBase, IComparable
    {
        private static readonly IReadOnlyDictionary<IClassBase, IMethodClassBase> EmptyMethodClassByAssociationTypeClass = new ReadOnlyDictionary<IClassBase, IMethodClassBase>(new Dictionary<IClassBase, IMethodClassBase>());

        private IReadOnlyDictionary<IClassBase, IMethodClassBase> derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;

        private readonly IMetaPopulationBase metaPopulation;

        public IReadOnlyDictionary<IClassBase, IMethodClassBase> MethodClassByClass
        {
            get
            {
                this.metaPopulation.Derive();
                return this.derivedMethodClassByClass;
            }
        }

        public MethodClassProps _ => this.props ??= new MethodClassProps(this);

        IMetaPopulationBase IMetaObjectBase.MetaPopulation => this.metaPopulation;
        IMetaPopulation IMetaObject.MetaPopulation => this.metaPopulation;
        Origin IMetaObject.Origin => Origin.Database;

        IComposite IMethodType.ObjectType => this.Composite;
        IMethodClass IMethodType.MethodClassBy(IClass @class) => this.MethodClassBy(@class);

        public string DisplayName => this.Name;

        /// <summary>
        /// Gets the validation name.
        /// </summary>
        /// <value>The validation name.</value>
        public string ValidationName
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


        public IMethodClassBase MethodClassBy(IClass @class) => this;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString() => this.Name;

        void IMethodTypeBase.DeriveMethodClasses() => this.derivedMethodClassByClass = EmptyMethodClassByAssociationTypeClass;

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

        private string[] assignedWorkspaceNames;
        private string[] derivedWorkspaceNames;

        private string name;
        private IClassBase @class;
        private MethodClassProps props;

        private MethodClass(IClassBase @class, Guid id, IMethodInterfaceBase methodInterface)
        {
            this.metaPopulation = @class.MetaPopulation;

            this.Class = @class;
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");
            this.MethodInterface = methodInterface;

            this.metaPopulation.OnMethodClassCreated(this);
        }

        public MethodClass(IClassBase @class, Guid id) : this(@class, id, null)
        {
        }

        public MethodClass(IClassBase @class, IMethodInterfaceBase methodInterface) : this(@class, methodInterface.Id, methodInterface)
        {
        }

        public Guid Id { get; }

        public string IdAsString { get; }

        IMethodInterface IMethodClass.MethodInterface => this.MethodInterface;

        public IMethodInterfaceBase MethodInterface { get; }

        public bool ExistMethodInterface => this.MethodInterface != null;

        public string[] AssignedWorkspaceNames
        {
            get => this.assignedWorkspaceNames;

            set
            {
                this.metaPopulation.AssertUnlocked();
                this.assignedWorkspaceNames = value;
                this.metaPopulation.Stale();
            }
        }

        public string[] WorkspaceNames
        {
            get
            {
                if (this.ExistMethodInterface)
                {
                    return this.MethodInterface.WorkspaceNames;
                }

                this.metaPopulation.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        public string Name
        {
            get => this.name ?? this.MethodInterface.Name;

            set
            {
                if (this.ExistMethodInterface)
                {
                    throw new ArgumentException("Name is readonly when ExistMethodInterface");
                }

                this.metaPopulation.AssertUnlocked();
                this.name = value;
                this.metaPopulation.Stale();
            }
        }

        public string FullName => this.MethodInterface != null
            ? this.MethodInterface.FullName
            : $"{this.Composite.Name}{this.Name}";

        public ICompositeBase Composite => this.Class;

        IClass IMethodClass.ObjectType => this.Class;

        public IClassBase Class
        {
            get => this.@class;

            set
            {
                this.metaPopulation.AssertUnlocked();
                this.@class = value;
                this.metaPopulation.Stale();
            }
        }

        public IList<Action<object, object>> Actions { get; private set; }
        IEnumerable<Action<object, object>> IMethodClass.Actions => this.Actions;

        public void DeriveWorkspaceNames() =>
            this.derivedWorkspaceNames = this.assignedWorkspaceNames != null
                ? this.assignedWorkspaceNames.Intersect(this.Composite.WorkspaceNames).ToArray()
                : Array.Empty<string>();

        public void ResetDerivedWorkspaceNames() => this.derivedWorkspaceNames = null;

        public void Bind(List<Domain> sortedDomains, MethodInfo[] extensionMethods, Dictionary<Type, Dictionary<MethodInfo, Action<object, object>>> actionByMethodInfoByType)
        {
            this.Actions = new List<Action<object, object>>();

            var interfaces = new List<IInterfaceBase>(this.Class.Supertypes);

            interfaces.Sort(
                (a, b) =>
                {
                    if (a.Supertypes.Contains(b))
                    {
                        return 1;
                    }

                    if (a.Subtypes.Contains(b))
                    {
                        return -1;
                    }

                    return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                });

            // Interface
            foreach (var @interface in interfaces)
            {
                foreach (var domain in sortedDomains)
                {
                    var methodName = domain.Name + this.Name;
                    var extensionMethodInfos = GetExtensionMethods(extensionMethods, @interface.ClrType, methodName);
                    if (extensionMethodInfos.Length > 1)
                    {
                        throw new Exception("Interface " + @interface + " has 2 extension methods for " + methodName);
                    }

                    if (extensionMethodInfos.Length == 1)
                    {
                        var methodInfo = extensionMethodInfos[0];

                        if (!actionByMethodInfoByType.TryGetValue(this.Class.ClrType, out var actionByMethodInfo))
                        {
                            actionByMethodInfo = new Dictionary<MethodInfo, Action<object, object>>();
                            actionByMethodInfoByType[this.Class.ClrType] = actionByMethodInfo;
                        }

                        if (!actionByMethodInfo.TryGetValue(methodInfo, out var action))
                        {
                            var o = Expression.Parameter(typeof(object));
                            var castO = Expression.Convert(o, methodInfo.GetParameters()[0].ParameterType);

                            var p = Expression.Parameter(typeof(object));
                            var castP = Expression.Convert(p, methodInfo.GetParameters()[1].ParameterType);

                            Expression call = Expression.Call(methodInfo, new Expression[] { castO, castP });

                            action = Expression.Lambda<Action<object, object>>(call, o, p).Compile();
                            actionByMethodInfo[methodInfo] = action;
                        }

                        this.Actions.Add(action);
                    }
                }
            }

            // Class
            {
                foreach (var domain in sortedDomains)
                {
                    var methodName = domain.Name + this.Name;

                    var methodInfo = this.Class.ClrType.GetTypeInfo().GetDeclaredMethod(methodName);
                    if (methodInfo != null)
                    {
                        var o = Expression.Parameter(typeof(object));
                        var castO = Expression.Convert(o, this.Class.ClrType);

                        var p = Expression.Parameter(typeof(object));
                        var castP = Expression.Convert(p, methodInfo.GetParameters()[0].ParameterType);

                        Expression call = Expression.Call(castO, methodInfo, castP);

                        var action = Expression.Lambda<Action<object, object>>(call, o, p).Compile();
                        this.Actions.Add(action);
                    }
                }
            }
        }

        private static MethodInfo[] GetExtensionMethods(MethodInfo[] extensionMethods, Type @interface, string methodName)
        {
            var query = from method in extensionMethods
                        where method.Name.Equals(methodName)
                        where method.GetParameters()[0].ParameterType == @interface
                        select method;
            return query.ToArray();
        }

        void IMethodClassBase.ResetDerivedWorkspaceNames() => this.ResetDerivedWorkspaceNames();
    }
}
