// <copyright file="PaymentExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static partial class PaymentExtensions
    {
        public static void AppsDelete(this Payment @this, DeletableDelete method)
        {
            foreach (PaymentApplication paymentApplication in @this.PaymentApplications)
            {
                paymentApplication.Delete();
            }
        }
    }
}
