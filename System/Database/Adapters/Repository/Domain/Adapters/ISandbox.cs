// <copyright file="ISandbox.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("7ba2ab26-491b-49eb-944c-26f6bb66e50f")]
    #endregion
    public partial interface ISandbox : Object
    {
        #region Allors
        [Id("38361bff-62b3-4607-8291-cfdaeedbd36d")]
        [Size(256)]
        #endregion
        string InvisibleValue { get; set; }

        #region Allors
        [Id("796ab057-88a0-4d71-bc4a-2673a209161b")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        ISandbox[] InvisibleManies { get; set; }

        #region Allors
        [Id("dba5deb2-880d-47f4-adae-0b3125ff1379")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        ISandbox InvisibleOne { get; set; }
    }
}
