using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using System.IO;
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
            _manifestRemotePath = string.Format("{0}/{1}", PathUtility.GetWWWPath(), manifestName);
#if UNITY_EDITOR
            _manifestLocalPath = string.Format("{0}Cache/{1}", PathUtility.GetAbOutPath(), manifestName);
#else
            _manifestLocalPath = string.Format("{0}/{1}", PathUtility.GetAbOutPath(), manifestName);
#endif
            _manifest = null;
            _abReadManifest = null;
            assetBundleList = new List<string>();
            IsDone = false;
        }

        public IEnumerator LoadManifestAsync()
        {
            using (var uwr = UnityWebRequest.Get(_manifestRemotePath))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(string.Format(
                        "{0}/LoadManifestAsync()/UnityWebRequest download error, please check it! Manifest URL: {1}, Error Message: {2}",
                        GetType(), _manifestRemotePath, uwr.error));
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
                        Debug.LogError(string.Format(
                            "{0}/LoadManifestAsync()/Failed to save manifest data, please check it! Manifest Path: {1}, Error Message: {2}",
                            GetType(), _manifestLocalPath, e));
                    }
                }
            }

            if (File.Exists(_manifestLocalPath))
            {
                var abCreateRequest = AssetBundle.LoadFromFileAsync(_manifestLocalPath);
                yield return abCreateRequest;
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

            Debug.LogError(String.Format("{0}/GetAssetBundleManifest()/_manifest(field) is null, please check it!",
                GetType()));
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