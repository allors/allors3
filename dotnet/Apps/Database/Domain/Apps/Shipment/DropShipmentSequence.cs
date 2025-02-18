// <copyright file="DropShipmentSequence.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class DropShipmentSequence
    {
        public bool IsEnforcedSequence => Equals(this.UniqueId, DropShipmentSequences.EnforcedSequenceId);

        public bool IsRestartOnFiscalYear => Equals(this.UniqueId, DropShipmentSequences.RestartOnFiscalYearId);
    }
}
