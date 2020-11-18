// <copyright file="MethodInterface.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Meta
{
    using System;
    using System.Linq;

    public sealed partial class MethodInterface : MethodType, IMethodInterface
    {
        private string[] assignedWorkspaceNames;
        private string[] derivedWorkspaceNames;

        private string name;

        public MethodInterface(Interface @interface, Guid id) : base(@interface.MetaPopulation)
        {
            this.Interface = @interface;
            this.Id = id;
            this.IdAsString = this.Id.ToString("D");

            this.MetaPopulation.OnMethodInterfaceCreated(this);
        }

        public override Guid Id { get; }

        public override string IdAsString { get; }


        public Interface Interface { get; }
        public override Composite Composite => this.Interface;

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

        public override string[] WorkspaceNames
        {
            get
            {
                this.MetaPopulation.Derive();
                return this.derivedWorkspaceNames;
            }
        }


        public override string Name
        {
            get => this.name;

            set
            {
                this.MetaPopulation.AssertUnlocked();
                this.name = value;
                this.MetaPopulation.Stale();
            }
        }

        public override string FullName => $"{this.Composite.Name}{this.Name}";

        protected internal override void DeriveWorkspaceNames()
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
