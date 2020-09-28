using System;

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("B02988B9-043C-47C0-99B5-C8149E92D1BA")]
    #endregion
    public partial class NonUnifiedPartBarcodePrint : Printable
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public PrintDocument PrintDocument { get; set; }

        #endregion

        #region Allors
        [Id("E350E850-7FF2-42E1-A253-C7712DE21A9E")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace(Default)]
        public NonUnifiedPart[] Parts { get; set; }

        #region Allors
        [Id("0cb73f54-31c8-4f9c-af50-492a90c7e94a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Locale Locale { get; set; }

        #region Allors
        [Id("ed177d0e-c542-404a-979d-4c70731d2860")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Print() { }

        #endregion
    }
}
