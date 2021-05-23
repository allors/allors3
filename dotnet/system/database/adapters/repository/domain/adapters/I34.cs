// <copyright file="I34.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("ebc22540-54c8-4601-a43d-2ed6da9f3e79")]
    #endregion
    public partial interface I34 : Object
    {
        #region Allors
        [Id("37e8d764-bfeb-40d8-b7e9-d94e455dcc11")]
        [Precision(19)]
        [Scale(2)]
        #endregion
        decimal I34AllorsDecimal { get; set; }

        #region Allors
        [Id("4a6db64f-aeeb-4657-a24c-7997129f3efa")]
        #endregion
        bool I34AllorsBoolean { get; set; }

        #region Allors
        [Id("9b774204-37f3-4663-9162-dc801ea200f6")]
        #endregion
        double I34AllorsDouble { get; set; }

        #region Allors
        [Id("cd30dada-24c5-4b94-8f58-ab1018f087ea")]
        #endregion
        int I34AllorsInteger { get; set; }

        #region Allors
        [Id("d8125c69-1921-4e16-84bc-d3d174be7b83")]
        [Size(256)]
        #endregion
        string I34AllorsString { get; set; }
    }
}
