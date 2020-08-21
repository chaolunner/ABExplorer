using System.Collections;
using System.Threading.Tasks;
using ABExplorer.Extensions;
using UnityEngine;

namespace ABExplorer.Core
{
    public class AssetLoader : System.IDisposable
    {
        private readonly AssetBundle _assetBundle;
        private readonly Hashtable _cache;

        public AssetLoader(AssetBundle bundle)
        {
            if (bundle != null)
            {
                _assetBundle = bundle;
                _cache = new Hashtable();
            }
            else
            {
                Debug.LogError($"{GetType()}/AssetLoader() param is null, please check it!");
            }
        }

        public string[] GetAllAssetNames()
        {
            return _assetBundle.GetAllAssetNames();
        }

        public T LoadAsset<T>(string assetName, bool isCache) where T : Object
        {
            if (_cache.Contains(assetName))
            {
                return _cache[assetName] as T;
            }

            var asset = _assetBundle.LoadAsset<T>(assetName);
            if (asset != null && isCache)
            {
                _cache.Add(assetName, asset);
            }
            else if (asset == null)
            {
                Debug.LogError($"{GetType()}/LoadAsset<T>() return value is null, please check it!");
            }

            return asset;
        }

        public async Task<T> LoadAssetAsync<T>(string assetName, bool isCache) where T : Object
        {
            if (_cache.Contains(assetName))
            {
                return _cache[assetName] as T;
            }

            var request = _assetBundle.LoadAssetAsync<T>(assetName);
            await request;

            var asset = request.asset as T;
            if (asset != null && isCache)
            {
                _cache.Add(assetName, asset);
            }
            else if (asset == null)
            {
                Debug.LogError($"{GetType()}/LoadAssetAsync<T>() return value is null, please check it!");
            }

            return asset;
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            _assetBundle.Unload(unloadAllLoadedObjects);
        }

        public void Dispose()
        {
            Unload(true);
        }
    }
}