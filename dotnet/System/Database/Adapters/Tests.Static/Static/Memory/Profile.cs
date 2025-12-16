// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

        public IDatabase CreateDatabaseWithVersion1Mode(SerializationVersion1Mode mode)
            => this.CreateMemoryDatabaseWithVersion1Mode(mode);
    }
}
