using System;
using System.Collections.Generic;

namespace Allors.Domain
{
    public partial class GameModes
    {
        public static readonly Guid GrandSlamId = new Guid("7a369e7f-4eec-4023-8174-a52cab4a4d79");
        public static readonly Guid MisèreId = new Guid("b24d304d-6f5b-4f21-b6cd-3fac42dbf87d");
        public static readonly Guid OpenMisèreId = new Guid("ad662e27-c3ab-462d-9dda-20d570a25783");
        public static readonly Guid SoloId = new Guid("f82471af-ee9d-424f-9919-bbfb656e4cdf");
        public static readonly Guid SmallSlamId = new Guid("a0c2ad9b-07cc-4cf5-8672-0747bcc1bea3");
        public static readonly Guid ProposalAndAcceptanceId = new Guid("c3d3eb58-d98d-45fc-af69-3f8da567e7ec");
        public static readonly Guid TroelId = new Guid("a8f35a76-7b7d-43f9-9514-681fef1d566f");
        public static readonly Guid AbondanceId = new Guid("323b16d4-49ad-46d6-9ff5-f65c6f4c5675");

        private UniquelyIdentifiableSticky<GameMode> cache;

        public GameMode GrandSlam => this.Cache[GrandSlamId];

        public GameMode Misère => this.Cache[MisèreId];

        public GameMode OpenMisère => this.Cache[OpenMisèreId];

        public GameMode Solo => this.Cache[SoloId];

        public GameMode SmallSlam => this.Cache[SmallSlamId];

        public GameMode ProposalAndAcceptance => this.Cache[ProposalAndAcceptanceId];

        public GameMode Troel => this.Cache[TroelId];

        public GameMode Abondance => this.Cache[AbondanceId];


        private UniquelyIdentifiableSticky<GameMode> Cache => this.cache ?? (this.cache = new UniquelyIdentifiableSticky<GameMode>(this.Session));

        protected override void CustomSetup(Setup setup)
        {
            this.CreateGameType("Grand slam", GrandSlamId);
            this.CreateGameType("Misère", MisèreId);
            this.CreateGameType("Open misère", OpenMisèreId);
            this.CreateGameType("Solo", SoloId);
            this.CreateGameType("Small slam", SmallSlamId);
            this.CreateGameType("Proposal and acceptance", ProposalAndAcceptanceId);
            this.CreateGameType("Troel", TroelId);
            this.CreateGameType("Abondance", AbondanceId);
        }

        private GameMode CreateGameType(string name, Guid uniqueId)
        {
            return new GameModeBuilder(this.Session)
                            .WithName(name)
                            .WithUniqueId(uniqueId)
                            .WithIsActive(true)
                            .Build();
        }
    }
}
