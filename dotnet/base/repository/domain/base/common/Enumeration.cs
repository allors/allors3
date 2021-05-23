// <copyright file="Enumeration.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Extent type.</summary>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("b7bcc22f-03f0-46fd-b738-4e035921d445")]
    #endregion
    public partial interface Enumeration : UniquelyIdentifiable, Object
    {
        #region Allors
        [Id("3d3ae4d0-bac6-4645-8a53-3e9f7f9af086")]
        #endregion
        [Indexed]
        [Required]
        [Size(256)]
        [Workspace(Default)]
        string Name { get; set; }

        #region Allors
        [Id("07e034f1-246a-4115-9662-4c798f31343f")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        LocalisedText[] LocalisedNames { get; set; }

        #region Allors
        [Id("f57bb62e-77a8-4519-81e6-539d54b71cb7")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        bool IsActive { get; set; }
    }
}
