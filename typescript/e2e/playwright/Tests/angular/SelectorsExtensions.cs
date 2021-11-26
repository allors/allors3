namespace Tests
{
    using System.Threading.Tasks;
    using Microsoft.Playwright;

    public static class SelectorsExtensions
    {
        public static async Task RegisterAngularSelectorsAsync(this ISelectors @this)
        {
            var options = new SelectorsRegisterOptions
            {
                Script =
@"
// Returns the first element matching given selector in the root's subtree.
query(root, selector) {
    return root.querySelector(selector);
},

// Returns all elements matching given selector in the root's subtree.
queryAll(root, selector)
{
return Array.from(root.querySelectorAll(selector));
}
",
            };

            await @this.RegisterAsync("angular", options);
        }
    }
}
