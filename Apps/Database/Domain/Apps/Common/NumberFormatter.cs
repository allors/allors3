// <copyright file="NumberFormatter.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public static class NumberFormatter
    {
        // TODO: should be a service (NumberFormatter)
        public static int SortableNumber(string prefix, string identification, string year)
        {
            if (prefix != null)
            {
                if (prefix.Contains("{year}"))
                {
                    // this.Store.SalesInvoiceNumberPrefix.Length - 2 because of {} in this string
                    return int.Parse(string.Concat(year, identification.Substring(prefix.Length - 2)));
                }
                else
                {
                    return int.Parse(identification.Substring(prefix.Length));
                }
            }
            else
            {
                return int.Parse(identification);
            }
        }
    }
}
