using UnityEngine.Networking;

namespace ABExplorer.Core
{
    public delegate void AbLoadStart(string abName, UnityWebRequest uwr);
    public delegate void AbLoadUpdate(string abName, UnityWebRequest uwr);
    public delegate void AbLoadCompleted(string abName);

    public static class AbDefine
    {
        public static string assetbundleManifest = "AssetBundleManifest";
    }
    
    public enum AbUnit
    {
        Byte = 0,
        KB = 1,
        MB = 2,
        GB = 3,
    }
}
