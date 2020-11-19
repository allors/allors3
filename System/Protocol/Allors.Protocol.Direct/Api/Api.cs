// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    using System.Linq;
    using Allors.Database;
    using Data;
    using Pull;

    public class Api
    {
        private readonly IDatabase database;

        public Api(IDatabase database) => this.database = database;

        public PullResponse Pull(PullRequest request)
        {
            using var session = this.database.CreateSession();

            var pulls = request.Pulls.Select(v =>
            {
                var visitor = new ToDatabaseVisitor(session);
                v.Accept(visitor);
                return visitor.Pull;
            });

            var response = new PullResponse();

            return null;
        }
    }
}
