// <copyright file="Sandbox.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("73970b0f-1ff4-4d39-aad8-fdbfbaae472f")]
    #endregion
    public partial class Sandbox : System.Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("0e0ee030-8fb5-42fb-82b5-5daade2aca9d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        public Sandbox[] InvisibleManies { get; set; }

        #region Allors
        [Id("122b0376-8d1a-4d46-b8a0-9f4ea94c9e96")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        public Sandbox InvisibleOne { get; set; }

        #region Allors
        [Id("5eec5096-d8ba-424e-988f-b50828fc7b51")]
        [Size(256)]
        #endregion
        public string InvisibleValue { get; set; }

        #region Allors
        [Id("856a0161-2a46-428a-bae5-95d6a86a89e8")]
        [Size(256)]
        #endregion
        public string Test { get; set; }

        #region Allors
        [Id("a0dac9fc-2d19-429b-a522-46425a01ab78")]
        #endregion
        public int AllorsInteger { get; set; }

        #region Allors
        [Id("c82d1693-7b88-4fab-8389-a43185c832ed")]
        [Size(256)]
        #endregion
        public string AllorsString { get; set; }

        #region Allors

        [Id("E551BDCA-9532-4024-B127-E971A5C1CDB2")]

        #endregion
        public void DoIt()
        {
        }

        #region inherited methods

        #endregion
    }
}
