using System.IO;
using System.Threading.Tasks;
using ABExplorer.Extensions;
using ABExplorer.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace ABExplorer.Core
{
    public class AbManifestManager
    {
        private readonly string _remotePath;
        private readonly string _localPath;
        private readonly string _tempPath;
        private readonly string _manifestRemotePath;
        private readonly string _manifestLocalPath;
        private AbManifest _abManifest;

        public static AbManifest AbManifest => Instance._abManifest;

        private static AbManifestManager _instance;
        public static AbManifestManager Instance => _instance ?? (_instance = new AbManifestManager());

        public AbManifestManager()
        {
            var manifestName = PathUtility.GetPlatformName();
            _remotePath = $"{PathUtility.GetWWWPath()}/manifest";
            _localPath = $"{PathUtility.GetAbCachePath()}/manifest";
            _tempPath = $"{PathUtility.GetAbCachePath()}/manifest_temp";
            _manifestRemotePath = $"{PathUtility.GetWWWPath()}/{manifestName}";
            _manifestLocalPath = $"{PathUtility.GetAbCachePath()}/{manifestName}";
        }

        private async Task DownloadFileAsync(string url, string path)
        {
            using (var uwr = UnityWebRequest.Get(url))
            {
                await uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(
                        $"{GetType()}/DownloadFileAsync() UnityWebRequest download error, please check it! URL: {url}, Error Message: {uwr.error}");
                }
                else
                {
                    var p = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(p) && !Directory.Exists(p))
                    {
                        Directory.CreateDirectory(p);
                    }

                    try
                    {
                        File.WriteAllBytes(path, uwr.downloadHandler.data);
#if UNITY_IOS
                        UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(
                            $"{GetType()}/DownloadFileAsync() failed to write file, please check it! Path: {path}, Error Message: {e}");
                    }
                }
            }
        }

        public async Task<AbUpdateMode> CheckUpdateAsync()
        {
            await DownloadFileAsync(_remotePath, _tempPath);
            if (!File.Exists(_localPath))
            {
                return AbUpdateMode.Force;
            }

            if (File.Exists(_tempPath))
            {
                _abManifest = JsonUtility.FromJson<AbManifest>(File.ReadAllText(_localPath));
                var temp = JsonUtility.FromJson<AbManifest>(File.ReadAllText(_tempPath));
                if (temp.version > _abManifest.version)
                {
                    return AbUpdateMode.Remind;
                }
            }

            return AbUpdateMode.None;
        }

        public async Task UpdateAsync()
        {
            if (File.Exists(_tempPath))
            {
                File.Move(_tempPath, _localPath);
            }

            await DownloadFileAsync(_manifestRemotePath, _manifestLocalPath);
            await LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (File.Exists(_localPath))
            {
                try
                {
                    _abManifest = JsonUtility.FromJson<AbManifest>(File.ReadAllText(_localPath));
                }
                catch (System.Exception e)
                {
                    Debug.LogError(
                        $"{GetType()}/UpdateAsync() failed to read AbManifest file, please check it! Error Message: {e}");
                    return;
                }
            }
            else
            {
                return;
            }

            if (File.Exists(_manifestLocalPath))
            {
                var abCreateRequest = AssetBundle.LoadFromFileAsync(_manifestLocalPath);
                await abCreateRequest;
                var assetBundle = abCreateRequest.assetBundle;
                var abManifest = _abManifest;
                abManifest.manifest = assetBundle.LoadAsset(AbDefine.assetbundleManifest) as AssetBundleManifest;
                _abManifest = abManifest;
            }
        }
    }
}