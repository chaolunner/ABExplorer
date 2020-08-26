using UnityEngine;
using UnityEngine.Networking;

namespace ABExplorer.Core
{
    public delegate void AbLoadStart(Hash128 abHash);

    public delegate void AbLoadUpdate(Hash128 abHash, UnityWebRequest uwr);

    public delegate void AbLoadCompleted(Hash128 abHash);

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

    public enum AbUpdateMode
    {
        None,
        Force,
        Remind,
    }
}