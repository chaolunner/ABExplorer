using ABExplorer.Utilities;
using UnityEngine;

namespace ABExplorer.Core
{
    public class AbDownloader
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
            _abDownLoadPath = string.Format("{0}/{1}", PathUtility.GetWWWPath(), abName);
        }
    }
}