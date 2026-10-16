// <copyright file="PopulationMaintenance.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    public sealed class PopulationMaintenance : IDisposable
    {
        private readonly PopulationLifecycle lifecycle;

        internal PopulationMaintenance(PopulationLifecycle lifecycle, Guid incarnation)
        {
            this.lifecycle = lifecycle;
            this.Incarnation = incarnation;
        }

        public Guid Incarnation { get; }

        internal int Operations { get; set; }

        internal bool Failed { get; set; }

        internal bool Completed { get; set; }

        internal bool Disposed { get; set; }

        public void Complete() => this.lifecycle.Complete(this);

        public void Dispose() => this.lifecycle.Dispose(this);
    }
}
