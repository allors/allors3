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
