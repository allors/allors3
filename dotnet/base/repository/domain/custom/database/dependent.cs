// <copyright file="Dependent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("0cb8d2a7-4566-432f-9882-893b05a77f44")]
    #endregion
    public partial class Dependent : Object, Deletable, DerivationCounted
    {
        #region inherited properties
        public int DerivationCount { get; set; }

        #endregion

        #region Allors
        [Id("8859af04-ba38-42ce-8ac9-f428c3f92f31")]
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        #endregion
        public Dependee Dependee { get; set; }

        #region Allors
        [Id("9884955e-74ed-4f9d-9362-8e0274c53bf9")]
        #endregion
        public int Counter { get; set; }

        #region Allors
        [Id("e971733a-c381-4b5e-8e62-6bbd6d285bd7")]
        #endregion
        public int Subcounter { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion

    }
}
