namespace Allors.E2E
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public static class PageExtensions
    {
        public static async Task<string> Locale(this IPage @this)
        {
            const string expression = "window.navigator.userLanguage || window.navigator.language";
            return await @this.EvaluateAsync<string>(expression);
        }
    }
}
