// <copyright file="Derivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Allors.Database.Domain;
    using Configuration.Derivations.Default;
    using Database.Derivations;
    using Domain.Derivations.Legacy;
    using Object = Domain.Object;

    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Allors Object")]
    public class LegacyDerivation : ILegacyDerivation
    {
        private bool guard;

        private LegacyProperties legacyProperties;

        public LegacyDerivation(ITransaction transaction, Engine engine, int maxCycles, bool continueOnError)
        {
            this.LegacyDerivationConfig = new LegacyDerivationConfig();
            this.Transaction = transaction;
            this.Engine = engine;
            this.MaxCycles = maxCycles;
            this.ContinueOnError = continueOnError;

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

        public bool ContinueOnError { get; }

        public LegacyDerivationConfig LegacyDerivationConfig { get; }

        public Guid Id { get; }

        public DateTime TimeStamp { get; private set; }

        public IValidation Validation { get; private set; }

        public ISet<Object> DerivedObjects { get; }

        ILegacyCycle ILegacyDerivation.LegacyCycle => this.LegacyCycle;

        IAccumulatedChangeSet IDerivation.ChangeSet => this.ChangeSet;

        public object this[string name]
        {
            get => this.legacyProperties?.Get(name);

            set
            {
                this.legacyProperties ??= new LegacyProperties();
                this.legacyProperties.Set(name, value);
            }
        }

        internal LegacyCycle LegacyCycle { get; set; }

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

                this.LegacyCycle = new LegacyCycle(this);
                var derivedObjects = this.LegacyCycle.Execute(GetAndResetMarked());

                while (derivedObjects.Any() || this.MarkedBacklog.Any())
                {
                    if (this.LegacyDerivationConfig.MaxCycles != 0 && count++ > this.LegacyDerivationConfig.MaxCycles)
                    {
                        throw new Exception("Maximum amount of cycles reached");
                    }

                    this.LegacyCycle = new LegacyCycle(this);
                    derivedObjects = this.LegacyCycle.Execute(GetAndResetMarked());
                }

                return this.Validation;
            }
            finally
            {
                this.LegacyCycle = null;
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
