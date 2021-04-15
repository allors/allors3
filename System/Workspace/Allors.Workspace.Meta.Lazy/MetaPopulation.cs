// <copyright file="MetaPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public abstract partial class MetaPopulation : IMetaPopulation
    {
        private Unit[] units;
        private Interface[] interfaces;
        private Class[] classes;
        private Inheritance[] inheritances;
        private RelationType[] relationTypes;
        private MethodType[] methodTypes;

        private Dictionary<Guid, MetaObjectBase> metaObjectById;

        private Composite[] composites;
        private Dictionary<string, Composite> compositeByLowercaseName;

        public void Derive(Unit[] builderUnits, Interface[] builderInterfaces, Class[] builderClasses, Inheritance[] builderInheritances, RelationType[] builderRelationTypes, MethodType[] builderMethodTypes)
        {
            this.units = builderUnits;
            this.interfaces = builderInterfaces;
            this.classes = builderClasses;
            this.inheritances = builderInheritances;
            this.relationTypes = builderRelationTypes;
            this.methodTypes = builderMethodTypes;

            this.metaObjectById =
                this.units.Cast<MetaObjectBase>()
                .Union(this.classes)
                .Union(this.relationTypes)
                .Union(this.methodTypes)
                .ToDictionary(v => ((IMetaIdentifiableObject)v).Id, v => v);

            this.composites = this.interfaces.Cast<Composite>().Union(this.classes).ToArray();
            this.compositeByLowercaseName = this.Composites.ToDictionary(v => v.Name.ToLowerInvariant());

            // DirectSupertypes
            foreach (var grouping in this.inheritances.GroupBy(v => v.Subtype, v => v.Supertype))
            {
                var composite = grouping.Key;
                composite.directSupertypes = new HashSet<Interface>(grouping);
            }

            // DirectSubtypes
            foreach (var grouping in this.inheritances.GroupBy(v => v.Supertype, v => v.Subtype))
            {
                var @interface = grouping.Key;
                @interface.directSubtypes = new HashSet<Composite>(grouping);
            }

            // Supertypes
            foreach (var composite in this.composites)
            {
                static IEnumerable<Interface> RecurseDirectSupertypes(Composite composite)
                {
                    if (composite.directSupertypes != null)
                    {
                        foreach (var directSupertype in composite.directSupertypes)
                        {
                            yield return directSupertype;

                            foreach (var directSuperSupertype in RecurseDirectSupertypes(directSupertype))
                            {
                                yield return directSuperSupertype;
                            }
                        }
                    }
                }

                composite.supertypes = new HashSet<Interface>(RecurseDirectSupertypes(composite));
            }

            // Subtypes
            foreach (var @interface in this.interfaces)
            {
                static IEnumerable<Composite> RecurseDirectSubtypes(Interface @interface)
                {
                    if (@interface.directSubtypes != null)
                    {
                        foreach (var directSubtype in @interface.directSubtypes)
                        {
                            yield return directSubtype;

                            if (directSubtype is Interface directSubinterface)
                            {
                                foreach (var directSubSubtype in RecurseDirectSubtypes(directSubinterface))
                                {
                                    yield return directSubSubtype;
                                }
                            }
                        }
                    }
                }

                @interface.subtypes = new HashSet<Composite>(RecurseDirectSubtypes(@interface));
                @interface.classes = new HashSet<Class>(@interface.subtypes.Where(v => v.IsClass).Cast<Class>());
            }

            // TODO:

            // RoleTypes & AssociationTypes
            var roleTypesByAssociationTypeObjectType = this.relationTypes
                .GroupBy(v => v.AssociationType.ObjectType)
                .ToDictionary(g => g.Key, g => new HashSet<RoleType>(g.Select(v => v.RoleType)));


            var associationTypesByRoleTypeObjectType = this.relationTypes
                .GroupBy(v => v.RoleType.ObjectType)
                .ToDictionary(g => g.Key, g => new HashSet<AssociationType>(g.Select(v => v.AssociationType)));

            // RoleTypes
            var sharedRoleTypes = new HashSet<RoleType>();
            foreach (var composite in this.composites)
            {
                composite.DeriveRoleTypes(sharedRoleTypes, roleTypesByAssociationTypeObjectType);
            }

            // AssociationTypes
            var sharedAssociationTypes = new HashSet<AssociationType>();
            foreach (var composite in this.composites)
            {
                composite.DeriveAssociationTypes(sharedAssociationTypes, associationTypesByRoleTypeObjectType);
            }

            // RoleType
            foreach (var relationType in this.relationTypes)
            {
                relationType.RoleType.DeriveScaleAndSize();
            }

            // RelationType Multiplicity
            foreach (var relationType in this.relationTypes)
            {
                relationType.DeriveMultiplicity();
            }

            var sharedMethodTypeList = new HashSet<MethodType>();

            // MethodTypes
            var methodTypeByClass = this.methodTypes
                .GroupBy(v => v.Composite)
                .ToDictionary(g => g.Key, g => new HashSet<MethodType>(g));

            foreach (var type in this.composites)
            {
                type.DeriveMethodTypes(sharedMethodTypeList, methodTypeByClass);
            }
        }

        IEnumerable<IUnit> IMetaPopulation.Units => this.units;
        IEnumerable<IInterface> IMetaPopulation.Interfaces => this.interfaces;
        IEnumerable<IClass> IMetaPopulation.Classes => this.classes;
        IEnumerable<IRelationType> IMetaPopulation.RelationTypes => this.relationTypes;
        IEnumerable<IMethodType> IMetaPopulation.MethodTypes => this.methodTypes;

        IEnumerable<IComposite> IMetaPopulation.Composites => this.Composites;
        public IEnumerable<Composite> Composites => this.composites;

        IMetaObject IMetaPopulation.Find(Guid id) => this.Find(id);
        public MetaObjectBase Find(Guid id)
        {
            this.metaObjectById.TryGetValue(id, out var metaObject);

            return metaObject;
        }

        IComposite IMetaPopulation.FindByName(string name) => this.FindByName(name);
        public IComposite FindByName(string name)
        {
            this.compositeByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var composite);
            return composite;
        }

        void IMetaPopulation.Bind(Type[] types) => this.Bind(types);
        public void Bind(Type[] types)
        {

            var typeByName = types.ToDictionary(type => type.Name, type => type);

            foreach (var unit in this.units)
            {
                unit.Bind();
            }

            foreach (var @interface in this.interfaces)
            {
                @interface.Bind(typeByName);
            }

            foreach (var @class in this.classes)
            {
                @class.Bind(typeByName);
            }
        }
    }
}
