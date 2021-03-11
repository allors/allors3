// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests.Relation
{
    using Xunit;

    public class PersonTests : DomainTest, IClassFixture<Fixture>
    {
        public PersonTests(Fixture fixture) : base(fixture) { }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void GivenPerson_WhenDeriving_ThenRequiredRelationsMustExist(object data)
        {
            this.RegisterAdditionalDerivations((DerivationTypes)data);

            var builder = new PersonBuilder(this.Transaction);
            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void OneDeclarerAndThreeDefenderDerivationTest(object data)
        {
            this.RegisterAdditionalDerivations((DerivationTypes) data);

            var people = new People(this.Transaction);

            Person player1 = people.FindBy(M.Person.UserName, "player1");
            Person player2 = people.FindBy(M.Person.UserName, "player2");
            Person player3 = people.FindBy(M.Person.UserName, "player3");
            Person player4 = people.FindBy(M.Person.UserName, "player4");

            Scoreboard scoreboard = new ScoreboardBuilder(this.Transaction)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            GameModes GameTypes = new GameModes(this.Transaction);

            this.Transaction.Derive();

            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = GameTypes.Solo;
            game.AddDeclarer(player1);

            this.Transaction.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Defenders);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void TwoDeclarerAndTwoDefenderDerivationTest(object data)
        {
            this.RegisterAdditionalDerivations((DerivationTypes)data);

            var people = new People(this.Transaction);

            Person player1 = people.FindBy(M.Person.UserName, "player1");
            Person player2 = people.FindBy(M.Person.UserName, "player2");
            Person player3 = people.FindBy(M.Person.UserName, "player3");
            Person player4 = people.FindBy(M.Person.UserName, "player4");

            Scoreboard scoreboard = new ScoreboardBuilder(this.Transaction)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            GameModes GameTypes = new GameModes(this.Transaction);

            this.Transaction.Derive();

            var game = new GameBuilder(this.Transaction).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Transaction.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameMode = GameTypes.Misery;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Transaction.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Declarers);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }
    }
}
