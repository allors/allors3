// <copyright file="ExternalMediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class ExternalMediaContent
    {
        // The object id is the storage key. Bytes live in external storage, never in the database.
        public byte[] Data
        {
            get => this.Storage.Read(this.Id);
            set
            {
                // Write-once: a MediaContent's file is written when the content is first built. Allors always
                // creates a fresh content for new data (see MediaRule), so a legitimate write targets an object
                // that is new in this transaction — its file carries no committed state, so a rollback that
                // discards it loses nothing. Overwriting the file of an already-persisted content is not
                // rollback-safe (the rollback restores the object but cannot restore the file), so reject it.
                // (Exists() is unreliable here: a freshly allocated id can collide with a stale orphan file.)
                if (!this.Strategy.IsNewInTransaction)
                {
                    throw new InvalidOperationException(
                        $"ExternalMediaContent {this.Id} is already persisted and is write-once: overwriting its " +
                        "file is not rollback-safe. To change media data set Media.InData (a fresh MediaContent is built); " +
                        "the old file is reclaimed at the next Load/Upgrade.");
                }

                this.Storage.Write(this.Id, value);
            }
        }

        // True when the backing file exists and is non-empty; checked via a cheap length probe (no read).
        public bool HasData => this.Storage.Length(this.Id) > 0;

        private IMediaContentStorage Storage => this.Strategy.Transaction.Database.Services.Get<IMediaContentStorage>();

        public void CoreOnPostDerive(ObjectOnPostDerive method)
        {
            if (!this.HasData)
            {
                method.Derivation.Validation.AddError($"ExternalMediaContent {this.Id} has no stored data.");
            }
        }

        public void CoreDelete(DeletableDelete method)
        {
            // Deletion is deferred: unlinking the file here is not rollback-safe — Strategy.Delete() is
            // reverted on Rollback, but a deleted file cannot be restored. The file is left as an orphan and
            // reclaimed at the next Load/Upgrade (ExternalMediaContents.ReconcileFiles), guaranteeing no data loss.
        }
    }
}
