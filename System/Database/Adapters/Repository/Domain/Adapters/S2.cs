// <copyright file="S2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("feeb7027-7c6c-4cb5-8718-93e6e8a4afd8")]
    #endregion
    public partial interface S2 : Object
    {
        #region Allors
        [Id("1c758737-140a-49f0-badc-29658b4bc55f")]
        [Size(256)]
        #endregion
        string S2AllorsString { get; set; }

        #region Allors
        [Id("1f5a6afe-f458-43db-bea0-8c90074b5abf")]
        #endregion
        int S2AllorsInteger { get; set; }

        #region Allors
        [Id("74dd2b7b-e647-4967-9838-46c701baf3a7")]
        #endregion
        double S2AllorsDouble { get; set; }

        #region Allors
        [Id("9a191c76-bd05-498f-91da-33184c72fe90")]
        #endregion
        bool S2AllorsBoolean { get; set; }

        #region Allors
        [Id("9d70a5f5-ed72-4ba3-98ac-e50752f8fb79")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal S2AllorsDecimal { get; set; }

        #region Allors
        [Id("a305d91a-5fe1-467d-9f24-6cce5dd30b1d")]
        #endregion
        DateTime S2AllorsDateTime { get; set; }
    }
}
