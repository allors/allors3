// <copyright file="I12.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("17239F78-C639-42E1-ACC5-63ED5A74BF7D")]
    #endregion
    public partial interface S12 : Object
    {
        #region Allors
        [Id("31038235-5D34-4D45-9044-CCBF03CAB556")]
        #endregion
        public bool ChangedRolePing { get; set; }

        #region Allors
        [Id("E0BE3514-3AAA-4330-9649-A6E192205C2C")]
        #endregion
        [Derived]
        public bool ChangedRolePong { get; set; }
    }
}
