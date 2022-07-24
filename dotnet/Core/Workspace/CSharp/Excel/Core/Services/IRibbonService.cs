namespace Allors.Excel
{
    public interface IRibbonService
    {
        string UserLabel { get; set; }

        string AuthenticationLabel { get; set; }

        void Invalidate();
    }
}
