// <copyright file="ExternalMediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ExternalMediaContent
    {
        // The object id is the storage key. Bytes live in external storage, never in the database.
        public byte[] Data
        {
            get => this.Storage.Read(this.Id);
            set => this.Storage.Write(this.Id, value);
        }

        private IMediaContentStorage Storage => this.Strategy.Transaction.Database.Services.Get<IMediaContentStorage>();

        public void CoreOnPostDerive(ObjectOnPostDerive method)
        {
            var data = this.Storage.Read(this.Id);

            if (data == null || data.Length == 0)
            {
                method.Derivation.Validation.AddError(this, this.Meta.Type, "Empty data");
            }
        }

        public void CoreDelete(DeletableDelete method)
        {
            // Deletion is deferred: unlinking the file here is not rollback-safe — Strategy.Delete() is
            // reverted on Rollback, but a deleted file cannot be restored. The file is left as an orphan and
            // reclaimed by the PruneMediaFiles command (ExternalMediaContents.RemoveOrphanedFiles), guaranteeing no data loss.
        }
    }
}
