// <copyright file="Case.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Case
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.Case, this.M.Case.CaseState),
        };

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCaseState)
            {
                this.CaseState = new CaseStates(this.Strategy.Transaction).Opened;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }
        }

        public void AppsClose(CaseClose method)
        {
            this.CaseState = new CaseStates(this.Strategy.Transaction).Closed;
            method.StopPropagation = true;
        }

        public void AppsComplete(CaseComplete method)
        {
            this.CaseState = new CaseStates(this.Strategy.Transaction).Completed;
            method.StopPropagation = true;
        }

        public void AppsReopen(CaseReOpen method)
        {
            this.CaseState = new CaseStates(this.Strategy.Transaction).Opened;
            method.StopPropagation = true;
        }
    }
}
