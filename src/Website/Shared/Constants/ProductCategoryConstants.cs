using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Constants
{
    public class ProductCategoryConstants
    {
        public const string OpenModPlugin = "OpenMod Plugin";
        public const string RocketPlugin = "Rocket Plugin";
        public const string UScriptPlugin = "UScript Plugin";
        public const string ClientModule = "Client Module";

        public const string DefaultCategory = RocketPlugin;

        public static readonly string[] Categories = new string[]
        {
            RocketPlugin,
            OpenModPlugin,            
            UScriptPlugin,
            ClientModule
        };

        private static readonly Dictionary<string, string> CategoryIcons = new Dictionary<string, string>()
        {
            { RocketPlugin, "fas fa-rocket" },
            { OpenModPlugin, "fas fa-plug" },
            { UScriptPlugin, "fas fa-scroll" },
            { ClientModule, "fas fa-cube" }
        };

        public static string GetCategoryIcon(string category)
        {
            if (!CategoryIcons.TryGetValue(category, out string icon))
            {
                icon = "fas fa-folder";
            }

            return icon;
        }
    }
}
