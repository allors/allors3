// <copyright file="Class.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class Class : Composite, IClass
    {
        private readonly Class[] classes;

        private readonly Dictionary<RoleType, RoleClass> derivedRoleClassByRoleType;

        private readonly Dictionary<MethodType, ConcreteMethodType> concreteMethodTypeByMethodType;

        private RoleClass[] derivedRoleClasses;
        private RoleClass[] derivedDatabaseRoleClasses;

        private ConcreteMethodType[] concreteMethodTypes;

        private Type clrType;

        internal Class(MetaPopulation metaPopulation, Guid id)
                    : base(metaPopulation)
        {
            this.Id = id;

            this.derivedRoleClassByRoleType = new Dictionary<RoleType, RoleClass>();
            this.concreteMethodTypeByMethodType = new Dictionary<MethodType, ConcreteMethodType>();

            this.classes = new[] { this };
            metaPopulation.OnClassCreated(this);
        }

        public bool Workspace => this.WorkspaceNames != null;

        public string[] WorkspaceNames { get; set; }
        
        // TODO: Review
        public RoleType[] DelegatedAccessRoleTypes { get; set; }

        public Dictionary<RoleType, RoleClass> RoleClassByRoleType
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedRoleClassByRoleType;
            }
        }

        public Dictionary<MethodType, ConcreteMethodType> ConcreteMethodTypeByMethodType
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.concreteMethodTypeByMethodType;
            }
        }

        public RoleClass[] RoleClasses
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedRoleClasses;
            }
        }

        public RoleClass[] DatabaseRoleClasses
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedDatabaseRoleClasses;
            }
        }

        public ConcreteMethodType[] ConcreteMethodTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.concreteMethodTypes;
            }
        }

        public override IEnumerable<Class> Classes => this.classes;

        public override IEnumerable<IClass> DatabaseClasses => this.Origin == Origin.Remote ? this.classes : Array.Empty<Class>();

        public override bool ExistClass => true;

        public override Class ExclusiveClass => this;

        public override Type ClrType => this.clrType;

        public IEnumerable<RoleClass> WorkspaceConcreteRoleTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.RoleClasses.Where(m => m.RoleType.Workspace);
            }
        }

        public IEnumerable<ConcreteMethodType> WorkspaceConcreteMethodTypes
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.ConcreteMethodTypes.Where(m => m.MethodType.Workspace);
            }
        }

        public override IEnumerable<Composite> Subtypes => new[] { this };

        public override IEnumerable<Composite> DatabaseSubtypes => this.Origin == Origin.Remote ? this.Subtypes : Array.Empty<Composite>();

        public override bool IsAssignableFrom(IComposite objectType) => this.Equals(objectType);

        public void DeriveRoleClasses(HashSet<RoleType> sharedRoleTypes)
        {
            sharedRoleTypes.Clear();
            var removedRoleTypes = sharedRoleTypes;
            removedRoleTypes.UnionWith(this.RoleClassByRoleType.Keys);

            foreach (var roleType in this.RoleTypes)
            {
                removedRoleTypes.Remove(roleType);

                if (!this.derivedRoleClassByRoleType.TryGetValue(roleType, out var concreteRoleType))
                {
                    concreteRoleType = new RoleClass(this, roleType);
                    this.derivedRoleClassByRoleType[roleType] = concreteRoleType;
                }
            }

            foreach (var roleType in removedRoleTypes)
            {
                this.derivedRoleClassByRoleType.Remove(roleType);
            }

            this.derivedRoleClasses = this.derivedRoleClassByRoleType.Values.ToArray();
            this.derivedDatabaseRoleClasses = this.derivedRoleClasses.Where(v=>v.RelationType.Origin == Origin.Remote).ToArray();
        }

        public void DeriveConcreteMethodTypes(HashSet<MethodType> sharedMethodTypes)
        {
            sharedMethodTypes.Clear();
            var removedMethodTypes = sharedMethodTypes;
            removedMethodTypes.UnionWith(this.concreteMethodTypeByMethodType.Keys);

            foreach (var methodType in this.MethodTypes)
            {
                removedMethodTypes.Remove(methodType);

                if (!this.concreteMethodTypeByMethodType.TryGetValue(methodType, out var concreteMethodType))
                {
                    concreteMethodType = new ConcreteMethodType(this, methodType);
                    this.concreteMethodTypeByMethodType[methodType] = concreteMethodType;
                }
            }

            foreach (var methodType in removedMethodTypes)
            {
                this.concreteMethodTypeByMethodType.Remove(methodType);
            }

            this.concreteMethodTypes = this.concreteMethodTypeByMethodType.Values.ToArray();
        }

        internal void Bind(Dictionary<string, Type> typeByTypeName) => this.clrType = typeByTypeName[this.Name];
    }
}
