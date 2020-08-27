// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Domain;

namespace Allors
{
    public partial class Setup
    {
        private void CustomOnPrePrepare()
        {
        }

        private void CustomOnPostPrepare()
        {
        }

        private void CustomOnPreSetup()
        {
        }

        private void CustomOnPostSetup()
        {
            var player1 = new PersonBuilder(this.session)
                .WithUserName("player1")
                .WithFirstName("player")
                .WithLastName("One")
                .Build();

            var player2 = new PersonBuilder(this.session)
                .WithUserName("player2")
                .WithFirstName("player")
                .WithLastName("Two")
                .Build();

            var player3 = new PersonBuilder(this.session)
                .WithUserName("player3")
                .WithFirstName("player")
                .WithLastName("Three")
                .Build();

            var player4 = new PersonBuilder(this.session)
                .WithUserName("player4")
                .WithFirstName("player")
                .WithLastName("Four")
                .Build();
        }
    }
}
