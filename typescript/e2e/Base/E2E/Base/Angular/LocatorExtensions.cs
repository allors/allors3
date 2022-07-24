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
     
        return component[property];
    }

    return null;
}";

            var value = await @this.EvaluateAsync<T>(expression, property);
            return value;
        }

        public static async Task<T> GetExpressionAsync<T>(this ILocator @this, string componentExpression)
        {
            const string expression = @"async (element, expression) => {
    if(window.ng){
        var component = window.ng.getComponent(element);
        if(!component){
            var component = window.ng.getOwningComponent(element);
        }

        return eval(expression);
    }

    return null;
}";

            var value = await @this.EvaluateAsync<T>(expression, componentExpression);
            return value;
        }
    }
}
