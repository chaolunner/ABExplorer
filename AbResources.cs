using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using ABExplorer.Core;

namespace ABExplorer
{
    public class AbResources
    {
        private static AbResources _instance;
        private static string[] _abResAbsolutePaths;
        private static string[] _abResRelativePaths;
        private const string AbResourcesName = "AB_Resources";

        private static AbResources Instance => _instance ?? (_instance = new AbResources());

        public static AsyncOperationHandle DownloadAbAsync()
        {
            var handle = new AsyncOperationHandle(AssetBundleManager.Instance.DownloadAbAsync());
            return handle;
        }

        public static float GetDownloadProgress(out float downloadedSize, out float contentSize, out AbUnit unit)
        {
            contentSize = AbLoaderManager.GetContentSize();
            unit = AbLoaderManager.ToUnit(ref contentSize);
            downloadedSize = AbLoaderManager.GetDownloadedSize(unit);
            return AbLoaderManager.GetDownloadProgress();
        }

        public static AsyncOperationHandle LoadAbAsync(string path)
        {
            GetAbNames(path, out var sceneName, out var abName, out _);
            var handle = new AsyncOperationHandle(AssetBundleManager.Instance.LoadAbAsync(sceneName, abName));
            return handle;
        }

        public static AsyncOperationHandle<T> LoadAssetAsync<T>(string path, bool isCache = false) where T : Object
        {
            var handle = new AsyncOperationHandle<T>(Instance.LoadAssetAsyncTask<T>(path, isCache));
            return handle;
        }

        private async Task<T> LoadAssetAsyncTask<T>(string path, bool isCache = false) where T : Object
        {
            GetAbNames(path, out var sceneName, out var abName, out _);
            await AssetBundleManager.Instance.LoadAbAsync(sceneName, abName);
            var task2 = LoadAssetAsyncInternal<T>(path, isCache);
            await task2;
            return task2.Result;
        }

        public static void Unload(string sceneName)
        {
            AssetBundleManager.Instance.Unload(sceneName);
        }

        public static void GetAllPaths(out string[] absPaths, out string[] relPaths)
        {
            if (_abResAbsolutePaths == null)
            {
                _abResAbsolutePaths =
                    Directory.GetDirectories(Application.dataPath, AbResourcesName, SearchOption.AllDirectories);
            }

            if (_abResRelativePaths == null && _abResAbsolutePaths != null)
            {
                _abResRelativePaths = new string[_abResAbsolutePaths.Length];
                for (int i = 0; i < _abResRelativePaths.Length; i++)
                {
                    _abResAbsolutePaths[i] = _abResAbsolutePaths[i].Replace('\\', '/');
                    _abResRelativePaths[i] = _abResAbsolutePaths[i].Substring(Application.dataPath.Length + 1);
                }
            }

            absPaths = _abResAbsolutePaths;
            relPaths = _abResRelativePaths;
        }

        public static void GetAbNames(string path, out string sceneName, out string abName, out string assetName)
        {
            sceneName = string.Empty;
            abName = string.Empty;
            assetName = string.Empty;

            var settings = AbExplorerSettings.Instance;
            if (settings.playMode == PlayMode.FastMode && Application.isEditor)
            {
                GetAllPaths(out _, out _);
                for (int i = 0; i < _abResAbsolutePaths.Length; i++)
                {
                    if (File.Exists($"{_abResAbsolutePaths[i]}/{path}"))
                    {
                        assetName = $"Assets/{_abResRelativePaths[i]}/{path}";
                        break;
                    }
                }
            }
            else
            {
                var names = path.Split('/');
                if (names.Length > 2)
                {
                    sceneName = names[0].ToLower();
                    abName = $"{names[0]}/{names[1]}.ab".ToLower();
                    assetName = names[2];
                }
                else if (names.Length == 2)
                {
                    sceneName = names[0].ToLower();
                    abName = $"{names[0]}/{names[0]}.ab".ToLower();
                    assetName = names[1];
                }
                else if (names.Length == 1)
                {
                    sceneName = names[0].ToLower();
                    abName = $"{names[0]}/{names[0]}.ab".ToLower();
                    assetName = names[0];
                }
            }
        }

        private static Task<T> LoadAssetAsyncInternal<T>(string path, bool isCache = false) where T : Object
        {
            GetAbNames(path, out var sceneName, out var abName, out var assetName);
            var settings = AbExplorerSettings.Instance;
            if (settings.playMode == PlayMode.FastMode)
            {
#if UNITY_EDITOR
                var task = new Task<T>(() => UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetName));
                task.RunSynchronously();
                return task;
#endif
            }

            return AssetBundleManager.Instance.LoadAssetAsync<T>(sceneName, abName, assetName, isCache);
        }
    }
}