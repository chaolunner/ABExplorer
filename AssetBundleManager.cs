using System.Collections.Generic;
using System.Collections;
using ABExplorer.Core;
using UnityEngine;

namespace ABExplorer
{
    public class AssetBundleManager : MonoBehaviour
    {
        private static AssetBundleManager _instance;
        private readonly Dictionary<string, MultiAbLoader> _multiAbLoaders = new Dictionary<string, MultiAbLoader>();
        public AbManifestLoader manifestLoader;

        public static AssetBundleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("AssetBundleManager").AddComponent<AssetBundleManager>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        private IEnumerator LoadManifestAsync()
        {
            if (manifestLoader == null)
            {
                manifestLoader = new AbManifestLoader();
                StartCoroutine(manifestLoader.LoadManifestAsync());
            }

            while (!manifestLoader.IsDone)
            {
                yield return null;
            }
        }

        public IEnumerator DownloadAbAsync()
        {
            yield return LoadManifestAsync();

            for (int i = 0; i < manifestLoader.assetBundleList.Count; i++)
            {
                var abName = manifestLoader.assetBundleList[i];
                var sceneName = abName.Substring(0, abName.IndexOf("/"));
                StartCoroutine(LoadAbAsync(sceneName, abName));
            }

            while (AbLoaderManager.GetDownloadProgress() < 1)
            {
                yield return null;
            }

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public IEnumerator LoadAbAsync(string sceneName, string abName, AbLoadCompleted onLoadCompleted = null)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(abName))
            {
                Debug.LogError(string.Format("{0}/LoadAsync() sceneName or abName is null, please check it!",
                    GetType()));
                yield return null;
            }

            yield return LoadManifestAsync();

            if (!manifestLoader.HasAssetBundle(abName))
            {
                yield break;
            }
            
            if (!_multiAbLoaders.ContainsKey(sceneName))
            {
                _multiAbLoaders.Add(sceneName, new MultiAbLoader(manifestLoader));
            }

            var loader = _multiAbLoaders[sceneName];
            if (loader != null)
            {
                yield return loader.LoadAbAsync(abName);
            }
            else
            {
                Debug.LogError(string.Format("{0}/LoadAsync() multiAbLoader is null, please check it!", GetType()));
            }
            onLoadCompleted?.Invoke(abName);
        }

        public T LoadAsset<T>(string sceneName, string abName, string assetName, bool isCache) where T : Object
        {
            if (_multiAbLoaders.ContainsKey(sceneName))
            {
                return _multiAbLoaders[sceneName].LoadAsset<T>(abName, assetName, isCache);
            }

            Debug.LogError(string.Format(
                "{0}/LoadAsset() can't found the scene, can't load assets in the bundle, please check it! sceneName = {1}",
                GetType(), sceneName));
            return null;
        }

        public void Unload(string sceneName)
        {
            if (_multiAbLoaders.ContainsKey(sceneName))
            {
                _multiAbLoaders[sceneName].Dispose();
            }
            else
            {
                Debug.LogError(string.Format(
                    "{0}/Dispose() can't found the scene, can't dispose assets in the bundle, please check it! sceneName = {1}",
                    GetType(), sceneName));
            }

            _multiAbLoaders.Remove(sceneName);
        }

        private void OnDestroy()
        {
            var e = _multiAbLoaders.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Dispose();
            }

            e.Dispose();
            _multiAbLoaders.Clear();
        }
    }
}