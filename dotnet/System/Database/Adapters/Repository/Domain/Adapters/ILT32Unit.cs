// <copyright file="ILT32Unit.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("228fa79f-afa7-418c-968e-8c0d38fb3ad2")]
    #endregion
    public partial interface ILT32Unit : Object
    {
        #region Allors
        [Id("6822f677-7249-4c28-9b9c-18b21ba6f597")]
        [Size(256)]
        #endregion
        string AllorsString1 { get; set; }

        #region Allors
        [Id("b2734796-7140-4830-a0de-88df7d27b6a8")]
        [Size(256)]
        #endregion
        string AllorsString3 { get; set; }

        #region Allors
        [Id("ced16c48-6301-4652-8dcb-ed8a80ea7ce4")]
        [Size(256)]
        #endregion
        string AllorsString2 { get; set; }
    }
}
