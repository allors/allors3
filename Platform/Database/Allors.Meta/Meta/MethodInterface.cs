// <copyright file="MethodInterface.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Meta
{
    using System;

    public sealed partial class MethodInterface : MethodType, IMethodInterface
    {
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
    }
}
