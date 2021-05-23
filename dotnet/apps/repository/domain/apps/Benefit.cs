// <copyright file="Benefit.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("8cea6932-d589-4b5b-99b8-ffba33936f8f")]
    #endregion
    public partial class Benefit : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0c3efe28-a934-467d-a361-293175330b62")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal EmployerPaidPercentage { get; set; }

        #region Allors
        [Id("6239a2cc-97ce-49cb-b5aa-23e1e9ff7e71")]
        #endregion
        [Size(-1)]

        public string Description { get; set; }

        #region Allors
        [Id("6e1e0ef1-0e2a-406f-afa4-a6c97657801f")]
        #endregion
        [Required]
        [Size(256)]

        public string Name { get; set; }

        #region Allors
        [Id("89460288-d09e-43f9-960a-86b6c1e2e0be")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal AvailableTime { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}
