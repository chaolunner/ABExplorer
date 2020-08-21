using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using ABExplorer.Extensions;
using ABExplorer.Utilities;

namespace ABExplorer.Core
{
    public class AbManifestLoader : IDisposable
    {
        private AssetBundleManifest _manifest;
        private readonly string _manifestRemotePath;
        private readonly string _manifestLocalPath;
        private AssetBundle _abReadManifest;
        public readonly List<string> assetBundleList;
        public bool IsDone { get; private set; }

        public AbManifestLoader()
        {
            var manifestName = PathUtility.GetPlatformName();
            _manifestRemotePath = $"{PathUtility.GetWWWPath()}/{manifestName}";
#if UNITY_EDITOR
            _manifestLocalPath = $"{PathUtility.GetAbOutPath()}/Cache/{manifestName}";
#else
            _manifestLocalPath = $"{PathUtility.GetAbOutPath()}/{manifestName}";
#endif
            _manifest = null;
            _abReadManifest = null;
            assetBundleList = new List<string>();
            IsDone = false;
        }

        public async Task LoadManifestAsync()
        {
            using (var uwr = UnityWebRequest.Get(_manifestRemotePath))
            {
                await uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(
                        $"{GetType()}/LoadManifestAsync()/UnityWebRequest download error, please check it! Manifest URL: {_manifestRemotePath}, Error Message: {uwr.error}");
                }
                else
                {
                    var path = Path.GetDirectoryName(_manifestLocalPath);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    try
                    {
                        File.WriteAllBytes(_manifestLocalPath, uwr.downloadHandler.data);
#if UNITY_IOS
                        UnityEngine.iOS.Device.SetNoBackupFlag(_manifestLocalPath);
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"{GetType()}/LoadManifestAsync()/Failed to save manifest data, please check it! Manifest Path: {_manifestLocalPath}, Error Message: {e}");
                    }
                }
            }

            if (File.Exists(_manifestLocalPath))
            {
                var abCreateRequest = AssetBundle.LoadFromFileAsync(_manifestLocalPath);
                await abCreateRequest;
                _abReadManifest = abCreateRequest.assetBundle;
                _manifest = _abReadManifest.LoadAsset(AbDefine.assetbundleManifest) as AssetBundleManifest;
                assetBundleList.AddRange(_manifest.GetAllAssetBundles());
                IsDone = true;
            }
        }

        public AssetBundleManifest GetAssetBundleManifest()
        {
            if (IsDone)
            {
                return _manifest;
            }

            Debug.LogError($"{GetType()}/GetAssetBundleManifest()/_manifest(field) is null, please check it!");
            return null;
        }

        public string[] GetAllDependencies(string abName)
        {
            if (_manifest != null && !string.IsNullOrEmpty(abName))
            {
                return _manifest.GetAllDependencies(abName);
            }

            return null;
        }

        public Hash128 GetAssetBundleHash(string abName)
        {
            if (_manifest != null && !string.IsNullOrEmpty(abName))
            {
                return _manifest.GetAssetBundleHash(abName);
            }

            return default;
        }

        public bool HasAssetBundle(string abName)
        {
            return assetBundleList.Contains(abName);
        }

        public void Dispose()
        {
            if (_abReadManifest != null)
            {
                _abReadManifest.Unload(true);
            }
        }
    }
}