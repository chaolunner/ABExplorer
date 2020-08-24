using System.Collections.Generic;
using System.Threading.Tasks;
using ABExplorer.Core;
using UnityEngine;

namespace ABExplorer
{
    public class AssetBundleManager
    {
        private static AssetBundleManager _instance;
        private readonly Dictionary<string, MultiAbLoader> _multiAbLoaders = new Dictionary<string, MultiAbLoader>();

        public static AssetBundleManager Instance => _instance ?? (_instance = new AssetBundleManager());
        
        public async Task LoadAbAsync(string sceneName, string abName, AbLoadCompleted onLoadCompleted = null)
        {
            if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(abName))
            {
                var settings = AbExplorerSettings.Instance;
                if (settings.playMode != PlayMode.FastMode)
                {
                    Debug.LogError($"{GetType()}/LoadAsync() sceneName or abName is null, please check it!");
                }

                return;
            }
            
            if (!AbManifestManager.AbManifest.HasAssetBundle(abName))
            {
                return;
            }

            if (!_multiAbLoaders.ContainsKey(sceneName))
            {
                _multiAbLoaders.Add(sceneName, new MultiAbLoader(AbManifestManager.AbManifest));
            }

            var loader = _multiAbLoaders[sceneName];
            if (loader != null)
            {
                await loader.LoadAbAsync(abName);
            }
            else
            {
                Debug.LogError($"{GetType()}/LoadAsync() multiAbLoader is null, please check it!");
            }

            onLoadCompleted?.Invoke(abName);
        }

        public T LoadAsset<T>(string sceneName, string abName, string assetName, bool isCache) where T : Object
        {
            if (_multiAbLoaders.ContainsKey(sceneName))
            {
                return _multiAbLoaders[sceneName].LoadAsset<T>(abName, assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAsset<T>() can't found the scene, can't load assets in the bundle, please check it! sceneName = {sceneName}");
            return null;
        }

        public Task<T> LoadAssetAsync<T>(string sceneName, string abName, string assetName, bool isCache)
            where T : Object
        {
            if (_multiAbLoaders.ContainsKey(sceneName))
            {
                return _multiAbLoaders[sceneName].LoadAssetAsync<T>(abName, assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAssetAsync<T>() can't found the scene, can't load assets in the bundle, please check it! sceneName = {sceneName}");
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
                Debug.LogError(
                    $"{GetType()}/Dispose() can't found the scene, can't dispose assets in the bundle, please check it! sceneName = {sceneName}");
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