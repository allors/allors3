// <copyright file="User.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;


    #region Allors
    [Id("a0309c3b-6f80-4777-983e-6e69800df5be")]
    #endregion
    public partial interface User : UniquelyIdentifiable, SecurityTokenOwner, UserPasswordReset, Deletable
    {
        #region Allors
        [Id("5e8ab257-1a1c-4448-aacc-71dbaaba525b")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string UserName { get; set; }

        #region Allors
        [Id("7397B596-D8FA-4E3C-8E0E-EA24790FE2E4")]
        #endregion
        [Size(256)]
        [Derived]
        string NormalizedUserName { get; set; }

        #region Allors
        [Id("ea0c7596-c0b8-4984-bc25-cb4b4857954e")]
        #endregion
        [Size(256)]
        string UserPasswordHash { get; set; }

        #region Allors
        [Id("c1ae3652-5854-4b68-9890-a954067767fc")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        string UserEmail { get; set; }

        #region Allors
        [Id("24977764-63D7-4B2B-9FD7-19D0A6F8CCDB")]
        #endregion
        [Size(256)]
        [Derived]
        string NormalizedUserEmail { get; set; }

        #region Allors
        [Id("0b3b650b-fcd4-4475-b5c4-e2ee4f39b0be")]
        #endregion
        [Required]
        bool UserEmailConfirmed { get; set; }

        #region Allors
        [Id("7792EAEE-6DC3-4E47-B5CB-0B6A08ED451F")]
        #endregion
        [Size(256)]
        string UserSecurityStamp { get; set; }

        #region Allors
        [Id("34F1AB65-94A0-4440-B770-1B79495DA77D")]
        #endregion
        [Size(256)]
        string UserPhoneNumber { get; set; }

        #region Allors
        [Id("DAD0F093-1D42-4F82-857B-D00255D1468A")]
        #endregion
        [Required]
        bool UserPhoneNumberConfirmed { get; set; }

        #region Allors
        [Id("C1D83C41-11D2-4B0D-8D4C-9FA85489CA32")]
        #endregion
        [Required]
        bool UserTwoFactorEnabled { get; set; }

        #region Allors
        [Id("00731A56-8921-443B-9F85-FB4AB2841019")]
        #endregion
        DateTime UserLockoutEnd { get; set; }

        #region Allors
        [Id("552F0C55-92E2-4923-A1FE-579170F4EE3C")]
        #endregion
        [Required]
        bool UserLockoutEnabled { get; set; }

        #region Allors
        [Id("01E02A45-5ECF-4E44-85FA-3A2A3678C0BB")]
        #endregion
        [Required]
        int UserAccessFailedCount { get; set; }

        #region Allors
        [Id("4C9FDD8A-D7D4-4F6C-9584-77C6E1FC90FD")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        Login[] Logins { get; set; }
    }
}
