using System.Threading.Tasks;
using UnityEngine.Networking;
using ABExplorer.Utilities;
using UnityEngine;

namespace ABExplorer.Core
{
    public class AbLoader : System.IDisposable
    {
        private bool _lock;
        private bool _done;
        private AssetLoader _assetLoader;
        private readonly AbLoadStart _onLoadStart;
        private readonly AbLoadUpdate _onLoadUpdate;
        private readonly AbLoadCompleted _onLoadCompleted;
        private readonly string _abName;
        private readonly string _abDownLoadPath;
        private readonly Hash128 _abHash;

        public AbLoader(string abName, Hash128 abHash = new Hash128(), AbLoadStart onLoadStart = null,
            AbLoadUpdate onLoadUpdate = null, AbLoadCompleted onLoadCompleted = null)
        {
            _assetLoader = null;
            _abName = abName;
            _abHash = abHash;
            _onLoadStart = onLoadStart;
            _onLoadUpdate = onLoadUpdate;
            _onLoadCompleted = onLoadCompleted;
            _abDownLoadPath = $"{PathUtility.GetWWWPath()}/{abName}";
        }

        public async Task LoadAsync()
        {
            while (_lock)
            {
                await Task.Yield();
            }

            if (_assetLoader != null)
            {
                _onLoadCompleted?.Invoke(_abName);
                return;
            }

            _lock = true;
            _done = false;

            using (var uwr = new UnityWebRequest(_abDownLoadPath))
            {
                uwr.downloadHandler = new DownloadHandlerAssetBundle(uwr.url, _abHash, 0);
                uwr.SendWebRequest();
                _onLoadStart?.Invoke(_abName, uwr);
                while (!uwr.isDone)
                {
                    _onLoadUpdate?.Invoke(_abName, uwr);
                    await Task.Yield();
                }

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(
                        $"{GetType()}/LoadAsync()/UnityWebRequest download error, please check it! AssetBundle URL: {_abDownLoadPath}, Error Message: {uwr.error}");
                    _done = Caching.IsVersionCached(_abDownLoadPath, _abHash);
                }
                else
                {
                    _done = true;
                }

                if (_done)
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    _assetLoader = new AssetLoader(bundle);
                    _onLoadCompleted?.Invoke(_abName);
                }
            }

            _lock = false;
        }

        public T LoadAsset<T>(string assetName, bool isCache) where T : Object
        {
            if (_assetLoader != null)
            {
                return _assetLoader.LoadAsset<T>(assetName, isCache);
            }

            Debug.LogError($"{GetType()}/LoadAsset<T>()/_assetLoader(field) is null, please check it!");
            return null;
        }

        public Task<T> LoadAssetAsync<T>(string assetName, bool isCache) where T : Object
        {
            if (_assetLoader != null)
            {
                return _assetLoader.LoadAssetAsync<T>(assetName, isCache);
            }

            Debug.LogError($"{GetType()}/LoadAssetAsync<T>()/_assetLoader(field) is null, please check it!");
            return null;
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            if (_assetLoader != null)
            {
                _assetLoader.Unload(unloadAllLoadedObjects);
                _assetLoader = null;
            }
            else
            {
                Debug.LogError($"{GetType()}/Unload()/_assetLoader(field) is null, please check it! abName: {_abName}");
            }
        }

        public void Dispose()
        {
            if (_assetLoader != null)
            {
                _assetLoader.Dispose();
                _assetLoader = null;
            }
            else
            {
                Debug.LogError(
                    $"{GetType()}/Dispose()/_assetLoader(field) is null, please check it! abName: {_abName}");
            }
        }

        public string[] GetAllAssetNames()
        {
            if (_assetLoader != null)
            {
                return _assetLoader.GetAllAssetNames();
            }

            Debug.LogError($"{GetType()}/GetAllAssetNames()/_assetLoader(field) is null, please check it!");
            return null;
        }
    }
}