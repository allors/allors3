// <copyright file="MetaPopulationProps.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MetaPopulation type.</summary>

namespace Allors.Database.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed partial class MetaPopulationProps
    {
        private readonly IMetaPopulationBase metaPopulation;

        internal MetaPopulationProps(IMetaPopulationBase relationClass) => this.metaPopulation = relationClass;

        public IEnumerable<IDomain> Domains => this.metaPopulation.Domains;

        public IEnumerable<IUnit> Units => this.metaPopulation.Units;

        public IEnumerable<IComposite> Composites => this.metaPopulation.Composites;

        public IEnumerable<IInterface> Interfaces => this.metaPopulation.Interfaces;

        public IEnumerable<IClass> Classes => this.metaPopulation.Classes;

        public IEnumerable<IRelationType> RelationTypes => this.metaPopulation.RelationTypes;

        public IEnumerable<IMethodType> MethodTypes => this.metaPopulation.MethodTypes;

        public IEnumerable<IInheritance> Inheritances => this.metaPopulation.Inheritances;

        public IEnumerable<IComposite> DatabaseComposites => this.metaPopulation.DatabaseComposites;

        public IEnumerable<IInterface> DatabaseInterfaces => this.metaPopulation.DatabaseInterfaces;

        public IEnumerable<IClass> DatabaseClasses => this.metaPopulation.DatabaseClasses;

        public IEnumerable<IRelationType> DatabaseRelationTypes => this.metaPopulation.DatabaseRelationTypes;

        public IEnumerable<string> WorkspaceNames => this.metaPopulation.WorkspaceNames;

        public IReadOnlyDictionary<string, IEnumerable<IComposite>> WorkspaceCompositesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.Composites.Where(w => w.WorkspaceNames.Contains(v)));

        public IReadOnlyDictionary<string, IEnumerable<IInterface>> WorkspaceInterfacesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.Interfaces.Where(w => w.WorkspaceNames.Contains(v)));

        public IReadOnlyDictionary<string, IEnumerable<IClass>> WorkspaceClassesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.Classes.Where(w => w.WorkspaceNames.Contains(v)));

        public IReadOnlyDictionary<string, IEnumerable<IRelationType>> WorkspaceRelationTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.RelationTypes.Where(w => w.WorkspaceNames.Contains(v)));

        public IReadOnlyDictionary<string, IEnumerable<IMethodType>> WorkspaceMethodTypesByWorkspaceName =>
            this.WorkspaceNames
                .ToDictionary(v => v, v => this.MethodTypes.Where(w => w.WorkspaceNames.Contains(v)));

        public bool IsValid => this.metaPopulation.IsValid;
    }
}
