// <copyright file="Named.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("fcaa52e3-4a90-4981-b45d-d158e2589506")]
    #endregion
    public partial interface Named : Object
    {
        #region Allors
        [Id("ce43ca5e-4dfb-4fe1-98ea-17d8382e9531")]
        [Size(256)]
        #endregion
        string Name { get; set; }

        #region Allors
        [Id("fdad723a-f062-492a-989c-8d8727c52679")]
        #endregion
        int Index { get; set; }

        #region Allors

        [Id("BFDDD727-6793-41A4-873D-BF80535D7DE2")]

        #endregion

        void InheritedDoIt();
    }
}
