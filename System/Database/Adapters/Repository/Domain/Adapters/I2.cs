// <copyright file="I2.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;

    #region Allors
    [Id("19bb2bc3-d53a-4d15-86d0-b250fdbcb0a0")]
    #endregion
    public partial interface I2 : Object, S1234, S2
    {
        #region Allors
        [Id("35040d7c-ab7f-4a99-9d09-e01e24ca3cb9")]
        #endregion
        bool I2AllorsBoolean { get; set; }

        #region Allors
        [Id("4f095abd-8803-4610-87f0-2847ddd5e9f4")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I2AllorsDecimal { get; set; }

        #region Allors
        [Id("81d9eb2f-55a7-4d1c-853d-4369eb691ba5")]
        #endregion
        DateTime I2AllorsDateTime { get; set; }

        #region Allors
        [Id("9f91841c-f63f-4ffa-bee6-62e100f3cd15")]
        [Size(256)]
        #endregion
        string I2AllorsString { get; set; }

        #region Allors
        [Id("d30dd036-6d28-48df-873b-3a76da8c029e")]
        #endregion
        int I2AllorsInteger { get; set; }

        #region Allors
        [Id("fbad33e7-ede1-41fc-97e9-ddf33a0f6459")]
        #endregion
        double I2AllorsDouble { get; set; }
    }
}
