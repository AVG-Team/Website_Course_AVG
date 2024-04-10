using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Resources;

namespace Website_Course_AVG.Managers
{

    public static class ResourceHelper
    {
        private static ResourceManager resourceManager;

        static ResourceHelper()
        {
            resourceManager = new ResourceManager(typeof(Website_Course_AVG.Resources.Resource));
        }

        public static string GetResource(string key)
        {
            string value = resourceManager.GetString(key, CultureInfo.CurrentCulture);

            return value ?? $"{key}";
        }
    }

}