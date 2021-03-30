// <copyright file="I12.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("17239F78-C639-42E1-ACC5-63ED5A74BF7D")]
    #endregion
    public partial interface S12 : Object
    {
        #region Allors
        [Id("31038235-5D34-4D45-9044-CCBF03CAB556")]
        #endregion
        public bool ChangedRolePingS12 { get; set; }

        #region Allors
        [Id("E0BE3514-3AAA-4330-9649-A6E192205C2C")]
        #endregion
        [Derived]
        public bool ChangedRolePongS12 { get; set; }

        #region Allors
        [Id("F114AC21-CF5D-48AC-9E72-14970FD09B27")]
        #endregion
        public bool ChangedRolePingI12 { get; set; }

        #region Allors
        [Id("38AE7AC0-3362-44B0-8AD0-89DF9FC765F3")]
        #endregion
        [Derived]
        public bool ChangedRolePongI12 { get; set; }

        #region Allors
        [Id("DF58D9F5-7A4B-435C-B753-1D1EEA85C36E")]
        #endregion
        public bool ChangedRolePingI1 { get; set; }

        #region Allors
        [Id("4CF6B30D-A264-4B93-98DC-ADDA1278DD97")]
        #endregion
        [Derived]
        public bool ChangedRolePongI1 { get; set; }

        #region Allors
        [Id("B9F8DF37-4C52-49D3-BE5A-1E1CC96C89A9")]
        #endregion
        public bool ChangedRolePingC1 { get; set; }

        #region Allors
        [Id("02945F51-B3D6-4945-B467-DA10AE1986B6")]
        #endregion
        [Derived]
        public bool ChangedRolePongC1 { get; set; }
    }
}
