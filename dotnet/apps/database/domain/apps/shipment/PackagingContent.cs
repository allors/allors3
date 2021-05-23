// <copyright file="PackagingContent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PackagingContent
    {
        public string ContentDescription
        {
            get
            {
                if (this.ShipmentItem.ExistGood)
                {
                    return this.ShipmentItem.Good.Name;
                }

                if (this.ShipmentItem.ExistPart)
                {
                    return this.ShipmentItem.Part.Name;
                }

                return this.ShipmentItem.ContentsDescription;
            }
        }
    }
}
