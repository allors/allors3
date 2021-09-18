// <copyright file="C1.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("93CDFDD3-B3CE-424D-96A5-6BF9DCB84CF9")]
    #endregion
    public partial class BB : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("023301F0-E80E-4AAD-A3B0-7C0DFDEC688C")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public CC One2One { get; set; }

        #region Allors
        [Id("cafbdbb6-7c98-40a1-b45a-7113e9dc0467")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        public CC UnusedOne2One { get; set; }

        #region Allors
        [Id("e6db1022-2d9c-44f8-9ee2-16c198e2a1c9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public CC Many2One { get; set; }

        #region Allors
        [Id("bb6496f3-4226-434c-898d-c18f0a5c0f60")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        public CC UnusedMany2One { get; set; }

        #region Allors
        [Id("5325fcbb-8504-4ec1-9534-0d5dcbdc5996")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public CC[] One2Many { get; set; }

        #region Allors
        [Id("141caf8f-7f93-445a-901c-fca2f19bda30")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        public CC[] UnusedOne2Many { get; set; }

        #region Allors
        [Id("6b95816c-48f0-4106-a12b-cb64c7770961")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public CC[] Many2Many { get; set; }

        #region Allors
        [Id("57671b5a-c7ae-4403-90ba-9b0f31243c61")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        public CC[] UnusedMany2Many { get; set; }

        #region Allors
        [Id("C14C0CBA-3046-48A5-AAD8-9867352CD5F3")]
        #endregion
        [Size(256)]
        public string Name { get; set; }

        #region Allors
        [Id("FAF8CCED-1967-4A5D-86AB-61D85312E34A")]
        #endregion
        [Size(256)]
        public string Derived { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
