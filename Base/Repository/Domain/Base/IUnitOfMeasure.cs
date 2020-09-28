// <copyright file="IUnitOfMeasure.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("b7215af5-97d6-42b0-9f6f-c1fccb2bc695")]
    #endregion
    public partial interface IUnitOfMeasure : Enumeration
    {
        #region Allors
        [Id("650F5B49-B4DA-4A34-938C-DF5EE2446298")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        string Abbreviation { get; set; }

        #region Allors
        [Id("4725FD6A-61FE-409C-8706-3A99548D9266")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        LocalisedText[] LocalisedAbbreviations { get; set; }

        #region Allors
        [Id("22d65b11-5d96-4632-9e95-72e30b885942")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("65c75f72-3bb4-415c-8aa7-b291d96dd157")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]

        UnitOfMeasureConversion[] UnitOfMeasureConversions { get; set; }

        #region Allors
        [Id("D35B0EDF-4196-4FE9-8DAA-8B93AEE3B70D")]
        #endregion
        [Workspace(Default)]
        string Symbol { get; set; }
    }
}
