// <copyright file="FileMediaContent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class FileMediaContent
    {
        // The object id is used as the filename. Bytes live on the filesystem, never in the database.
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

        public void CoreDelete(DeletableDelete method) => this.Storage.Delete(this.Id);
    }
}
