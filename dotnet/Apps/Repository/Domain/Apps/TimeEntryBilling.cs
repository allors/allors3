// <copyright file="TimeEntryBilling.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("DDDA4365-DD74-4664-8B7D-92C894AECA21")]
    #endregion
    public partial class TimeEntryBilling : Object, Deletable
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("A6C5F1EB-53B1-4A62-8F94-1ECCFD87048A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public TimeEntry TimeEntry { get; set; }

        #region Allors
        [Id("9D155FCD-CAF9-4150-A632-4FF5F3BAA2DF")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InvoiceItem InvoiceItem { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}
