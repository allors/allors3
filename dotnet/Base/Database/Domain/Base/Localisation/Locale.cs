// <copyright file="Locale.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Globalization;

    public partial class Locale
    {
        public bool ExistCultureInfo => this.ExistName;

        public CultureInfo CultureInfo => this.ExistName ? new CultureInfo(this.Name) : null;

        public void BaseOnInit(ObjectOnInit method)
        {
            if (!this.ExistName)
            {
                if (this.ExistLanguage)
                {
                    if (this.ExistCountry)
                    {
                        this.Name = this.Language.IsoCode.ToLowerInvariant() + "-" + this.Country.IsoCode.ToUpperInvariant();
                    }
                    else
                    {
                        this.Name = this.Language.IsoCode.ToLowerInvariant();
                    }
                }
            }
        }
    }
}
