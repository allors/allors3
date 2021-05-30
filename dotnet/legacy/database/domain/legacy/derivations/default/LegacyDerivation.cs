// <copyright file="Derivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations.Legacy.Default
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Database.Derivations;
    using Rules.Default;
    using Object = Object;

    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Allors Object")]
    public class LegacyDerivation : ILegacyDerivation
    {
        private bool guard;

        private Properties properties;

        public LegacyDerivation(ITransaction transaction, Engine engine, int maxCycles)
        {
            this.DerivationConfig = new DerivationConfig();
            this.Transaction = transaction;
            this.Engine = engine;
            this.MaxCycles = maxCycles;

            this.Id = Guid.NewGuid();
            this.TimeStamp = this.Transaction.Now();

            this.ChangeSet = new AccumulatedChangeSet();
            this.DerivedObjects = new HashSet<Object>();
            this.Validation = new Validation();

            this.MarkedBacklog = new HashSet<Object>();

            this.guard = false;
        }

        public ITransaction Transaction { get; }

        public Engine Engine { get; }

        public int MaxCycles { get; }

        public DerivationConfig DerivationConfig { get; }

        public Guid Id { get; }

        public DateTime TimeStamp { get; private set; }

        public IValidation Validation { get; private set; }

        public ISet<Object> DerivedObjects { get; }

        ICycle ILegacyDerivation.Cycle => this.Cycle;

        IAccumulatedChangeSet IDerivation.ChangeSet => this.ChangeSet;

        public object this[string name]
        {
            get => this.properties?.Get(name);

            set
            {
                this.properties ??= new Properties();
                this.properties.Set(name, value);
            }
        }

        internal Cycle Cycle { get; set; }

        internal AccumulatedChangeSet ChangeSet { get; }

        internal ISet<Object> MarkedBacklog { get; private set; }

        public void Mark(Object @object) => this.MarkedBacklog.Add(@object);

        public void Mark(params Object[] objects) => this.MarkedBacklog.UnionWith(objects);

        public IValidation Derive()
        {
            Object[] GetAndResetMarked()
            {
                var marked = this.MarkedBacklog.Where(v => v != null).ToArray();
                this.MarkedBacklog = new HashSet<Object>();
                return marked;
            }

            try
            {
                this.Guard();

                var count = 1;

                this.Cycle = new Cycle(this);
                var derivedObjects = this.Cycle.Execute(GetAndResetMarked());

                while (derivedObjects.Any() || this.MarkedBacklog.Any())
                {
                    if (this.DerivationConfig.MaxCycles != 0 && count++ > this.DerivationConfig.MaxCycles)
                    {
                        throw new Exception("Maximum amount of cycles reached");
                    }

                    this.Cycle = new Cycle(this);
                    derivedObjects = this.Cycle.Execute(GetAndResetMarked());
                }

                return this.Validation;
            }
            finally
            {
                this.Cycle = null;
            }
        }

        private void Guard()
        {
            if (this.guard)
            {
                throw new Exception("Derive can only be called once. Create a new Derivation object.");
            }

            this.guard = true;
        }
    }
}
