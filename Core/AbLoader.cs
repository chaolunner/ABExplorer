using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using ABExplorer.Utilities;
using UnityEngine;

namespace ABExplorer.Core
{
    public class AbLoader : System.IDisposable
    {
        private bool _lock;
        private AssetLoader _assetLoader;
        private readonly AbLoadStart _onLoadStart;
        private readonly AbLoadUpdate _onLoadUpdate;
        private readonly AbLoadCompleted _onLoadCompleted;
        private readonly string _abName;
        private readonly string _abDownLoadPath;
        private readonly string _abCachePath;
        private readonly Hash128 _abHash;

        public AbLoader(string abName, Hash128 abHash, AbLoadStart onLoadStart = null, AbLoadUpdate onLoadUpdate = null,
            AbLoadCompleted onLoadCompleted = null)
        {
            _abName = abName;
            _abHash = abHash;
            _onLoadStart = onLoadStart;
            _onLoadUpdate = onLoadUpdate;
            _onLoadCompleted = onLoadCompleted;
            _abDownLoadPath = $"{PathUtility.GetWWWPath()}/{abName}";
            _abCachePath = $"{PathUtility.GetAbCachePath()}/{abHash}";
        }

        public async Task UpdateAbAsync()
        {
            if (File.Exists(_abCachePath))
            {
                return;
            }

            _lock = true;
            _onLoadStart?.Invoke(_abHash);

            using (var uwr = UnityWebRequest.Get(_abDownLoadPath))
            {
                uwr.SendWebRequest();

                while (!uwr.isDone)
                {
                    _onLoadUpdate?.Invoke(_abHash, uwr);
                    await Task.Yield();
                }

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(
                        $"{GetType()}/LoadAsync() UnityWebRequest download error, please check it! AssetBundle URL: {_abDownLoadPath}, Error Message: {uwr.error}");
                }
                else
                {
                    var path = Path.GetDirectoryName(_abCachePath);
                    if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    try
                    {
                        File.WriteAllBytes(_abCachePath, uwr.downloadHandler.data);
#if UNITY_IOS
                        UnityEngine.iOS.Device.SetNoBackupFlag(_abCachePath);
#endif
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(
                            $"{GetType()}/LoadAsync() failed to save asset bundle, please check it! Cache path: {_abCachePath}, Error Message: {e}");
                    }
                }
            }

            _lock = false;
        }

        public async Task LoadAbAsync()
        {
            while (_lock)
            {
                await Task.Yield();
            }

            if (_assetLoader != null)
            {
                _onLoadCompleted?.Invoke(_abHash);
                return;
            }

            await UpdateAbAsync();

            if (File.Exists(_abCachePath))
            {
                _assetLoader = new AssetLoader(AssetBundle.LoadFromFile(_abCachePath));
                _onLoadCompleted?.Invoke(_abHash);
            }
        }

        public T LoadAsset<T>(string assetName, bool isCache) where T : Object
        {
            if (_assetLoader != null)
            {
                return _assetLoader.LoadAsset<T>(assetName, isCache);
            }

            Debug.LogError($"{GetType()}/LoadAsset<T>() _assetLoader is null, please check it! assetName: {assetName}");
            return null;
        }

        public Task<T> LoadAssetAsync<T>(string assetName, bool isCache) where T : Object
        {
            if (_assetLoader != null)
            {
                return _assetLoader.LoadAssetAsync<T>(assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAssetAsync<T>() _assetLoader is null, please check it! assetName: {assetName}");
            return null;
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            if (_assetLoader != null)
            {
                _assetLoader.Unload(unloadAllLoadedObjects);
                if (unloadAllLoadedObjects)
                {
                    _assetLoader = null;
                }
            }
            else
            {
                Debug.LogError($"{GetType()}/Unload() _assetLoader is null, please check it!");
            }
        }

        public void Dispose()
        {
            if (_assetLoader != null)
            {
                _assetLoader.Dispose();
                _assetLoader = null;
            }
        }

        public string[] GetAllAssetNames()
        {
            if (_assetLoader != null)
            {
                return _assetLoader.GetAllAssetNames();
            }

            Debug.LogError($"{GetType()}/GetAllAssetNames() _assetLoader is null, please check it!");
            return null;
        }
    }
}