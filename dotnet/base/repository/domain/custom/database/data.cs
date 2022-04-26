// <copyright file="Data.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("0E82B155-208C-41FD-B7D0-731EADBB5338")]
    #endregion
    [Workspace(Default)]
    public partial class Data : Object
    {
        #region inherited properties
        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("46964F62-AF12-4450-83DA-C695C4A0ECE8")]
        #endregion
        [Workspace(Default)]
        public bool Checkbox { get; set; }

        #region Allors
        [Id("7E098B17-2ECB-4D1C-AA73-80684394BD9B")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public Person[] Chips { get; set; }

        #region Allors
        [Id("46C310DE-8E36-412E-8068-A9D734734E74")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string String { get; set; }

        #region Allors
        [Id("35b7b205-80f6-4bdb-a201-7595985f6b15")]
        #endregion
        [Workspace(Default)]
        public Decimal Decimal { get; set; }

        #region Allors
        [Id("31D0A290-2637-452D-8462-4BBB744E3065")]
        #endregion
        [Workspace(Default)]
        public DateTime Date { get; set; }

        #region Allors
        [Id("487A0EF5-C987-4064-BF6B-0B7354EC4315")]
        #endregion
        [Workspace(Default)]
        public DateTime DateTime { get; set; }

        #region Allors
        [Id("940DAD46-78C6-44B3-93A2-4AE0137C2839")]
        #endregion
        [Workspace(Default)]
        public DateTime DateTime2 { get; set; }

        #region Allors
        [Id("BA910E25-0D71-43E1-8311-7C9620AC0CDE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Media File { get; set; }

        #region Allors
        [Id("BC8943B1-7A8A-4C67-B8DB-BC0ECFF478DF")]
        #endregion
        [Workspace(Default)]
        public DateTime Month { get; set; }

        #region Allors
        [Id("68515CCE-3E87-4D21-B5E5-2136CC3D4F5C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        public Media[] MultipleFiles { get; set; }

        #region Allors
        [Id("3AA7FE12-F9DC-43A8-ACA7-3EADAEE0D05D")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string RadioGroup { get; set; }

        #region Allors
        [Id("C5061BAE-0B3B-474D-ABAA-DDAD638B8DA1")]
        #endregion
        [Workspace(Default)]
        public int Slider { get; set; }

        #region Allors
        [Id("753E6310-B943-48E8-A9F6-306D2A5DB6E4")]
        #endregion
        [Workspace(Default)]
        public bool SlideToggle { get; set; }

        #region Allors
        [Id("7B18C411-5414-4E28-A7C1-5749347B673B")]
        #endregion
        [Workspace(Default)]
        [MediaType("text/plain")]
        [Size(-1)]
        public string PlainText { get; set; }

        #region Allors
        [Id("A01C4AD6-A07E-48D0-B3FB-A35ADEDC9050")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        [MediaType("text/markdown")]
        public string Markdown { get; set; }

        #region Allors
        [Id("BF21BDD8-07D8-460B-B8BF-4B69E5B96725")]
        #endregion
        [Workspace(Default)]
        [Size(-1)]
        [MediaType("text/html")]
        public string Html { get; set; }

        #region Allors
        [Id("6F504BB6-8A55-46EF-BC3E-503486AF7384")]
        #endregion
        [Workspace(Default)]
        public string Static { get; set; }

        #region Allors
        [Id("36FA4EB8-5EA9-4F56-B5AA-9908EF2B417F")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteFilter { get; set; }

        #region Allors
        [Id("C1C4D5D9-EEC0-44B5-9317-713E9AB2277E")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteOptions { get; set; }

        #region Allors
        [Id("7C2CC44F-1BE9-4C1C-9A99-8BC742DA7DEC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteAssignedFilter { get; set; }

        #region Allors
        [Id("7624D2D5-E2C7-40E9-A805-AC89A02EAC63")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteDerivedFilter { get; set; }

        #region Allors
        [Id("EB3F28B4-471E-45C2-B6EF-5F2C3612638A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteAssignedOptions { get; set; }

        #region Allors
        [Id("7E1531B0-9328-49CA-96CC-763E4F9877AE")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person AutocompleteDerivedOptions { get; set; }

        #region Allors
        [Id("5FA4E339-5955-42E7-ABF2-0C3C17F38351")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person Select { get; set; }

        #region Allors
        [Id("62E43E2D-892B-4A1E-A326-AE508DD10A79")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person SelectAssigned { get; set; }

        #region Allors
        [Id("D0976E4A-B93F-426C-94B3-BB175900523A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Person SelectDerived { get; set; }

        #region Allors
        [Id("90BA01A8-5831-484A-818E-2B660F7C3A9A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public LocalisedText[] LocalisedTexts { get; set; }

        #region Allors
        [Id("7AB21625-164A-4686-A59E-5D64013EE9CC")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public LocalisedText[] LocalisedMarkdowns { get; set; }

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
