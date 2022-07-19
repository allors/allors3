// <copyright file="Person.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("6E026CC2-1979-413A-A4B2-54B41667E013")]
    #endregion
    [Workspace(Default)]
    public partial class Person : User
    {
        #region inherited properties

        public Guid UniqueId { get; set; }

        public SecurityToken OwnerSecurityToken { get; set; }

        public Grant OwnerGrant { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string InExistingUserPassword { get; set; }

        public string InUserPassword { get; set; }

        public string UserPasswordHash { get; set; }

        public string UserEmail { get; set; }

        public string NormalizedUserEmail { get; set; }

        public bool UserEmailConfirmed { get; set; }

        public string UserSecurityStamp { get; set; }

        public string UserPhoneNumber { get; set; }

        public bool UserPhoneNumberConfirmed { get; set; }

        public bool UserTwoFactorEnabled { get; set; }

        public DateTime UserLockoutEnd { get; set; }

        public bool UserLockoutEnabled { get; set; }

        public int UserAccessFailedCount { get; set; }

        public Login[] Logins { get; set; }

        #endregion

        #region Allors
        [Id("ed4b710a-fe24-4143-bb96-ed1bd9beae1a")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string FirstName { get; set; }

        #region Allors
        [Id("eb18bb28-da9c-47b4-a091-2f8f2303dcb6")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string MiddleName { get; set; }

        #region Allors
        [Id("8a3e4664-bb40-4208-8e90-a1b5be323f27")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string LastName { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
        #region Allors
        [Id("2a25125f-3545-4209-afc6-523eb0d8851e")]
        #endregion
        public int Age { get; set; }

        #region Allors
        [Id("adf83a86-878d-4148-a9fc-152f56697136")]
        #endregion
        [Workspace(Default)]
        public DateTime BirthDate { get; set; }
        
        #region Allors
        [Id("4F2E08E1-2892-41F9-A71F-4148AD9803F4")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string SessionFullName { get; set; }

        #region Allors
        [Id("105CF367-F076-45F8-8E2A-2431BB2D65C7")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Derived]
        public string DomainFullName { get; set; }

        #region Allors
        [Id("0DDC847A-713D-4A19-9C6F-E8FE9175301D")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Derived]
        public string DomainGreeting { get; set; }

        #region Allors
        [Id("9B98B181-E2E6-499B-BD17-82C6E1D6679A")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Derived]
        public string CustomFullName { get; set; }

        #region Allors
        [Id("a8a3b4b8-c4f2-4054-ab2a-2eac6fd058e4")]
        #endregion
        public bool IsMarried { get; set; }

        #region Allors
        [Id("54f11f06-8d3f-4d58-bcdc-d40e6820fdad")]
        #endregion
        [Workspace(Default)]
        public bool IsStudent { get; set; }

        #region Allors
        [Id("6b626ba5-0c45-48c7-8b6b-5ea85e002d90")]
        #endregion
        public int ShirtSize { get; set; }

        #region Allors
        [Id("1b057406-3343-426b-ab5b-ceb93ba02446")]
        [Size(-1)]
        #endregion
        public string Text { get; set; }

        #region Allors
        [Id("15de4e58-c5ef-4ebb-9bf6-5ab06a02c5a4")]
        [Size(-1)]
        #endregion
        public string TinyMCEText { get; set; }

        #region Allors
        [Id("afc32e62-c310-421b-8c1d-6f2b0bb88b54")]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        #endregion
        public decimal Weight { get; set; }

        #region Allors
        [Id("5661A98D-A935-4325-9B28-9D86175B1BD6")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public Organisation CycleOne { get; set; }

        #region Allors
        [Id("2EB2AF4F-2BF4-475F-BB41-D740197F168E")]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        public Organisation[] CycleMany { get; set; }

        #region Allors
        [Id("46395751-D63E-4D84-9110-197BA6930ACC")]
        #endregion
        [Derived]
        public bool Owning { get; set; }

        #region Allors
        [Id("1198D746-8FEE-4B94-B878-09028A1FCC9F")]
        #endregion
        [Derived]
        public string DatabaseOnlyField { get; set; }

        #region Allors
        [Id("423735DF-1712-4C19-B5FE-121FF9BEF9F1")]
        #endregion
        [Workspace(Default)]
        [Derived]
        public string DefaultWorkspaceField { get; set; }

        #region Allors
        [Id("38CA55F8-675C-4CBC-A994-F9E0B5661D61")]
        [Size(256)]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public string DisplayName { get; set; }

        [Id("FAF120ED-09D1-4E42-86A6-F0D9FF75E03C")]
        public void Method() { }

        #region inherited methods

        public void Delete()
        {
        }

        #endregion
    }
}
