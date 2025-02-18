// <copyright file="Parts.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Parts
    {
        public static void Daily(ITransaction transaction)
        {
            foreach (Part part in new Parts(transaction).Extent())
            {
                part.DeriveRelationships();
            }
        }
    }
}
