using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace ABExplorer.Core
{
    public class MultiAbLoader : System.IDisposable
    {
        private readonly AbManifestLoader _manifestLoader;
        private Dictionary<string, AbLoader> _loaders;
        private Dictionary<string, AbRelation> _relations;
        private AbLoadCompleted _onLoadCompleted;

        public MultiAbLoader(AbManifestLoader manifestLoader, AbLoadCompleted onLoadCompleted = null)
        {
            _loaders = new Dictionary<string, AbLoader>();
            _relations = new Dictionary<string, AbRelation>();
            _manifestLoader = manifestLoader;
            _onLoadCompleted = onLoadCompleted;
        }

        public IEnumerator LoadAbAsync(string abName)
        {
            yield return LoadAbByRecursiveAsync(abName);
            _onLoadCompleted?.Invoke(abName);
        }

        private IEnumerator LoadAbByRecursiveAsync(string abName)
        {
            if (_relations == null)
            {
                yield break;
            }

            if (!_relations.ContainsKey(abName))
            {
                _relations.Add(abName, new AbRelation(abName));
            }

            var relation = _relations[abName];
            var dependencies = _manifestLoader.GetAllDependencies(abName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                relation.AddDependence(dependencies[i]);
                yield return LoadReferenceByRecursiveAsync(dependencies[i], abName);
            }

            if (_loaders == null)
            {
                yield break;
            }

            if (!_loaders.ContainsKey(abName))
            {
                _loaders.Add(abName, AbLoaderManager.Create(abName, _manifestLoader.GetAssetBundleHash(abName)));
            }

            yield return _loaders[abName].LoadAsync();
        }

        private IEnumerator LoadReferenceByRecursiveAsync(string abName, string refAbName)
        {
            if (_relations == null)
            {
                yield break;
            }

            if (_relations.ContainsKey(abName))
            {
                _relations[abName].AddReference(refAbName);
            }
            else
            {
                var relation = new AbRelation(abName);
                relation.AddReference(refAbName);
                _relations.Add(abName, relation);
                yield return LoadAbByRecursiveAsync(abName);
            }
        }

        public T LoadAsset<T>(string abName, string assetName, bool isCache) where T : Object
        {
            if (_loaders.ContainsKey(abName))
            {
                return _loaders[abName].LoadAsset<T>(assetName, isCache);
            }

            Debug.LogError(string.Format(
                "{0}/LoadAsset() can't found the AssetBundle, can't load the asset, plase check it! abName = {1}, assetName = {2}",
                GetType(), abName, assetName));
            return null;
        }

        public void Dispose()
        {
            try
            {
                var e = _loaders.GetEnumerator();
                while (e.MoveNext())
                {
                    AbLoaderManager.Dispose(e.Current.Key);
                }

                e.Dispose();
            }
            finally
            {
                _loaders.Clear();
                _loaders = null;
                _relations.Clear();
                _relations = null;
                _onLoadCompleted = null;
            }
        }
    }
}