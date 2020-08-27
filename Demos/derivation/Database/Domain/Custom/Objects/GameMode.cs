namespace Allors.Domain
{
    public partial class GameMode
    {
        public bool IsGrandSlam => this.UniqueId == GameModes.GrandSlamId;
        public bool IsMisery => this.UniqueId == GameModes.MiseryId;
        public bool IsOpenMisery => this.UniqueId == GameModes.OpenMiseryId;
        public bool IsSmallSlam => this.UniqueId == GameModes.SmallSlamId;
        public bool IsSolo => this.UniqueId == GameModes.SoloId;
        public bool IsProposalAndAcceptance => this.UniqueId == GameModes.ProposalAndAcceptanceId;
        public bool IsAbondance => this.UniqueId == GameModes.AbondanceId;
        public bool IsTrull => this.UniqueId == GameModes.TrullId;
    }
}
