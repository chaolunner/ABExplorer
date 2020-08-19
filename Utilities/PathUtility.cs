using UnityEngine;

namespace ABExplorer.Utilities
{
    public class PathUtility
    {
        public static string GetAbOutPath()
        {
            return string.Format("{0}/{1}", GetPlatformPath(), GetPlatformName());
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

#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    platformName = "Windows";
                    break;
                case UnityEditor.BuildTarget.StandaloneOSX:
                    platformName = "OSX";
                    break;
                case UnityEditor.BuildTarget.iOS:
                    platformName = "iOS";
                    break;
                case UnityEditor.BuildTarget.Android:
                    platformName = "Android";
                    break;
            }
#else
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
#endif
            return platformName;
        }

        public static string GetWWWPath()
        {
            var outPath = string.Empty;

            if (AbExplorerSettings.Settings.playMode == PlayMode.PackedPlayMode)
            {
                outPath = AbExplorerSettings.Settings.URL + "/" + GetPlatformName();
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