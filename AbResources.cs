using System.Collections;
using UnityEngine;
using System.IO;

namespace ABExplorer
{
    public static class AbResources
    {
        private static string[] _abResAbsolutePaths;
        private static string[] _abResRelativePaths;
        private const string AbResourcesName = "AB_Resources";

        public static IEnumerator DownloadAbAsync()
        {
            yield return AssetBundleManager.Instance.DownloadAbAsync();
        }

        public static void Unload(string sceneName)
        {
            AssetBundleManager.Instance.Unload(sceneName);
        }

        public static string[] GetAllPaths()
        {
            if (_abResAbsolutePaths != null)
            {
                return _abResAbsolutePaths;
            }

            _abResAbsolutePaths =
                Directory.GetDirectories(Application.dataPath, AbResourcesName, SearchOption.AllDirectories);
            _abResRelativePaths = new string[_abResAbsolutePaths.Length];
            for (int i = 0; i < _abResRelativePaths.Length; i++)
            {
                _abResAbsolutePaths[i] = _abResAbsolutePaths[i].Replace('\\', '/');
                _abResRelativePaths[i] = _abResAbsolutePaths[i].Substring(Application.dataPath.Length + 1);
            }

            return _abResAbsolutePaths;
        }

        public static T LoadAsset<T>(string path, bool isCache = false) where T : Object
        {
            var settings = AbExplorerSettings.Settings;
            if (settings.playMode == PlayMode.FastMode)
            {
#if UNITY_EDITOR
                GetAllPaths();
                for (int i = 0; i < _abResRelativePaths.Length; i++)
                {
                    var assetPath = string.Format("Assets/{0}/{1}", _abResRelativePaths[i], path);
                    var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                    if (obj != null)
                    {
                        return obj;
                    }
                }
#endif
            }

            var names = path.Split('/');
            if (names.Length > 2)
            {
                return AssetBundleManager.Instance.LoadAsset<T>(names[0].ToLower(),
                    string.Format("{0}/{1}.ab", names[0], names[1]).ToLower(), names[2], isCache);
            }

            if (names.Length == 2)
            {
                return AssetBundleManager.Instance.LoadAsset<T>(names[0].ToLower(),
                    string.Format("{0}/{1}.ab", names[0], names[0]).ToLower(), names[1], isCache);
            }

            if (names.Length == 1)
            {
                return AssetBundleManager.Instance.LoadAsset<T>(names[0].ToLower(),
                    string.Format("{0}/{1}.ab", names[0], names[0]).ToLower(), names[0], isCache);
            }
            
            return null;
        }
    }
}