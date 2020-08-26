using System;
using System.Collections.Generic;
using UnityEngine;

namespace ABExplorer.Core
{
    [Serializable]
    public struct AbInfo
    {
        public string abHash;
        public ulong abSize;
    }

    [Serializable]
    public struct AbManifest
    {
        public uint version;
        public AbInfo[] abInfos;
        private Dictionary<Hash128, AbInfo> _abInfoMap;
        [NonSerialized] public AssetBundleManifest manifest;
        
        public bool IsValid => manifest != null;

        public Dictionary<Hash128, AbInfo> AbInfoMap
        {
            get
            {
                if (_abInfoMap == null)
                {
                    _abInfoMap = new Dictionary<Hash128, AbInfo>();

                    for (int i = 0; i < abInfos.Length; i++)
                    {
                        _abInfoMap.Add(Hash128.Parse(abInfos[i].abHash), abInfos[i]);
                    }
                }

                return _abInfoMap;
            }
        }

        public ulong GetAssetBundleSize(Hash128 abHash)
        {
            if (AbInfoMap.ContainsKey(abHash))
            {
                return AbInfoMap[abHash].abSize;
            }

            return 0;
        }

        public string[] GetAllDependencies(string abName)
        {
            return manifest.GetAllDependencies(abName);
        }

        public string[] GetAllAssetBundles()
        {
            return manifest.GetAllAssetBundles();
        }

        public Hash128 GetAssetBundleHash(string abName)
        {
            return manifest.GetAssetBundleHash(abName);
        }

        public bool HasAssetBundle(string abName)
        {
            return HasAssetBundle(GetAssetBundleHash(abName));
        }

        public bool HasAssetBundle(Hash128 abHash)
        {
            return AbInfoMap.ContainsKey(abHash);
        }
    }
}