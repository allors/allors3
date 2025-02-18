// <copyright file="InvoiceSequence.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class InvoiceSequence
    {
        public bool IsEnforcedSequence => Equals(this.UniqueId, InvoiceSequences.EnforcedSequenceId);

        public bool IsRestartOnFiscalYear => Equals(this.UniqueId, InvoiceSequences.RestartOnFiscalYearId);
    }
}
