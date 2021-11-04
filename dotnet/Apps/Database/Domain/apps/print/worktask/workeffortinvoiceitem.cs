// <copyright file="WorkEffortInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

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

        public void AppsDelegateAccess(DelegatedAccessObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.Assignment?.SecurityTokens.ToArray();
            }

            if (method.Revocations == null)
            {
                method.Revocations = this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.Assignment?.Revocations.ToArray();
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
