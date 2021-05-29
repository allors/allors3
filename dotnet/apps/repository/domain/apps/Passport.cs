// <copyright file="Passport.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("827bc38b-6570-41d7-8ae1-f1bbdf4409f9")]
    #endregion
    public partial class Passport : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("85036007-8e01-4d90-9cfe-7b9c25e43537")]
        #endregion

        public DateTime IssueDate { get; set; }

        #region Allors
        [Id("dd30acd3-2e7b-49e6-9fcd-04cfdafb62d0")]
        #endregion

        public DateTime ExpiriationDate { get; set; }

        #region Allors
        [Id("eb3cdf1a-d577-46ff-9d0e-d709c6e7d9d9")]
        #endregion
        [Required]
        [Unique]
        [Size(256)]

        public string Number { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion

    }
}
