// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonTests.cs" company="Allors bvba">
//   Copyright 2002-2009 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Domain
{
    using Allors.Meta;
    using Xunit;

    public class PersonTests : DomainTest
    {
        [Fact]
        public void GivenPerson_WhenDeriving_ThenRequiredRelationsMustExist()
        {
            var builder = new PersonBuilder(this.Session);
            builder.Build();

            Assert.False(this.Session.Derive(false).HasErrors);
        }

        [Theory]
        [MemberData(nameof(TestedDerivationTypes))]
        public void OneDeclarerAndThreeDefenderDerivationTest(object data)
        {
            this.RegisterDerivations((DerivationTypes) data);

            var people = new People(this.Session);

            Person player1 = people.FindBy(M.Person.UserName, "speler1");
            Person player2 = people.FindBy(M.Person.UserName, "speler2");
            Person player3 = people.FindBy(M.Person.UserName, "speler3");
            Person player4 = people.FindBy(M.Person.UserName, "speler4");

            Scoreboard scoreboard = new ScoreboardBuilder(this.Session)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            GameTypes GameTypes = new GameTypes(this.Session);

            this.Session.Derive();

            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameType = GameTypes.AlleenGaan;
            game.AddDeclarer(player1);

            this.Session.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Defenders);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }

        [Fact]
        public void TwoDeclarerAndTwoDefenderDerivationTest()
        {
            this.RegisterDerivations(DerivationTypes.Fine);

            var people = new People(this.Session);

            Person player1 = people.FindBy(M.Person.UserName, "speler1");
            Person player2 = people.FindBy(M.Person.UserName, "speler2");
            Person player3 = people.FindBy(M.Person.UserName, "speler3");
            Person player4 = people.FindBy(M.Person.UserName, "speler4");

            Scoreboard scoreboard = new ScoreboardBuilder(this.Session)
                .WithPlayer(player1)
                .WithPlayer(player2)
                .WithPlayer(player3)
                .WithPlayer(player4)
                .Build();

            GameTypes GameTypes = new GameTypes(this.Session);

            this.Session.Derive();

            var game = new GameBuilder(this.Session).Build();
            scoreboard.AddGame(game);

            game.StartDate = this.Session.Now();
            game.EndDate = game.StartDate.Value.AddHours(1);

            //Act
            game.GameType = GameTypes.Miserie;
            game.AddDeclarer(player1);
            game.AddDeclarer(player2);

            this.Session.Derive();

            Assert.Contains(player1, game.Declarers);
            Assert.Contains(player2, game.Declarers);
            Assert.Contains(player3, game.Defenders);
            Assert.Contains(player4, game.Defenders);
        }
    }
}
