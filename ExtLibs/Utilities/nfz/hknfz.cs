namespace MissionPlanner.Utilities.nfz
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using Flurl;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using GeoJSON.Net;
    using GeoJSON.Net.Feature;
    using GeoJSON.Net.Geometry;
    using System.IO;
    using log4net;

    public class HK
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static string filecache = Path.Combine(Settings.GetDataDirectory(), "hknfz.json");

        // always show it no matter what
        public static bool forceshow
        {
            get
            {
                return Settings.Instance.GetBoolean("hknfzforceshow");
            }
            set 
            { 
                Settings.Instance["hknfzforceshow"] = value.ToString();
            }
        }

        // user has chosen to show/hide it
        public static bool show
        {
            get
            {
                return Settings.Instance.GetBoolean("hknfzshow", true);
            }
            set
            {
                Settings.Instance["hknfzshow"] = value.ToString();
            }
        }

        public static bool asked
        {
            get
            {
                return Settings.Instance.GetBoolean("hknfzshowask", false);
            }
            set
            {
                Settings.Instance["hknfzshowask"] = value.ToString();
            }
        }

        public delegate bool cfnofly();
        public static event cfnofly ConfirmNoFly;

        public static async System.Threading.Tasks.Task<FeatureCollection> LoadNFZ()
        {
            // Disabled: no external connections in offline-hardened build
            return null;
        }
    }
}