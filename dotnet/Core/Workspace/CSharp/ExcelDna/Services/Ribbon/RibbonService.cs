namespace ExcelDNA
{
    using Allors.Excel;

    public class RibbonService : IRibbonService
    {
        private Ribbon ribbon;

        public RibbonService(Ribbon ribbon) => this.ribbon = ribbon;

        public string UserLabel { get => this.ribbon.UserLabel; set => this.ribbon.UserLabel = value; }

        public string AuthenticationLabel { get => this.ribbon.AuthenticationLabel; set => this.ribbon.AuthenticationLabel = value; }

        public void Invalidate() => this.ribbon.Invalidate();
    }
}
