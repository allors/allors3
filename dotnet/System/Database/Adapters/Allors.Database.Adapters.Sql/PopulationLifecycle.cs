// <copyright file="PopulationLifecycle.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;

    /// <summary>
    /// Tracks offline population replacement. All serving hosts and writers must already be stopped;
    /// this instance's ownership checks do not provide distributed exclusion.
    /// </summary>
    public sealed class PopulationLifecycle
    {
        private readonly IPopulationMetadataStore store;
        private readonly object sync = new object();
        private PopulationMaintenance active;

        public PopulationLifecycle(IPopulationMetadataStore store) =>
            this.store = store ?? throw new ArgumentNullException(nameof(store));

        public PopulationMetadata Read() => this.store.Read();

        // Administrative status only. A future coherent reader must obtain this metadata on its own snapshot.
        public PopulationMetadata RequireReady()
        {
            var metadata = this.Read();
            if (metadata == null)
            {
                throw new InvalidOperationException("Population metadata is missing. Stop all hosts and writers, then enroll the existing population before serving reads.");
            }

            if (metadata.State != PopulationState.Ready)
            {
                throw new InvalidOperationException("The population is in Maintenance. Complete the offline initialization/import workflow or explicitly validate and renew the recovered population before reopening service.");
            }

            return metadata;
        }

        public PopulationMetadata Enroll()
        {
            lock (this.sync)
            {
                this.EnsureNoOwner();
                var metadata = this.Read();
                if (metadata != null)
                {
                    if (metadata.State != PopulationState.Ready)
                    {
                        throw new InvalidOperationException("Enrollment cannot complete a population in Maintenance. Recover it offline and explicitly renew its incarnation after validating the recovered data.");
                    }

                    this.store.Validate();
                    return metadata;
                }

                return this.Renew();
            }
        }

        /// <summary>
        /// Explicit administrative attestation for an existing/restored population. Schema validation
        /// does not prove application derivation or retire authentication credentials.
        /// </summary>
        public PopulationMetadata Renew()
        {
            using var maintenance = this.BeginMaintenance();
            maintenance.Complete();
            return new PopulationMetadata(maintenance.Incarnation, PopulationState.Ready);
        }

        public PopulationMaintenance BeginMaintenance()
        {
            lock (this.sync)
            {
                this.EnsureNoOwner();
                var incarnation = Guid.NewGuid();
                this.store.BeginMaintenance(incarnation);
                return this.active = new PopulationMaintenance(this, incarnation);
            }
        }

        // Provider Init/Load phases join the outer workflow. A standalone phase can retire the
        // population but cannot make it ready: only an explicit administrative owner can do that.
        public PopulationOperation BeginOperation()
        {
            lock (this.sync)
            {
                var ownsMaintenance = this.active == null;
                var maintenance = this.active ?? this.BeginMaintenance();
                this.EnsureOpen(maintenance);
                ++maintenance.Operations;
                return new PopulationOperation(this, maintenance, ownsMaintenance);
            }
        }

        internal void Complete(PopulationMaintenance maintenance)
        {
            lock (this.sync)
            {
                this.EnsureOpen(maintenance);
                try
                {
                    if (maintenance.Failed || maintenance.Operations != 0)
                    {
                        throw new InvalidOperationException("Population maintenance has failed or still has unfinished operations. Keep service stopped and start a new recovery workflow; this incarnation cannot be marked Ready.");
                    }

                    this.store.Validate();
                    if (!this.store.CompleteMaintenance(maintenance.Incarnation))
                    {
                        throw new InvalidOperationException("The population incarnation changed during maintenance. Keep service stopped and restart the administrative workflow with its current owner.");
                    }

                    maintenance.Completed = true;
                }
                catch
                {
                    maintenance.Failed = true;
                    throw;
                }
            }
        }

        internal void Complete(PopulationOperation operation)
        {
            lock (this.sync)
            {
                this.EnsureOpen(operation.Maintenance);
                if (operation.Disposed || operation.Completed)
                {
                    throw new InvalidOperationException("This population operation has already ended. Start a new offline maintenance workflow.");
                }

                operation.Completed = true;
            }
        }

        internal void Dispose(PopulationOperation operation)
        {
            lock (this.sync)
            {
                if (operation.Disposed)
                {
                    return;
                }

                operation.Disposed = true;
                var maintenance = operation.Maintenance;
                maintenance.Failed |= !operation.Completed;
                --maintenance.Operations;
                if (operation.OwnsMaintenance)
                {
                    this.Dispose(maintenance);
                }

                // A disposed owner still fences its unfinished phases. Releasing it earlier
                // would let another workflow publish Ready while those phases can still write.
                if (maintenance.Disposed && maintenance.Operations == 0 && ReferenceEquals(this.active, maintenance))
                {
                    this.active = null;
                }
            }
        }

        internal void Dispose(PopulationMaintenance maintenance)
        {
            lock (this.sync)
            {
                if (maintenance.Disposed)
                {
                    return;
                }

                maintenance.Disposed = true;
                maintenance.Failed |= maintenance.Operations != 0;
                if (maintenance.Operations == 0 && ReferenceEquals(this.active, maintenance))
                {
                    this.active = null;
                }
            }
        }

        private void EnsureNoOwner()
        {
            if (this.active != null)
            {
                throw new InvalidOperationException("A population maintenance workflow is already active on this database instance. Finish or dispose it before starting another workflow.");
            }
        }

        private void EnsureOpen(PopulationMaintenance maintenance)
        {
            if (!ReferenceEquals(this.active, maintenance) || maintenance.Disposed || maintenance.Completed)
            {
                throw new InvalidOperationException("This population maintenance workflow has ended. Start a new offline workflow before performing more operations.");
            }
        }
    }
}
