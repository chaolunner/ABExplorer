using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace ABExplorer.Core
{
    public static class AbLoaderManager
    {
        private static Dictionary<Hash128, AbLoader> _loaders = new Dictionary<Hash128, AbLoader>();
        private static Dictionary<Hash128, AbDownload> _downloads = new Dictionary<Hash128, AbDownload>();

        public static AbLoader Create(string abName, Hash128 abHash)
        {
            if (!_downloads.ContainsKey(abHash))
            {
                _downloads.Add(abHash, new AbDownload());
            }

            if (!_loaders.ContainsKey(abHash))
            {
                _loaders.Add(abHash,
                    new AbLoader(abName, abHash, OnDownloadStart, OnDownloadUpdate, OnDownloadCompleted));
            }

            return _loaders[abHash];
        }

        public static void OnDownloadStart(Hash128 abHash)
        {
            var download = _downloads[abHash];
            download.contentSize = AbManifestManager.AbManifest.GetAssetBundleSize(abHash);
            download.downloadedSize = 0;
            download.progress = 0;
            _downloads[abHash] = download;
        }

        public static void OnDownloadUpdate(Hash128 abHash, UnityWebRequest uwr)
        {
            var download = _downloads[abHash];
            download.downloadedSize = uwr.downloadedBytes;
            download.progress = Mathf.Clamp((float) download.downloadedSize / download.contentSize, 0, 0.9999f);
            _downloads[abHash] = download;
        }

        public static void OnDownloadCompleted(Hash128 abHash)
        {
            var download = _downloads[abHash];
            download.downloadedSize = download.contentSize;
            download.progress = 1;
            _downloads[abHash] = download;
        }

        public static float GetContentSize(AbUnit unit = AbUnit.Byte)
        {
            float size = 0;
            var e = _downloads.GetEnumerator();
            while (e.MoveNext())
            {
                size += e.Current.Value.contentSize;
            }

            e.Dispose();

            for (int i = 0; i < (int) unit; i++)
            {
                size /= 1024;
            }

            return size;
        }

        public static float GetDownloadedSize(AbUnit unit = AbUnit.Byte)
        {
            float size = 0;
            var e = _downloads.GetEnumerator();
            while (e.MoveNext())
            {
                size += e.Current.Value.downloadedSize;
            }

            e.Dispose();

            for (int i = 0; i < (int) unit; i++)
            {
                size /= 1024;
            }

            return size;
        }

        public static AbUnit ToUnit(ref float size)
        {
            var unit = 0;
            while (size > 1024)
            {
                size /= 1024;
                unit++;
            }

            return (AbUnit) unit;
        }

        public static float GetDownloadProgress()
        {
            var downloadProgress = 0f;
            var e = _downloads.GetEnumerator();
            while (e.MoveNext())
            {
                downloadProgress += e.Current.Value.progress;
            }

            e.Dispose();

            if (_downloads.Count > 0)
            {
                downloadProgress = downloadProgress / _downloads.Count;
            }

            if (GetContentSize() > 0)
            {
                downloadProgress = Mathf.Min(downloadProgress, GetDownloadedSize() / GetContentSize());
            }

            return downloadProgress;
        }

        public static int GetDownloadPercent()
        {
            return Mathf.RoundToInt(GetDownloadProgress() * 100);
        }

        public static void Unload(Hash128 abHash)
        {
            if (_loaders.ContainsKey(abHash))
            {
                _loaders[abHash].Dispose();
                _loaders.Remove(abHash);
            }

            if (_downloads.ContainsKey(abHash))
            {
                _downloads.Remove(abHash);
            }
        }
    }
}