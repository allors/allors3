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

            await @this.WaitForAngular();
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
                try
                {
                    var value = await @this.EvaluateAsync<bool?>(expression);
                    isStable = value ?? false;
                }
                catch (PlaywrightException) when (!@this.IsClosed)
                {
                    // A navigation (router redirect / post-login redirect) swapped or tore down the JS
                    // execution context mid-poll. That settling navigation is exactly what WaitForAngular
                    // is waiting for, not a defect, so keep polling until Angular re-stabilises in the new
                    // context. We gate on @this.IsClosed (a structural signal) rather than matching the
                    // exception message, so a Playwright wording change can't silently break this; a real
                    // page/browser teardown sets IsClosed and lets the exception propagate. A persistent
                    // failure is still bounded by the one-minute timeOut above.
                    isStable = false;
                }

                Thread.Sleep(Math.Min(10 * factor++, 100));
            }
        }
    }
}
