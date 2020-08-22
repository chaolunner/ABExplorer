using UnityEngine;

namespace ABExplorer.Utilities
{
    public class PathUtility
    {
        public static string GetAbOutPath()
        {
            return $"{GetPlatformPath()}/{GetPlatformName()}";
        }

        public static string GetAbCachePath()
        {
            return $"{Application.persistentDataPath}/{GetPlatformName()}";
        }

        private static string GetPlatformPath()
        {
            var platformPath = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    platformPath = Application.streamingAssetsPath;
                    break;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                    platformPath = Application.persistentDataPath;
                    break;
            }

            return platformPath;
        }

        public static string GetPlatformName()
        {
            var platformName = string.Empty;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    platformName = "Windows";
                    break;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    platformName = "OSX";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platformName = "iOS";
                    break;
                case RuntimePlatform.Android:
                    platformName = "Android";
                    break;
            }

            return platformName;
        }

        public static string GetWWWPath()
        {
            var outPath = string.Empty;

            if (AbExplorerSettings.Instance.playMode == PlayMode.PackedPlayMode)
            {
                outPath = AbExplorerSettings.Instance.URL + "/" + GetPlatformName();
            }
            else
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                        outPath = "file://" + GetAbOutPath();
                        break;
                    case RuntimePlatform.Android:
                        outPath = "jar:file://" + GetAbOutPath();
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        outPath = GetAbOutPath() + "/Raw/";
                        break;
                }
            }

            return outPath;
        }
    }
}