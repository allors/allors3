using Allors.Repository.Attributes;
using System;

namespace Allors.Repository
{
    #region Allors
    [Id("23207fbd-0a09-45db-94e8-d57a9cf84e3a")]
    #endregion

    public class GameMode : Enumeration
    {
        #region inherited properties
        public string Name { get; set; }
        public LocalisedText[] LocalisedNames { get; set; }
        public bool IsActive { get; set; }
        public Restriction[] Restrictions { get; set; }
        public SecurityToken[] SecurityTokens { get; set; }
        public Guid UniqueId { get; set; }
        #endregion

        #region inherited methods
        public void OnBuild()
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

        #endregion inherited methods
    }
}
