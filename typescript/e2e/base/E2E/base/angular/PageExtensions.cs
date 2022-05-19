namespace Allors.E2E.Angular
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public static class PageExtensions
    {
        public static async Task NavigateAsync(this IPage @this, string url)
        {
            const string expression =
                @"async (url) => {
    var app = document.querySelector('allors-root');
    var component = window.ng.getComponent(app);
    var router = component.router;
    var ngZone = component.ngZone;

    ngZone.run(async ()=> {
        await router.navigateByUrl(url);
    });
}";
            await @this.EvaluateAsync(expression, url);
        }

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
    }
}
