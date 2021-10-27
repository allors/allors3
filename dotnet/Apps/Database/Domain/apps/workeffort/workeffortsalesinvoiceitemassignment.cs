// <copyright file="WorkEffortSalesInvoiceItemAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class WorkEffortSalesInvoiceItemAssignment
    {
        public void AppsDelegateAccess(DelegatedAccessObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.Assignment?.SecurityTokens.ToArray();
            }

            if (method.Revocations == null)
            {
                method.Revocations = this.Assignment?.Revocations.ToArray();
            }
        }
    }
}
