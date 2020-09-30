// <copyright file="ContactMechanism.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("b033f9c9-c799-485c-a199-914a9e9119d9")]
    #endregion
    public partial interface ContactMechanism : Auditable, Deletable
    {
        #region Allors
        [Id("3c4ab373-8ff4-44ef-a97d-d8a27513f69c")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        string Description { get; set; }

        #region Allors
        [Id("e2bd1f50-f891-4e3f-bac0-e9582b89e64c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        ContactMechanism[] FollowTo { get; set; }

        #region Allors
        [Id("E1DF1F98-5366-46CF-8A32-FB2ED04986AC")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        ContactMechanismType ContactMechanismType { get; set; }
    }
}
