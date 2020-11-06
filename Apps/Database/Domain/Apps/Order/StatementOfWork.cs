// <copyright file="StatementOfWork.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Linq;

    public partial class StatementOfWork
    {
        public bool IsDeletable =>
            (this.QuoteState.Equals(new QuoteStates(this.Strategy.Session).Created)
                || this.QuoteState.Equals(new QuoteStates(this.Strategy.Session).Cancelled)
                || this.QuoteState.Equals(new QuoteStates(this.Strategy.Session).Rejected));

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.StatementOfWork, this.M.StatementOfWork.QuoteState),
        };
    }
}
