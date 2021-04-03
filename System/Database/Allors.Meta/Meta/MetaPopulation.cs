// <copyright file="MetaPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed partial class MetaPopulation : IMetaPopulationBase
    {
        private readonly Dictionary<Guid, MetaObjectBase> metaObjectById;

        private string[] derivedWorkspaceNames;

        private Dictionary<string, Class> derivedClassByLowercaseName;

        private IList<Domain> domains;
        private IList<Unit> units;
        private IList<Interface> interfaces;
        private IList<Class> classes;
        private IList<Inheritance> inheritances;
        private IList<RelationType> relationTypes;
        private IList<AssociationType> associationTypes;
        private IList<RoleType> roleTypes;
        private IList<MethodType> methodTypes;

        private bool isStale;
        private bool isDeriving;

        private Composite[] derivedComposites;

        private Composite[] derivedDatabaseComposites;
        private Interface[] derivedDatabaseInterfaces;
        private Class[] derivedDatabaseClasses;
        private RelationType[] derivedDatabaseRelationTypes;

        public MetaPopulation()
        {
            this.isStale = true;
            this.isDeriving = false;

            this.domains = new List<Domain>();
            this.units = new List<Unit>();
            this.interfaces = new List<Interface>();
            this.classes = new List<Class>();
            this.inheritances = new List<Inheritance>();
            this.relationTypes = new List<RelationType>();
            this.associationTypes = new List<AssociationType>();
            this.roleTypes = new List<RoleType>();
            this.methodTypes = new List<MethodType>();

            this.metaObjectById = new Dictionary<Guid, MetaObjectBase>();
        }

        public string[] WorkspaceNames
        {
            get
            {
                this.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        public bool IsBound { get; private set; }

        public IEnumerable<Domain> Domains => this.domains;

        public IEnumerable<Domain> SortedDomains
        {
            get
            {
                var sortedDomains = new List<Domain>(this.domains);
                sortedDomains.Sort((x, y) => x.Superdomains.Contains(y) ? -1 : 1);
                return sortedDomains.ToArray();
            }
        }

        public IEnumerable<Unit> Units => this.units;

        public IEnumerable<Interface> Interfaces => this.interfaces;

        public IEnumerable<Class> Classes => this.classes;

        public IEnumerable<Inheritance> Inheritances => this.inheritances;

        public IEnumerable<RelationType> RelationTypes => this.relationTypes;

        public IEnumerable<AssociationType> AssociationTypes => this.associationTypes;

        public IEnumerable<RoleType> RoleTypes => this.roleTypes;

        public IEnumerable<MethodType> MethodTypes => this.methodTypes;

        public IEnumerable<Composite> Composites
        {
            get
            {
                this.Derive();
                return this.derivedComposites;
            }
        }

        public IEnumerable<Composite> SortedComposites => this.Composites.OrderBy(v => v.Name);

        /// <summary>
        /// Gets a value indicating whether this state is valid.
        /// </summary>
        /// <value><c>true</c> if this state is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                var validation = this.Validate();
                if (validation.ContainsErrors)
                {
                    return false;
                }

                return true;
            }
        }

        public IEnumerable<IComposite> DatabaseComposites
        {
            get
            {
                this.Derive();
                return this.derivedDatabaseComposites;
            }
        }

        public IEnumerable<IInterface> DatabaseInterfaces
        {
            get
            {
                this.Derive();
                return this.derivedDatabaseInterfaces;
            }
        }

        public IEnumerable<IClass> DatabaseClasses
        {
            get
            {
                this.Derive();
                return this.derivedDatabaseClasses;
            }
        }

        public IEnumerable<IRelationType> DatabaseRelationTypes
        {
            get
            {
                this.Derive();
                return this.derivedDatabaseRelationTypes;
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<Composite>> WorkspaceCompositesByWorkspaceName
        {
            get
            {
                this.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.Composites.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<Interface>> WorkspaceInterfacesByWorkspaceName
        {
            get
            {
                this.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.Interfaces.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<Class>> WorkspaceClassesByWorkspaceName
        {
            get
            {
                this.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.Classes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<RelationType>> WorkspaceRelationTypesByWorkspaceName
        {
            get
            {
                this.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.RelationTypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        public IReadOnlyDictionary<string, IEnumerable<MethodType>> WorkspaceMethodTypesByWorkspaceName
        {
            get
            {
                this.Derive();
                return this.WorkspaceNames
                    .ToDictionary(v => v, v => this.MethodTypes.Where(w => w.WorkspaceNames.Contains(v)));
            }
        }

        IMetaObject IMetaPopulation.Find(Guid id) => this.Find(id);

        /// <summary>
        /// Find a meta object by meta object id.
        /// </summary>
        /// <param name="id">
        /// The meta object id.
        /// </param>
        /// <returns>
        /// The <see cref="IMetaObject"/>.
        /// </returns>
        public MetaObjectBase Find(Guid id)
        {
            this.metaObjectById.TryGetValue(id, out var metaObject);

            return metaObject;
        }

        /// <summary>
        /// Find a meta object by name.
        /// </summary>
        /// <param name="name">
        /// The meta object id.
        /// </param>
        /// <returns>
        /// The <see cref="IMetaObject"/>.
        /// </returns>
        public Class FindByName(string name)
        {
            this.Derive();

            this.derivedClassByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var cls);

            return cls;
        }

        IValidationLog IMetaPopulation.Validate() => this.Validate();

        /// <summary>
        /// Validates this state.
        /// </summary>
        /// <returns>The Validate.</returns>
        public ValidationLog Validate()
        {
            var log = new ValidationLog();

            foreach (var domain in this.Domains)
            {
                domain.Validate(log);
            }

            foreach (var unitType in this.Units)
            {
                unitType.Validate(log);
            }

            foreach (var @interface in this.Interfaces)
            {
                @interface.Validate(log);
            }

            foreach (var @class in this.Classes)
            {
                @class.Validate(log);
            }

            foreach (var inheritance in this.Inheritances)
            {
                inheritance.Validate(log);
            }

            foreach (var relationType in this.RelationTypes)
            {
                relationType.Validate(log);
            }

            foreach (var methodType in this.MethodTypes)
            {
                methodType.Validate(log);
            }

            var inheritancesBySubtype = new Dictionary<Composite, List<Inheritance>>();
            foreach (var inheritance in this.Inheritances)
            {
                var subtype = inheritance.Subtype;
                if (subtype != null)
                {
                    if (!inheritancesBySubtype.TryGetValue(subtype, out var inheritanceList))
                    {
                        inheritanceList = new List<Inheritance>();
                        inheritancesBySubtype[subtype] = inheritanceList;
                    }

                    inheritanceList.Add(inheritance);
                }
            }

            var supertypes = new HashSet<Interface>();
            foreach (var subtype in inheritancesBySubtype.Keys)
            {
                supertypes.Clear();
                if (this.HasCycle(subtype, supertypes, inheritancesBySubtype))
                {
                    var message = subtype.ValidationName + " has a cycle in its inheritance hierarchy";
                    log.AddError(message, subtype, ValidationKind.Cyclic, "IComposite.Supertypes");
                }
            }

            return log;
        }

        public void Bind(Type[] types, MethodInfo[] methods)
        {
            if (!this.IsBound)
            {
                this.Derive();

                this.IsBound = true;

                this.domains = this.domains.ToArray();
                this.units = this.units.ToArray();
                this.interfaces = this.interfaces.ToArray();
                this.classes = this.classes.ToArray();
                this.inheritances = this.inheritances.ToArray();
                this.relationTypes = this.relationTypes.ToArray();
                this.associationTypes = this.associationTypes.ToArray();
                this.roleTypes = this.roleTypes.ToArray();
                this.methodTypes = this.methodTypes.ToArray();

                foreach (var domain in this.domains)
                {
                    domain.Bind();
                }

                var typeByName = types.ToDictionary(type => type.Name, type => type);

                foreach (var unit in this.Units)
                {
                    unit.Bind();
                }

                foreach (Interface @interface in this.DatabaseInterfaces)
                {
                    @interface.Bind(typeByName);
                }

                foreach (Class @class in this.DatabaseClasses)
                {
                    @class.Bind(typeByName);
                }

                var sortedDomains = new List<Domain>(this.domains);
                sortedDomains.Sort((a, b) => a.Superdomains.Contains(b) ? -1 : 1);

                var actionByMethodInfoByType = new Dictionary<Type, Dictionary<MethodInfo, Action<object, object>>>();

                foreach (Class @class in this.DatabaseClasses)
                {
                    foreach (MethodClass concreteMethodType in @class.MethodTypes)
                    {
                        concreteMethodType.Bind(sortedDomains, methods, actionByMethodInfoByType);
                    }
                }
            }
        }

        void IMetaPopulationBase.AssertUnlocked()
        {
            if (this.IsBound)
            {
                throw new Exception("Environment is locked");
            }
        }

        void IMetaPopulationBase.Derive() => this.Derive();

        private void Derive()
        {
            if (this.isStale && !this.isDeriving)
            {
                try
                {
                    this.isDeriving = true;

                    var sharedDomains = new HashSet<Domain>();
                    var sharedCompositeTypes = new HashSet<Composite>();
                    var sharedInterfaces = new HashSet<Interface>();
                    var sharedClasses = new HashSet<Class>();
                    var sharedAssociationTypes = new HashSet<AssociationType>();
                    var sharedRoleTypes = new HashSet<RoleType>();
                    var sharedMethodTypes = new HashSet<MethodType>();

                    // Domains
                    foreach (var domain in this.domains)
                    {
                        domain.DeriveSuperdomains(sharedDomains);
                    }

                    // Unit & IComposite ObjectTypes
                    var compositeTypes = new List<Composite>(this.Interfaces);
                    compositeTypes.AddRange(this.Classes);
                    this.derivedComposites = compositeTypes.ToArray();

                    // Database
                    this.derivedDatabaseComposites = this.derivedComposites.Where(v => v.Origin == Origin.Database).ToArray();
                    this.derivedDatabaseInterfaces = this.interfaces.Where(v => v.Origin == Origin.Database).ToArray();
                    this.derivedDatabaseClasses = this.classes.Where(v => v.Origin == Origin.Database).ToArray();
                    this.derivedDatabaseRelationTypes = this.relationTypes.Where(v => v.Origin == Origin.Database).ToArray();

                    // DirectSupertypes
                    foreach (var type in this.derivedComposites)
                    {
                        type.DeriveDirectSupertypes(sharedInterfaces);
                    }

                    // DirectSubtypes
                    foreach (var type in this.Interfaces)
                    {
                        type.DeriveDirectSubtypes(sharedCompositeTypes);
                    }

                    // Supertypes
                    foreach (var type in this.derivedComposites)
                    {
                        type.DeriveSupertypes(sharedInterfaces);
                    }

                    // isSynced
                    foreach (var composite in this.Composites)
                    {
                        composite.DeriveIsSynced();
                    }

                    // Subtypes
                    foreach (var type in this.Interfaces)
                    {
                        type.DeriveSubtypes(sharedCompositeTypes);
                    }

                    // Subclasses
                    foreach (var type in this.Interfaces)
                    {
                        type.DeriveSubclasses(sharedClasses);
                    }

                    // Exclusive Subclass
                    foreach (var type in this.Interfaces)
                    {
                        type.DeriveExclusiveSubclass();
                    }

                    // RoleTypes & AssociationTypes
                    var roleTypesByAssociationTypeObjectType = this.RelationTypes
                        .GroupBy(v => v.AssociationType.ObjectType)
                        .ToDictionary(g => g.Key, g => new HashSet<RoleType>(g.Select(v => v.RoleType)));


                    var associationTypesByRoleTypeObjectType = this.RelationTypes
                        .GroupBy(v => v.RoleType.ObjectType)
                        .ToDictionary(g => g.Key, g => new HashSet<AssociationType>(g.Select(v => v.AssociationType)));

                    // RoleTypes
                    foreach (var type in this.derivedComposites)
                    {
                        type.DeriveRoleTypes(sharedRoleTypes, roleTypesByAssociationTypeObjectType);
                    }

                    // AssociationTypes
                    foreach (var type in this.derivedComposites)
                    {
                        type.DeriveAssociationTypes(sharedAssociationTypes, associationTypesByRoleTypeObjectType);
                    }

                    // RoleType
                    foreach (var relationType in this.RelationTypes)
                    {
                        relationType.RoleType.DeriveScaleAndSize();
                    }

                    // RelationType Multiplicity
                    foreach (var relationType in this.RelationTypes)
                    {
                        relationType.DeriveMultiplicity();
                    }

                    var sharedMethodTypeList = new HashSet<MethodType>();

                    // MethodClasses
                    foreach (var methodType in this.MethodTypes)
                    {
                        methodType.DeriveMethodClasses();
                    }

                    // MethodTypes
                    var methodTypeByClass = this.MethodTypes
                        .GroupBy(v => v.Composite)
                        .ToDictionary(g => g.Key, g => new HashSet<MethodType>(g));

                    foreach (var type in this.derivedComposites)
                    {
                        type.DeriveMethodTypes(sharedMethodTypeList, methodTypeByClass);
                    }

                    // WorkspaceNames
                    var workspaceNames = new HashSet<string>();
                    foreach (var @class in this.classes)
                    {
                        @class.DeriveWorkspaceNames(workspaceNames);
                    }

                    this.derivedWorkspaceNames = workspaceNames.ToArray();

                    foreach (var relationType in this.relationTypes)
                    {
                        relationType.DeriveWorkspaceNames();
                    }

                    foreach (var methodType in this.methodTypes)
                    {
                        methodType.DeriveWorkspaceNames();
                    }

                    foreach (var @interface in this.interfaces)
                    {
                        @interface.DeriveWorkspaceNames();
                    }

                    // MetaPopulation
                    this.derivedClassByLowercaseName = new Dictionary<string, Class>();
                    foreach (var cls in this.classes)
                    {
                        this.derivedClassByLowercaseName[cls.Name.ToLowerInvariant()] = cls;
                    }
                }
                finally
                {
                    // Ignore stale requests during a derivation
                    this.isStale = false;
                    this.isDeriving = false;
                }
            }
        }

        void IMetaPopulationBase.OnDomainCreated(Domain domain)
        {
            this.domains.Add(domain);
            this.metaObjectById.Add(domain.Id, domain);

            this.Stale();
        }

        internal void OnUnitCreated(Unit unit)
        {
            this.units.Add(unit);
            this.metaObjectById.Add(unit.Id, unit);

            this.Stale();
        }

        internal void OnInterfaceCreated(Interface @interface)
        {
            this.interfaces.Add(@interface);
            this.metaObjectById.Add(@interface.Id, @interface);

            this.Stale();
        }

        internal void OnClassCreated(Class @class)
        {
            this.classes.Add(@class);
            this.metaObjectById.Add(@class.Id, @class);

            this.Stale();
        }

        internal void OnInheritanceCreated(Inheritance inheritance)
        {
            this.inheritances.Add(inheritance);
            this.Stale();
        }

        void IMetaPopulationBase.OnRelationTypeCreated(RelationType relationType)
        {
            this.relationTypes.Add(relationType);
            this.metaObjectById.Add(relationType.Id, relationType);

            this.Stale();
        }

        void IMetaPopulationBase.OnAssociationTypeCreated(AssociationType associationType) => this.Stale();

        void IMetaPopulationBase.OnRoleTypeCreated(RoleType roleType) => this.Stale();

        void IMetaPopulationBase.OnMethodInterfaceCreated(MethodInterface methodInterface)
        {
            this.methodTypes.Add(methodInterface);
            this.metaObjectById.Add(methodInterface.Id, methodInterface);

            this.Stale();
        }

        void IMetaPopulationBase.OnMethodClassCreated(MethodClass methodClass)
        {
            if (methodClass.MethodInterface == null)
            {
                this.methodTypes.Add(methodClass);
                this.metaObjectById.Add(methodClass.Id, methodClass);
            }

            this.Stale();
        }


        void IMetaPopulationBase.Stale() => this.Stale();
        private void Stale() => this.isStale = true;


        private bool HasCycle(Composite subtype, HashSet<Interface> supertypes, Dictionary<Composite, List<Inheritance>> inheritancesBySubtype)
        {
            foreach (var inheritance in inheritancesBySubtype[subtype])
            {
                var supertype = inheritance.Supertype;
                if (supertype != null)
                {
                    if (this.HasCycle(subtype, supertype, supertypes, inheritancesBySubtype))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasCycle(Composite originalSubtype, Interface currentSupertype, HashSet<Interface> supertypes, Dictionary<Composite, List<Inheritance>> inheritancesBySubtype)
        {
            if (originalSubtype is Interface && supertypes.Contains((Interface)originalSubtype))
            {
                return true;
            }

            if (!supertypes.Contains(currentSupertype))
            {
                supertypes.Add(currentSupertype);

                if (inheritancesBySubtype.TryGetValue(currentSupertype, out var currentSuperInheritances))
                {
                    foreach (var inheritance in currentSuperInheritances)
                    {
                        var newSupertype = inheritance.Supertype;
                        if (newSupertype != null)
                        {
                            if (this.HasCycle(originalSubtype, newSupertype, supertypes, inheritancesBySubtype))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public MethodType MethodType(string id) => ((MethodType)this.Find(new Guid(id)));

        public RoleType RoleType(string id) => ((RelationType)this.Find(new Guid(id))).RoleType;
    }
}
