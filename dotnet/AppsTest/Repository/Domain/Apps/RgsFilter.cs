// <copyright file="RgsFilter.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors

    [Id("e9fc4ce6-0a35-4683-a55e-232e05f7fbfa")]

    #endregion
    public partial class RgsFilter : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {
        }

        public void OnInit()
        {
        }

        public void OnPostDerive()
        {
        }

        #endregion

        #region Allors
        [Id("c685d7f3-a107-4dde-b2fc-d5f5c18e37f0")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseBase { get; set; }

        #region Allors
        [Id("1237f6a0-f0e6-4974-acd6-c3cd4855ed9f")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseExtended { get; set; }

        #region Allors
        [Id("545fa8c9-9ee1-421c-b126-7d80a54229a7")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseEz { get; set; }

        #region Allors
        [Id("1488b378-7d55-4402-9737-3b90dfa1c4d1")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseZzp { get; set; }

        #region Allors
        [Id("74087097-b9f1-4e8e-aa2c-59b52cbfc343")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool UseWoCo { get; set; }

        #region Allors
        [Id("9bde6a25-f8eb-4d61-a98b-eeeee3e947ed")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeOpeningBalance { get; set; }

        #region Allors
        [Id("938e71c1-7852-4740-900f-6d273e1fbf62")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeZzp{ get; set; }

        #region Allors
        [Id("a3c1e96e-9f08-49a2-a79e-5cdd5969bbbb")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeAgro { get; set; }

        #region Allors
        [Id("b0120773-74b7-4ce0-816d-03dc5553fcb7")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeWkr { get; set; }

        #region Allors
        [Id("54ff35ec-2cbc-49ed-b77f-0ae481e626c0")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeEz { get; set; }

        #region Allors
        [Id("59b3467b-4c6a-4282-a4ae-e894358e1b42")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeBv { get; set; }

        #region Allors
        [Id("f261f3bc-c6d7-4b15-8188-b0c38f72d7c8")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeWoCo { get; set; }

        #region Allors
        [Id("cad8c04d-a7bc-4641-b790-7edd63514efd")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeBank { get; set; }

        #region Allors
        [Id("0d1f73a6-cc4d-46a1-a411-d58720406bfe")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeOzw { get; set; }

        #region Allors
        [Id("27251022-37a1-4a9b-b4ec-c5b0fd151fd0")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeCashRegister { get; set; }

        #region Allors
        [Id("9175f338-7e1d-499d-9d0c-614d660c0863")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeLevel5 { get; set; }

        #region Allors
        [Id("5026b024-7970-43f2-83c7-cc160bff492f")]
        #endregion
        [Workspace(Default)]
        public bool ExcludeLevel5Extension { get; set; }
    }
}
