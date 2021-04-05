// <copyright file="Two.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
   

    public partial class Pages
    {
        public static readonly Guid IndexId = new Guid("A88D6A90-43F0-49B6-83D6-B05B2F783F9D");

        private UniquelyIdentifiableCache<Page> cache;

        public Cache<Guid, Page> Cache => this.cache ??= new UniquelyIdentifiableCache<Page>(this.Transaction);

        public Page Index => this.Cache[IndexId];

        protected override void CustomPrepare(Setup setup) => setup.AddDependency(this.ObjectType, this.M.Media);

        protected override void CustomSetup(Setup setup)
        {
            var medias = new Medias(this.Transaction);

            var merge = this.Cache.Merger().Action();

            merge(IndexId, v =>
            {
                v.Name = "About";
                v.Content = medias.About;
            });
        }
    }
}
