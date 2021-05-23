// <copyright file="WorkEffortVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region
    [Id("E86A7C06-1376-42C0-B901-2F64C6D0B1A6")]
    #endregion
    public partial interface WorkEffortVersion : Version
    {
        #region Allors
        [Id("4c541aac-1d5c-453c-bf7d-aba07bed78e1")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Organisation TakenBy { get; set; }

        #region Allors
        [Id("D2282505-6967-412B-8D92-53D10A8BE7BE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        WorkEffortState WorkEffortState { get; set; }

        #region Allors
        [Id("C8F73224-717F-4E55-99BC-23507CDE4967")]
        #endregion
        [Workspace(Default)]
        string Name { get; set; }

        #region Allors
        [Id("33ECA579-D79A-488F-A9E5-B760C6DD2E29")]
        #endregion
        [Size(-1)]
        [Workspace]
        string Description { get; set; }

        #region Allors
        [Id("4C4240A5-528B-497F-8846-AA7C99942C82")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Priority Priority { get; set; }

        #region Allors
        [Id("35A7A9A0-00C2-4548-BA8F-DCBDFDFD577E")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        WorkEffortPurpose[] WorkEffortPurposes { get; set; }

        #region Allors
        [Id("1744851E-CD98-4F34-AFD1-B1096E4DC23E")]
        #endregion
        [Workspace(Default)]
        DateTime ActualCompletion { get; set; }

        #region Allors
        [Id("9415C622-BE7B-4927-9E72-D14D663BDDE6")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledStart { get; set; }

        #region Allors
        [Id("379F481B-D393-4FCE-9754-C5743A938524")]
        #endregion
        [Workspace(Default)]
        DateTime ScheduledCompletion { get; set; }

        #region Allors
        [Id("55CD6EE3-11CE-4546-8296-43E6C6AF0402")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal ActualHours { get; set; }

        #region Allors
        [Id("97460292-E59B-4D71-90C7-31AC1FE5ADE4")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal EstimatedHours { get; set; }

        #region Allors
        [Id("1D7C6E6B-C871-4BC9-A0DE-77611BAC9F4A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        WorkEffort[] Precendencies { get; set; }

        #region Allors
        [Id("0954D5B6-228B-46F7-9929-99AEAC303F0D")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Facility Facility { get; set; }

        #region Allors
        [Id("AF35EA36-E0F3-45F9-A865-1820576DBDEB")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Deliverable[] DeliverablesProduced { get; set; }

        #region Allors
        [Id("2185ECCD-1FA7-401D-9FF3-24601A587E62")]
        #endregion
        [Workspace(Default)]
        DateTime ActualStart { get; set; }

        #region Allors
        [Id("DC486C30-23A6-4996-B6F2-990D994B5678")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        WorkEffort[] Children { get; set; }

        #region Allors
        [Id("5528643C-3081-4E1D-BA37-BB17BEC3FDEF")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        OrderItem OrderItemFulfillment { get; set; }

        #region Allors
        [Id("96DEF640-D70D-47F0-AE44-FD4C084F8128")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        WorkEffortType WorkEffortType { get; set; }

        #region Allors
        [Id("AC3B999B-A4C9-4BFD-A666-6AD131DB6D37")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Requirement[] RequirementFulfillments { get; set; }

        #region Allors
        [Id("CF1F73FA-0517-4D5C-81B9-35EE1243309F")]
        #endregion
        [Workspace(Default)]
        string SpecialTerms { get; set; }

        #region Allors
        [Id("B0E17757-358A-4490-82A7-3CC54C8302B2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        WorkEffort[] Concurrencies { get; set; }
    }
}
