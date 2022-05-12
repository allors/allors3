namespace Allors.E2E
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public static class LocatorExtensions
    {
        public static async Task<T> GetPropertyAsync<T>(this ILocator @this, string property)
        {
            const string expression = @"async (element, property) => {
    if(window.ng){
        var component = window.ng.getComponent(element);
        if(!component){
            var component = window.ng.getOwningComponent(element);
        }

        console.debug(component);

        return component[property];
    }

    return null;
}";

            var value = await @this.EvaluateAsync<T>(expression, property);
            return value;
        }
    }
}
