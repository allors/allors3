// <copyright file="PopulationOperation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    public sealed class PopulationOperation : IDisposable
    {
        private readonly PopulationLifecycle lifecycle;

        internal PopulationOperation(PopulationLifecycle lifecycle, PopulationMaintenance maintenance, bool ownsMaintenance)
        {
            this.lifecycle = lifecycle;
            this.Maintenance = maintenance;
            this.OwnsMaintenance = ownsMaintenance;
        }

        internal PopulationMaintenance Maintenance { get; }

        internal bool OwnsMaintenance { get; }

        internal bool Completed { get; set; }

        internal bool Disposed { get; set; }

        public void Complete() => this.lifecycle.Complete(this);

        public void Dispose() => this.lifecycle.Dispose(this);
    }
}
