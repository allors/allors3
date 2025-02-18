// <copyright file="WebAddress.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WebAddress
    {
        public bool IsPostalAddress => false;

        public override string ToString() => this.ElectronicAddressString;
    }
}
