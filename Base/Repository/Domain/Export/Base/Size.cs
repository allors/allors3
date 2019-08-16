namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("45f5a73c-34d8-4452-8f22-7a744bd6650b")]
    #endregion
    public partial class Size : Enumeration, ProductFeature
    {
        #region inherited properties
        public LocalisedText[] LocalisedNames { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public EstimatedProductCost[] EstimatedProductCosts { get; set; }

        public PriceComponent[] BasePrices { get; set; }

        public string Description { get; set; }

        public ProductFeature[] DependentFeatures { get; set; }

        public ProductFeature[] IncompatibleFeatures { get; set; }

        public VatRate VatRate { get; set; }

        #endregion

        #region inherited methods


        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {

        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

        #region Allors
        [Id("939C10A3-0F1B-4994-8E0E-28B6F70B7B2F")]
        #endregion
        [Workspace]
        public void Delete() { }
    }
}