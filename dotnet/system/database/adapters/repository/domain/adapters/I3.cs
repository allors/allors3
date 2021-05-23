// <copyright file="I3.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("2d86277f-3993-4831-a7de-3640166d3d50")]
    #endregion
    public partial interface I3 : Object, S1234, S3
    {
        #region Allors
        [Id("00b706bb-681e-44ce-bbf3-c3b01bb11269")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        C4[] I3C4many2manies { get; set; }

        #region Allors
        [Id("25a3bcbf-cd9a-4735-879d-c5415b19cf88")]
        [Size(256)]
        #endregion
        string I3AllorsString { get; set; }

        #region Allors
        [Id("2b273c39-cc85-4585-806f-d991f43dda29")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        I4[] I3I4one2manies { get; set; }

        #region Allors
        [Id("3a55d57f-768f-4c11-9c18-baa5f3eeda8c")]
        [Multiplicity(Multiplicity.OneToMany)]
        #endregion
        C4[] I3C4one2manies { get; set; }

        #region Allors
        [Id("3f553db3-b490-4de5-b388-5d096d83de0d")]
        [Multiplicity(Multiplicity.ManyToMany)]
        #endregion
        I4[] I3I4many2manies { get; set; }

        #region Allors
        [Id("57f8f305-e1a9-452b-bcc1-febf7ccc346a")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        I4 I3I4many2one { get; set; }

        #region Allors
        [Id("cc48853e-46f3-4292-be9b-8a4937cea308")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C4 I3C4one2one { get; set; }

        #region Allors
        [Id("d36e7cf1-08d1-4333-b539-e50503c10934")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        I4 I3I4one2one { get; set; }

        #region Allors
        [Id("d5ff5333-6bbc-4bb5-8208-44e1d4b53aee")]
        [Multiplicity(Multiplicity.ManyToOne)]
        #endregion
        C4 I3C4many2one { get; set; }

        #region Allors
        [Id("e0cf6092-d865-4386-823b-a2906a3eab1a")]
        [Size(256)]
        #endregion
        string I3StringEquals { get; set; }

        #region Allors
        [Id("fb90c539-a392-4618-bb0b-9809a3a673aa")]
        [Multiplicity(Multiplicity.OneToOne)]
        #endregion
        C1 C1one2one { get; set; }
    }
}
