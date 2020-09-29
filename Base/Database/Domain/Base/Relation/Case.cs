// <copyright file="Case.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class Case
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.Case, this.M.Case.CaseState),
        };

        public void BaseOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCaseState)
            {
                this.CaseState = new CaseStates(this.Strategy.Session).Opened;
            }
        }

        public void BaseClose()
        {
            var closed = new CaseStates(this.Strategy.Session).Closed;
            this.CaseState = closed;
        }

        public void BaseComplete()
        {
            var completed = new CaseStates(this.Strategy.Session).Completed;
            this.CaseState = completed;
        }

        public void BaseReopen()
        {
            var opened = new CaseStates(this.Strategy.Session).Opened;
            this.CaseState = opened;
        }
    }
}
