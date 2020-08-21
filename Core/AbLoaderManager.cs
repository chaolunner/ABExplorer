using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace ABExplorer.Core
{
    public static class AbLoaderManager
    {
        private static Dictionary<string, AbLoader> _loaders = new Dictionary<string, AbLoader>();
        private static Dictionary<string, AbDownloader> _downloaders = new Dictionary<string, AbDownloader>();

        public static AbLoader Create(string abName, Hash128 abHash = new Hash128())
        {
            if (!_loaders.ContainsKey(abName))
            {
                _loaders.Add(abName,
                    new AbLoader(abName, abHash, OnDownloadStart, OnDownloadUpdate, OnDownloadCompleted));
            }

            if (!_downloaders.ContainsKey(abName))
            {
                _downloaders.Add(abName, new AbDownloader(abName, abHash));
            }

            return _loaders[abName];
        }

        public static void OnDownloadStart(string abName, UnityWebRequest uwr)
        {
            var downloader = _downloaders[abName];
            var size = uwr.GetResponseHeader("Content-Length");
            downloader.contentSize = System.Convert.ToUInt64(size);
            downloader.downloadedSize = uwr.downloadedBytes;
            if (downloader.contentSize > 0 && downloader.downloadedSize == downloader.contentSize)
            {
                downloader.downloadedSize = (ulong) (downloader.downloadedSize * 0.99f);
            }

            downloader.progress =
                Mathf.Clamp(
                    downloader.contentSize == 0
                        ? uwr.downloadProgress
                        : (float) downloader.downloadedSize / downloader.contentSize, 0, 0.99f);
        }

        public static void OnDownloadUpdate(string abName, UnityWebRequest uwr)
        {
            var downloader = _downloaders[abName];
            if (downloader.contentSize == 0)
            {
                string size = uwr.GetResponseHeader("Content-Length");
                downloader.contentSize = System.Convert.ToUInt64(size);
            }

            downloader.downloadedSize = uwr.downloadedBytes;
            if (downloader.contentSize > 0 && downloader.downloadedSize == downloader.contentSize)
            {
                downloader.downloadedSize = (ulong) (downloader.downloadedSize * 0.99f);
            }

            downloader.progress =
                Mathf.Clamp(
                    downloader.contentSize == 0
                        ? uwr.downloadProgress
                        : (float) downloader.downloadedSize / downloader.contentSize, 0, 0.99f);
        }

        public static void OnDownloadCompleted(string abName)
        {
            var downloader = _downloaders[abName];
            downloader.downloadedSize = downloader.contentSize;
            downloader.progress = 1;
        }

        public static float GetContentSize(AbUnit unit = AbUnit.Byte)
        {
            float size = 0;
            var e = _downloaders.GetEnumerator();
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
            var e = _downloaders.GetEnumerator();
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
            var e = _downloaders.GetEnumerator();
            while (e.MoveNext())
            {
                downloadProgress += e.Current.Value.progress;
            }

            e.Dispose();

            if (_downloaders.Count > 0)
            {
                downloadProgress = downloadProgress / _downloaders.Count;
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

        public static void Unload(string abName)
        {
            if (_loaders.ContainsKey(abName))
            {
                _loaders[abName].Dispose();
                _loaders.Remove(abName);
            }

            if (_downloaders.ContainsKey(abName))
            {
                _downloaders[abName].Dispose();
                _downloaders.Remove(abName);
            }
        }
    }
}