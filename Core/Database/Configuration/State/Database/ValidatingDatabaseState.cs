// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain
{
    using Database;
    using Microsoft.AspNetCore.Http;

    public class ValidatingDatabaseState : DatabaseState
    {
        public ValidatingDatabaseState(IHttpContextAccessor httpContextAccessor = null) : base(httpContextAccessor) { }

        public override void OnInit(IDatabase database)
        {
            base.OnInit(database);

            this.DerivationFactory = new ValidatingDerivationFactory();
        }
    }
}
