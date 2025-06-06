// <copyright file="MetaPopulation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
        private readonly Dictionary<Guid, IMetaIdentifiableObjectBase> metaObjectById;
        private readonly Dictionary<string, IMetaIdentifiableObjectBase> metaObjectByTag;

        private string[] derivedWorkspaceNames;

        private Dictionary<string, ICompositeBase> derivedDatabaseCompositeByLowercaseName;

        private IList<Domain> domains;
        private IList<IUnitBase> units;
        private IList<IInterfaceBase> interfaces;
        private IList<IClassBase> classes;
        private IList<Inheritance> inheritances;
        private IList<IRelationTypeBase> relationTypes;
        private IList<IAssociationTypeBase> associationTypes;
        private IList<IRoleTypeBase> roleTypes;
        private IList<IMethodTypeBase> methodTypes;

        private bool isStale;
        private bool isDeriving;
        private bool isStructuralDeriving;

        private ICompositeBase[] structuralDerivedComposites;

        private ICompositeBase[] structuralDerivedDatabaseComposites;
        private IInterfaceBase[] structuralDerivedDatabaseInterfaces;
        private IClassBase[] structuralDerivedDatabaseClasses;
        private IRelationTypeBase[] structuralDerivedDatabaseRelationTypes;

        public MetaPopulation()
        {
            this.isStale = true;
            this.isDeriving = false;

            this.domains = new List<Domain>();
            this.units = new List<IUnitBase>();
            this.interfaces = new List<IInterfaceBase>();
            this.classes = new List<IClassBase>();
            this.inheritances = new List<Inheritance>();
            this.relationTypes = new List<IRelationTypeBase>();
            this.associationTypes = new List<IAssociationTypeBase>();
            this.roleTypes = new List<IRoleTypeBase>();
            this.methodTypes = new List<IMethodTypeBase>();

            this.metaObjectById = new Dictionary<Guid, IMetaIdentifiableObjectBase>();
            this.metaObjectByTag = new Dictionary<string, IMetaIdentifiableObjectBase>();
        }

        public MethodCompiler MethodCompiler { get; private set; }

        public IEnumerable<string> WorkspaceNames
        {
            get
            {
                this.Derive();
                return this.derivedWorkspaceNames;
            }
        }

        private bool IsBound { get; set; }

        IEnumerable<IDomain> IMetaPopulation.Domains => this.Domains;
        public IEnumerable<IDomainBase> Domains => this.domains;

        IEnumerable<IClass> IMetaPopulation.Classes => this.classes;
        public IEnumerable<IClassBase> Classes => this.classes;

        IEnumerable<IInheritanceBase> IMetaPopulationBase.Inheritances => this.Inheritances;

        public IEnumerable<Inheritance> Inheritances => this.inheritances;

        IEnumerable<IRelationType> IMetaPopulation.RelationTypes => this.relationTypes;
        public IEnumerable<IRelationTypeBase> RelationTypes => this.relationTypes;

        public IEnumerable<IAssociationTypeBase> AssociationTypes => this.associationTypes;

        public IEnumerable<IRoleTypeBase> RoleTypes => this.roleTypes;

        IEnumerable<IInterface> IMetaPopulation.Interfaces => this.interfaces;

        IEnumerable<IComposite> IMetaPopulation.Composites => this.Composites;
        public IEnumerable<ICompositeBase> Composites => this.structuralDerivedComposites;

        /// <summary>
        /// Gets a value indicating whether this state is valid.
        /// </summary>
        /// <value><c>true</c> if this state is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                var validation = this.Validate();
                return !validation.ContainsErrors;
            }
        }

        IEnumerable<IUnit> IMetaPopulation.Units => this.Units;
        public IEnumerable<IUnitBase> Units => this.units;

        IEnumerable<IComposite> IMetaPopulation.DatabaseComposites => this.DatabaseComposites;
        public IEnumerable<ICompositeBase> DatabaseComposites => this.structuralDerivedDatabaseComposites;

        IEnumerable<IInterface> IMetaPopulation.DatabaseInterfaces => this.DatabaseInterfaces;
        public IEnumerable<IInterfaceBase> DatabaseInterfaces => this.structuralDerivedDatabaseInterfaces;

        IEnumerable<IClass> IMetaPopulation.DatabaseClasses => this.DatabaseClasses;
        public IEnumerable<IClassBase> DatabaseClasses => this.structuralDerivedDatabaseClasses;

        IEnumerable<IRelationType> IMetaPopulation.DatabaseRelationTypes => this.DatabaseRelationTypes;
        public IEnumerable<IRelationTypeBase> DatabaseRelationTypes => this.structuralDerivedDatabaseRelationTypes;

        IEnumerable<IMethodType> IMetaPopulation.MethodTypes => this.MethodTypes;
        public IEnumerable<IMethodTypeBase> MethodTypes => this.methodTypes;

        IMetaIdentifiableObject IMetaPopulation.FindById(Guid id) => this.FindById(id);

        IMetaIdentifiableObject IMetaPopulation.FindByTag(string tag) => this.FindByTag(tag);

        /// <summary>
        /// Find a meta object by id.
        /// </summary>
        /// <param name="id">
        /// The meta object id.
        /// </param>
        /// <returns>
        /// The <see cref="IMetaObject"/>.
        /// </returns>
        public IMetaIdentifiableObjectBase FindById(Guid id)
        {
            this.metaObjectById.TryGetValue(id, out var metaObject);

            return metaObject;
        }

        /// <summary>
        /// Find a meta object by tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>
        /// The <see cref="IMetaObject"/>.
        /// </returns>
        public IMetaIdentifiableObjectBase FindByTag(string tag)
        {
            this.metaObjectByTag.TryGetValue(tag, out var metaObject);

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
        public ICompositeBase FindDatabaseCompositeByName(string name)
        {
            this.Derive();

            this.derivedDatabaseCompositeByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var composite);

            return composite;
        }

        IValidationLog IMetaPopulation.Validate() => this.Validate();

        /// <summary>
        /// Validates this state.
        /// </summary>
        /// <returns>The Validate.</returns>
        public ValidationLog Validate()
        {
            var log = new ValidationLog();

            foreach (var domain in this.domains)
            {
                domain.Validate(log);
            }

            foreach (var unitType in this.units)
            {
                unitType.Validate(log);
            }

            foreach (var @interface in this.interfaces)
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

        public void Bind(Type[] types, Dictionary<Type, MethodInfo[]> extensionMethodsByInterface)
        {
            if (!this.IsBound)
            {
                this.Derive();

                this.IsBound = true;

                foreach (var domain in this.domains)
                {
                    domain.Bind();
                }

                var typeByName = types.ToDictionary(type => type.Name, type => type);

                foreach (var unit in this.units)
                {
                    unit.Bind();
                }

                this.Derive();
                foreach (var @interface in this.DatabaseInterfaces)
                {
                    @interface.Bind(typeByName);
                }

                foreach (var @class in this.DatabaseClasses)
                {
                    @class.Bind(typeByName);
                }

                this.MethodCompiler = new MethodCompiler(this, extensionMethodsByInterface);
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

        public void StructuralDerive()
        {
            this.isStructuralDeriving = true;

            try
            {
                this.domains = this.domains.ToArray();
                this.units = this.units.ToArray();
                this.interfaces = this.interfaces.ToArray();
                this.classes = this.classes.ToArray();
                this.inheritances = this.inheritances.ToArray();
                this.relationTypes = this.relationTypes.ToArray();
                this.associationTypes = this.associationTypes.ToArray();
                this.roleTypes = this.roleTypes.ToArray();
                this.methodTypes = this.methodTypes.ToArray();

                var sharedDomains = new HashSet<Domain>();
                var sharedComposites = new HashSet<ICompositeBase>();
                var sharedInterfaces = new HashSet<IInterfaceBase>();
                var sharedClasses = new HashSet<IClassBase>();
                var sharedAssociationTypes = new HashSet<IAssociationTypeBase>();
                var sharedRoleTypes = new HashSet<IRoleTypeBase>();
                var sharedMethodTypeList = new HashSet<IMethodTypeBase>();

                // Domains
                foreach (var domain in this.domains)
                {
                    domain.StructuralDeriveSuperdomains(sharedDomains);
                }

                // Unit & IComposite ObjectTypes
                var compositeTypes = new List<ICompositeBase>(this.interfaces);
                compositeTypes.AddRange(this.Classes);
                this.structuralDerivedComposites = compositeTypes.ToArray();

                // Database
                this.structuralDerivedDatabaseComposites = this.Composites.Where(v => v.Origin == Origin.Database).ToArray();
                this.structuralDerivedDatabaseInterfaces = this.interfaces.Where(v => v.Origin == Origin.Database).ToArray();
                this.structuralDerivedDatabaseClasses = this.classes.Where(v => v.Origin == Origin.Database).ToArray();
                this.structuralDerivedDatabaseRelationTypes = this.relationTypes.Where(v => v.Origin == Origin.Database).ToArray();

                // DirectSupertypes
                foreach (var type in this.Composites)
                {
                    type.StructuralDeriveDirectSupertypes(sharedInterfaces);
                }

                // DirectSubtypes
                foreach (var type in this.interfaces)
                {
                    type.StructuralDeriveDirectSubtypes(sharedComposites);
                }

                // Supertypes
                foreach (var type in this.Composites)
                {
                    type.StructuralDeriveSupertypes(sharedInterfaces);
                }

                // Subtypes
                foreach (var type in this.interfaces)
                {
                    type.StructuralDeriveSubtypes(sharedComposites);
                }

                // Subclasses
                foreach (var type in this.interfaces)
                {
                    type.StructuralDeriveSubclasses(sharedClasses);
                }

                // Exclusive Subclass
                foreach (var type in this.interfaces)
                {
                    type.StructuralDeriveExclusiveSubclass();
                }

                // RoleTypes & AssociationTypes
                var roleTypesByAssociationTypeObjectType = this.RelationTypes
                    .GroupBy(v => v.AssociationType.ObjectType)
                    .ToDictionary(g => g.Key, g => new HashSet<IRoleTypeBase>(g.Select(v => v.RoleType)));


                var associationTypesByRoleTypeObjectType = this.RelationTypes
                    .GroupBy(v => v.RoleType.ObjectType)
                    .ToDictionary(g => g.Key, g => new HashSet<IAssociationTypeBase>(g.Select(v => v.AssociationType)));

                // RoleTypes
                foreach (var composite in this.Composites)
                {
                    composite.StructuralDeriveRoleTypes(sharedRoleTypes, roleTypesByAssociationTypeObjectType);
                }

                // AssociationTypes
                foreach (var composite in this.Composites)
                {
                    composite.StructuralDeriveAssociationTypes(sharedAssociationTypes, associationTypesByRoleTypeObjectType);
                }

                // MethodTypes
                var methodTypeByClass = this.MethodTypes
                    .GroupBy(v => v.ObjectType)
                    .ToDictionary(g => g.Key, g => new HashSet<IMethodTypeBase>(g));

                foreach (var composite in this.Composites)
                {
                    composite.StructuralDeriveMethodTypes(sharedMethodTypeList, methodTypeByClass);
                }
            }
            finally
            {
                this.isStructuralDeriving = false;
            }
        }


        private void Derive()
        {
            if (this.isStructuralDeriving)
            {
                throw new Exception("Structural Derive is ongoing");
            }

            if (this.isStale && !this.isDeriving)
            {
                try
                {
                    this.isDeriving = true;

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

                    // Required RoleTypes
                    foreach (var @class in this.classes)
                    {
                        @class.DeriveRequiredRoleTypes();
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

                    foreach (var composite in this.Composites)
                    {
                        composite.DeriveIsRelationship();
                    }

                    // MetaPopulation
                    this.derivedDatabaseCompositeByLowercaseName = this.DatabaseComposites.ToDictionary(v => v.Name.ToLowerInvariant());
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
            this.metaObjectByTag.Add(domain.Tag, domain);

            this.Stale();
        }

        internal void OnUnitCreated(Unit unit)
        {
            this.units.Add(unit);
            this.metaObjectById.Add(unit.Id, unit);
            this.metaObjectByTag.Add(unit.Tag, unit);

            this.Stale();
        }

        public void OnInterfaceCreated(Interface @interface)
        {
            this.interfaces.Add(@interface);
            this.metaObjectById.Add(@interface.Id, @interface);
            this.metaObjectByTag.Add(@interface.Tag, @interface);

            this.Stale();
        }

        void IMetaPopulationBase.OnClassCreated(Class @class)
        {
            this.classes.Add(@class);
            this.metaObjectById.Add(@class.Id, @class);
            this.metaObjectByTag.Add(@class.Tag, @class);

            this.Stale();
        }

        void IMetaPopulationBase.OnInheritanceCreated(Inheritance inheritance)
        {
            this.inheritances.Add(inheritance);
            this.Stale();
        }

        void IMetaPopulationBase.OnRelationTypeCreated(RelationType relationType)
        {
            this.relationTypes.Add(relationType);
            this.metaObjectById.Add(relationType.Id, relationType);
            this.metaObjectByTag.Add(relationType.Tag, relationType);

            this.Stale();
        }

        void IMetaPopulationBase.OnAssociationTypeCreated(AssociationType associationType) => this.Stale();

        void IMetaPopulationBase.OnRoleTypeCreated(RoleType roleType) => this.Stale();

        void IMetaPopulationBase.OnMethodTypeCreated(MethodType methodType)
        {
            this.methodTypes.Add(methodType);
            this.metaObjectById.Add(methodType.Id, methodType);
            this.metaObjectByTag.Add(methodType.Tag, methodType);

            this.Stale();
        }

        void IMetaPopulationBase.Stale() => this.Stale();
        private void Stale() => this.isStale = true;

        private bool HasCycle(Composite subtype, HashSet<Interface> supertypes, Dictionary<Composite, List<Inheritance>> inheritancesBySubtype)
        {
            foreach (var inheritance in inheritancesBySubtype[subtype])
            {
                var supertype = inheritance.Supertype;
                if (supertype != null && this.HasCycle(subtype, supertype, supertypes, inheritancesBySubtype))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasCycle(Composite originalSubtype, Interface currentSupertype, HashSet<Interface> supertypes, Dictionary<Composite, List<Inheritance>> inheritancesBySubtype)
        {
            if (originalSubtype is Interface @interface && supertypes.Contains(@interface))
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
                        if (newSupertype != null && this.HasCycle(originalSubtype, newSupertype, supertypes, inheritancesBySubtype))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IMethodTypeBase MethodType(string id) => (IMethodTypeBase)this.FindById(new Guid(id));

        public IRoleTypeBase RoleType(string id) => ((RelationType)this.FindById(new Guid(id))).RoleType;
        IComposite IMetaPopulation.FindDatabaseCompositeByName(string name) => this.FindDatabaseCompositeByName(name);
    }
}
