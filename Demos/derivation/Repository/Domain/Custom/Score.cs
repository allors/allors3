using Allors.Repository.Attributes;

namespace Allors.Repository
{
    #region Allors
    [Id("2df33e72-cb19-4647-9160-ed10a2552729")]
    #endregion

    public class Score : Object, Deletable
    {
        #region Inherited Properties
        public Permission[] DeniedPermissions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors

        [Id("19b4ccc7-c396-474d-a3b2-ad631932545e")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace]
        public Person Player { get; set; }

        #region Allors

        [Id("e68d4b5c-41c7-41d1-93cb-b00f71bcb6bf")]

        #endregion Allors
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public int Value { get; set; }

        #region inherited methods
        public void Delete()
        {
        }
        public void OnBuild()
        {

        }

        public void OnDerive()
        {

        }

        public void OnInit()
        {

        }

        public void OnPostBuild()
        {

        }

        public void OnPostDerive()
        {

        }

        public void OnPreDerive()
        {

        }

        #endregion inherited methods
    }
}
