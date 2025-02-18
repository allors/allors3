// <copyright file="DeletableDelete.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class DeletableDelete
    {
        public override void Execute()
        {
            var deleting = this.Object.Strategy.Transaction.Services.Get<IDeleting>();

            var id = this.Object.Id;

            if (deleting.IsDeleting(id))
            {
                throw new Exception("Object with id " + id + " is already deleting");
            }

            deleting.OnBeginDelete(id);

            try
            {
                base.Execute();

                this.Object.Strategy.Delete();
            }
            finally
            {
                deleting.OnEndDelete(id);
            }
        }
    }
}
