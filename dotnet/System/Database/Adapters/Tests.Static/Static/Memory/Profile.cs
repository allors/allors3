// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Generic;
    using Adapters;

    public class Profile : Adapters.Profile
    {
        public override Action[] Markers
        {
            get
            {
                var markers = new List<Action>
                {
                    () => { },
                    () => this.Transaction.Commit(),
                };

                if (Settings.ExtraMarkers)
                {
                    markers.Add(
                        () =>
                        {
                            this.Transaction.Checkpoint();
                        });
                }

                return markers.ToArray();
            }
        }

        public override IDatabase CreateDatabase() => this.CreateMemoryDatabase();
    }
}
