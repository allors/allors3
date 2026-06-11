// <copyright file="PurchaseInvoiceStates.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System.Web;

    public partial class Media
    {
        public string Source
        {
            get
            {
                var fileNamePart = !string.IsNullOrWhiteSpace(this.FileName.Value) ? $"/{HttpUtility.UrlEncode(this.FileName.Value)}" : null;
                return $"/media/{this.UniqueId.Value}{fileNamePart}";
            }
        }
    }
}
