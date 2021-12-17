// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("b05ce5be-e217-4cb8-abef-772b1221b3b2")]
    #endregion
    public partial interface Requirement : Transitional, UniquelyIdentifiable, Deletable, Searchable
    {
        #region ObjectStates
        #region Allors
        [Id("048F8002-E484-4AB7-880A-DB57B9F3634A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        RequirementState PreviousRequirementState { get; set; }

        #region Allors
        [Id("DCCF39B6-E085-4778-B732-F45A51BA4CA2")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        RequirementState LastRequirementState { get; set; }

        #region Allors
        [Id("B0009B12-8439-4F1A-81F6-7126E5B10D47")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        RequirementState RequirementState { get; set; }
        #endregion

        #region Allors
        [Id("dba6efbf-0098-42e1-a5e2-076c3d89b727")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        string RequirementStateName { get; set; }

        #region Allors
        [Id("abbc250a-550a-4328-9bea-a531f5ea76c6")]
        #endregion
        [Workspace(Default)]
        [Derived]
        [Required]
        string RequirementNumber { get; set; }

        #region Allors
        [Id("de474afa-f179-443e-bdd7-10a876ba2352")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        int SortableRequirementNumber { get; set; }

        #region Allors
        [Id("0f2c9ca2-9f2a-403e-8110-311fc0622326")]
        #endregion
        [Workspace(Default)]
        DateTime RequiredByDate { get; set; }

        #region Allors
        [Id("2202F95A-9D57-4792-BD8F-E35DECDD515E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        RequirementType RequirementType { get; set; }

        #region Allors
        [Id("98ea33ed-1edf-43d7-9e4d-86294b71b47d")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        string RequirementTypeName { get; set; }

        #region Allors
        [Id("2b828f2b-201d-4ae2-b64c-b2c5be713653")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Authorizer { get; set; }

        #region Allors
        [Id("3a6ba1d0-3efb-44f3-b90b-7e504ed11140")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Reason { get; set; }

        #region Allors
        [Id("3ecf2b1e-ac3d-4533-9da1-341111fca04d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        Requirement[] Children { get; set; }

        #region Allors
        [Id("43e11ee6-dcee-4a2c-80a7-8e04ee36ceb8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party NeededFor { get; set; }

        #region Allors
        [Id("5ed2803c-02d4-4187-8155-bee79e1a0829")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Party Originator { get; set; }

        #region Allors
        [Id("809e41dc-6da4-4f7d-87dd-db984e6d57f9")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        string OriginatorName { get; set; }

        #region Allors
        [Id("b6b7e1e9-6cce-4ca0-a085-0afd3a58ec50")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Facility Facility { get; set; }

        #region Allors
        [Id("bfce13c0-b5c2-46f0-b0fd-d0d288f8dc07")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Organisation ServicedBy { get; set; }

        #region Allors
        [Id("70042dae-d4b0-4f18-acf6-eac685d8cec2")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        string ServicedByName { get; set; }

        #region Allors
        [Id("61ad0824-c7ff-472d-9392-d3b74e987349")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        Priority Priority { get; set; }

        #region Allors
        [Id("e53e48f8-1a6a-42cd-83f0-296527e896e5")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        string PriorityName { get; set; }

        #region Allors
        [Id("c34694b4-bd8e-46e9-8bf1-fb1296738ab4")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        decimal EstimatedBudget { get; set; }

        #region Allors
        [Id("d902fe48-c91f-43fe-b402-e0d87606124a")]
        #endregion
        [Required]
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("f553ad3c-675f-4b97-95c9-42f4d85eb5f9")]
        #endregion
        [Workspace(Default)]
        int Quantity { get; set; }

        #region Allors
        [Id("49a550dd-a885-4669-9825-722281ec4056")]
        [Indexed]
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.ManyToMany)]
        Media[] Pictures { get; set; }

        #region Allors

        [Id("8B09FA26-51AC-4286-8304-439E54A1CB2A")]

        #endregion
        [Workspace(Default)]
        void Reopen();

        #region Allors

        [Id("F96CD431-5143-463E-9C6E-1703AFC2F5E1")]

        #endregion
        [Workspace(Default)]
        void Cancel();

        #region Allors

        [Id("b2c4db2c-b2c9-44a4-bb81-da353071735b")]

        #endregion
        [Workspace(Default)]
        void Close();
    }
}
