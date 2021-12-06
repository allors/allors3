namespace Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public static class PageExtensions
    {
        public static async Task WaitForAngular(this IPage @this)
        {
            const string expression =
@"async () => {
    if(window.getAngularTestability){
        var app = document.querySelector('allors-root');
        var testability = window.getAngularTestability(app);
        if(testability){
            return testability.isStable();
        }
    }

    return false;
}";
            var timeOut = DateTime.Now.AddMinutes(1);

            var isStable = false;
            var factor = 1;
            while (!isStable && timeOut > DateTime.Now)
            {
                var value = await @this.EvaluateAsync<bool?>(expression);
                isStable = value ?? false;
                Thread.Sleep(Math.Min(10 * factor++, 100));
            }
        }

        public static async Task<string> Locale(this IPage @this)
        {
            const string expression = "window.navigator.userLanguage || window.navigator.language";
            var locale = await @this.EvaluateAsync<string>(expression);
            return locale;
        }
    }
}
