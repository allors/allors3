// <copyright file="RepeatingPurchaseInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("B5E190E1-F05F-420E-915C-0E5553D88109")]
    #endregion
    public partial class RepeatingPurchaseInvoice : Object
    {
        #region inherited properties

        public Restriction[] Restrictions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("A2EC3A94-A529-4A5E-8B66-6CEE3F5231DD")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public Organisation Supplier { get; set; }

        #region Allors
        [Id("8CF664E5-A7E8-4686-83AF-1C58A3CC5132")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("51361693-38A4-4FF8-8C64-57E78521EBB9")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public TimeFrequency Frequency { get; set; }

        #region Allors
        [Id("C16AFF4B-7DDC-4E7E-87FD-33F12F8B3219")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public DayOfWeek DayOfWeek { get; set; }

        #region Allors
        [Id("506127E5-0E94-44A4-9274-09E3D6C0103F")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime NextExecutionDate { get; set; }

        #region Allors
        [Id("E95ED423-E581-4B3C-8DCA-9E8882B26FED")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public DateTime PreviousExecutionDate { get; set; }

        #region Allors
        [Id("B258B677-E0D0-4D22-9148-598960EA60A9")]
        #endregion
        [Workspace(Default)]
        public DateTime FinalExecutionDate { get; set; }

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
