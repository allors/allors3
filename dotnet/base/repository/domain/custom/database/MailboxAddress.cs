// <copyright file="MailboxAddress.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("7ee3b00b-4e63-4774-b744-3add2c6035ab")]
    #endregion
    [Plural("MailboxAddresses")]
    public partial class MailboxAddress : Object, Address
    {
        #region inherited properties
        public Place Place { get; set; }

        #endregion

        #region Allors
        [Id("03c9970e-d9d6-427d-83d0-00e0888f5588")]
        [Size(256)]
        #endregion
        public string PoBox { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
