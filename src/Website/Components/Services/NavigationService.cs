using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Website.Components.Services
{
    public class NavigationService
    {
        private readonly NavigationManager manager;

        public NavigationService(NavigationManager manager)
        {
            this.manager = manager;
        }

        public string GetQueryString(string key)
        {
            Uri uri = manager.ToAbsoluteUri(manager.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out StringValues value))
            {
                return value;
            }

            return null;
        }

        public int GetQueryInt(string key)
        {
            string val = GetQueryString(key);
            if (val == null)
                return 0;

            int.TryParse(val, out int result);
            return result;
        }
    }
}
