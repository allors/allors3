// <copyright file="WorkEffortInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkEffortInvoiceItem
    {
        public bool IsDeletable
        {
            get
            {
                var workEffortState = this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem?.Assignment?.WorkEffortState;

                return !workEffortState.IsCompleted && !workEffortState.IsFinished;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (IsDeletable)
            {
                this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.Delete();
            }
        }
    }
}
