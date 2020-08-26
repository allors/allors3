using System;
using System.Collections.Generic;

namespace Allors.Domain
{
    public partial class GameMode
    {
        public bool IsGrandSlam
        {
            get { return this.UniqueId == GameModes.GrandSlamId; }
        }

        public bool IsMisère
        {
            get { return this.UniqueId == GameModes.MisèreId; }
        }
        public bool IsOpenMisère
        {
            get { return this.UniqueId == GameModes.OpenMisèreId; }
        }
        public bool IsSmallSlam
        {
            get { return this.UniqueId == GameModes.SmallSlamId; }
        }
        public bool IsSolo
        {
            get { return this.UniqueId == GameModes.SoloId; }
        }
        public bool IsProposalAndAcceptance
        {
            get { return this.UniqueId == GameModes.ProposalAndAcceptanceId; }
        }
        public bool IsAbondance
        {
            get { return this.UniqueId == GameModes.AbondanceId; }
        }
        public bool IsTroel
        {
            get { return this.UniqueId == GameModes.TroelId; }
        }
    }
}
