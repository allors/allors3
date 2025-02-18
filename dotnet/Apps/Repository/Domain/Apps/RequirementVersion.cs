// <copyright file="RequirementVersion.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7fad9b2d-bc58-4405-91b6-800f862ecdf9")]
    #endregion
    public partial interface RequirementVersion : Version
    {
        #region Allors
        [Id("BB7D5EA4-6D52-4E64-B18C-CD74FA010102")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        RequirementState RequirementState { get; set; }

        #region Allors
        [Id("5719FF6E-EEC1-4465-A9B0-6B0779CFE2B4")]
        #endregion
        [Workspace(Default)]
        DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("4F5BF6F8-C4B8-4E3B-A1F2-6E212322AB8B")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        RequirementType RequirementType { get; set; }

        #region Allors
        [Id("37EFDA28-11F6-45C1-8243-AE38F8DAD9C9")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Party Authorizer { get; set; }

        #region Allors
        [Id("86DF3995-B405-4A26-934C-40C47DFA48D9")]
        #endregion
        [Workspace(Default)]
        string Reason { get; set; }

        #region Allors
        [Id("58872073-760D-4E9C-9926-A6A784329010")]

        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        Requirement[] Children { get; set; }

        #region Allors
        [Id("82377E0E-9AA2-4571-BB4C-A859AFB1DB22")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Party NeededFor { get; set; }

        #region Allors
        [Id("367D85A1-D463-440C-A0F0-1DA4633EE6F1")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Party Originator { get; set; }

        #region Allors
        [Id("dd839107-60fb-49f9-9b4d-25abc4c0dbda")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Priority Priority { get; set; }

        #region Allors
        [Id("6775C4EA-06F2-48DC-854A-7913510D117A")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Facility Facility { get; set; }

        #region Allors
        [Id("2FA42AB7-9B48-44E2-B929-3D7040555425")]

        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Organisation ServicedBy { get; set; }

        #region Allors
        [Id("9E02E045-F22F-4D4B-9382-42593FDF7FD6")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal EstimatedBudget { get; set; }

        #region Allors
        [Id("0A5A967A-E35D-45E3-BE3D-588A9633E330")]
        #endregion
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("571C92CC-F79E-49D9-BCE4-DDADDABD7F6B")]
        #endregion
        [Workspace(Default)]
        int Quantity { get; set; }
    }
}
