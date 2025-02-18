// <copyright file="CustomerShipmentSequence.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class CustomerShipmentSequence
    {
        public bool IsEnforcedSequence => Equals(this.UniqueId, CustomerShipmentSequences.EnforcedSequenceId);

        public bool IsRestartOnFiscalYear => Equals(this.UniqueId, CustomerShipmentSequences.RestartOnFiscalYearId);
    }
}
