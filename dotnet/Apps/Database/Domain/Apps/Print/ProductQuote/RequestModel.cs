// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Print.ProductQuoteModel
{
    using System.Collections.Generic;

    public class RequestModel
    {
        public RequestModel(Quote quote, Dictionary<string, byte[]> imageByImageName) => this.Number = quote.Request?.RequestNumber;

        public string Number { get; }
    }
}
