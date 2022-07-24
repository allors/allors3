// <copyright file="Generation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Domain.Derivations.Legacy;
    using Object = Domain.Object;

    public class LegacyCycle : ILegacyCycle
    {
        private LegacyProperties legacyProperties;

        internal LegacyCycle(LegacyDerivation derivation)
        {
            this.Derivation = derivation;
            this.ChangeSet = new AccumulatedChangeSet();
        }

        IAccumulatedChangeSet ILegacyCycle.ChangeSet => this.ChangeSet;

        ILegacyIteration ILegacyCycle.LegacyIteration => this.LegacyIteration;

        ILegacyDerivation ILegacyCycle.Derivation => this.Derivation;

        internal AccumulatedChangeSet ChangeSet { get; }

        internal LegacyIteration LegacyIteration { get; set; }

        internal LegacyDerivation Derivation { get; }

        public object this[string name]
        {
            get => this.legacyProperties?.Get(name);

            set
            {
                this.legacyProperties ??= new LegacyProperties();
                this.legacyProperties.Set(name, value);
            }
        }

        internal List<Object> Execute(Object[] marked = null)
        {
            try
            {
                var config = this.Derivation.LegacyDerivationConfig;
                var count = 1;

                var postDeriveBacklog = new List<Object>();
                var previousCount = postDeriveBacklog.Count;

                this.LegacyIteration = new LegacyIteration(this);
                this.LegacyIteration.Execute(postDeriveBacklog, marked);

                while (postDeriveBacklog.Count != previousCount)
                {
                    if (config.MaxIterations != 0 && count++ > config.MaxIterations)
                    {
                        throw new Exception("Maximum amount of iterations reached");
                    }

                    previousCount = postDeriveBacklog.Count;

                    this.LegacyIteration = new LegacyIteration(this);
                    this.LegacyIteration.Execute(postDeriveBacklog);
                }

                var postDerived = new HashSet<Object>();
                for (var i = postDeriveBacklog.Count - 1; i >= 0; i--)
                {
                    var @object = postDeriveBacklog[i];
                    if (!postDerived.Contains(@object) && !@object.Strategy.IsDeleted)
                    {
                        @object.OnPostDerive(x => x.WithDerivation(this.Derivation));
                    }

                    postDerived.Add(@object);
                }

                return postDeriveBacklog;
            }
            finally
            {
                this.LegacyIteration = null;
            }
        }
    }
}
