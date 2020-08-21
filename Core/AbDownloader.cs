using System;
using ABExplorer.Utilities;
using UnityEngine;

namespace ABExplorer.Core
{
    public class AbDownloader : IDisposable
    {
        public ulong contentSize;
        public ulong downloadedSize;
        public float progress;
        private readonly string _abDownLoadPath;
        private readonly Hash128 _abHash;

        public bool IsVersionCached => Caching.IsVersionCached(_abDownLoadPath, _abHash);

        public AbDownloader(string abName, Hash128 abHash)
        {
            _abHash = abHash;
            _abDownLoadPath = $"{PathUtility.GetWWWPath()}/{abName}";
        }

        public void Dispose()
        {
            contentSize = 0;
            downloadedSize = 0;
            progress = 0;
        }
    }
}