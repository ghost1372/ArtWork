using System;

namespace ArtWork
{
    public class AppVar
    {
        public static readonly int NumberOfAllItemExist = 9360;
        public static readonly string imagesBaseUrl = "https://kraken99.blob.core.windows.net/images4000xn/";
        public static readonly string jsonBaseUrl = "https://kraken99.blob.core.windows.net/tileinfo/";

        #region Update Config
        public static string UpdateServer = "https://raw.githubusercontent.com/ghost1372/ArtWork/master/Updater.xml";
        public const string UpdateXmlTag = "ArtWork"; //Defined in Xml file
        public const string UpdateXmlChildTag = "AppVersion"; //Defined in Xml file
        public const string UpdateVersionTag = "version"; //Defined in Xml file
        public const string UpdateUrlTag = "url"; //Defined in Xml file
        public const string UpdateChangeLogTag = "changelog";

        public static bool IsVersionLater(string newVersion, string oldVersion)
        {
            // split into groups
            string[] cur = newVersion.Split('.');
            string[] cmp = oldVersion.Split('.');
            // get max length and fill the shorter one with zeros
            int len = Math.Max(cur.Length, cmp.Length);
            int[] vs = new int[len];
            int[] cs = new int[len];
            Array.Clear(vs, 0, len);
            Array.Clear(cs, 0, len);
            int idx = 0;
            // skip non digits
            foreach (string n in cur)
            {
                if (!Int32.TryParse(n, out vs[idx]))
                {
                    vs[idx] = -999; // mark for skip later
                }
                idx++;
            }
            idx = 0;
            foreach (string n in cmp)
            {
                if (!Int32.TryParse(n, out cs[idx]))
                {
                    cs[idx] = -999; // mark for skip later
                }
                idx++;
            }
            for (int i = 0; i < len; i++)
            {
                // skip non digits
                if ((vs[i] == -999) || (cs[i] == -999))
                {
                    continue;
                }
                if (vs[i] < cs[i])
                {
                    return (false);
                }
                else if (vs[i] > cs[i])
                {
                    return (true);
                }
            }
            return (false);
        }

        #endregion
    }
}
