// <copyright file="DeploymentUsage.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("ca0f0654-3974-4e5e-a57e-593216c05e16")]
    #endregion
    public partial interface DeploymentUsage : Commentable, Period, Object
    {
        #region Allors
        [Id("50c6bc05-83ff-4d40-b476-51418355eb0c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        TimeFrequency Frequency { get; set; }
    }
}
