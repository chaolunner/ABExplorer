using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task LoadAbAsync(string abName)
        {
            await LoadAbByRecursiveAsync(abName);
            _onLoadCompleted?.Invoke(abName);
        }

        private async Task LoadAbByRecursiveAsync(string abName)
        {
            if (_relations == null)
            {
                return;
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
                await LoadReferenceByRecursiveAsync(dependencies[i], abName);
            }

            if (_loaders == null)
            {
                return;
            }

            if (!_loaders.ContainsKey(abName))
            {
                _loaders.Add(abName, AbLoaderManager.Create(abName, _manifestLoader.GetAssetBundleHash(abName)));
            }

            await _loaders[abName].LoadAsync();
        }

        private async Task LoadReferenceByRecursiveAsync(string abName, string refAbName)
        {
            if (_relations == null)
            {
                return;
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
                await LoadAbByRecursiveAsync(abName);
            }
        }

        public T LoadAsset<T>(string abName, string assetName, bool isCache) where T : Object
        {
            if (_loaders.ContainsKey(abName))
            {
                return _loaders[abName].LoadAsset<T>(assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAsset<T>() can't found the AssetBundle, can't load the asset, plase check it! abName = {abName}, assetName = {assetName}");
            return null;
        }

        public Task<T> LoadAssetAsync<T>(string abName, string assetName, bool isCache) where T : Object
        {
            if (_loaders.ContainsKey(abName))
            {
                return _loaders[abName].LoadAssetAsync<T>(assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAssetAsync<T>() can't found the AssetBundle, can't load the asset, plase check it! abName = {abName}, assetName = {assetName}");
            return null;
        }

        public void Dispose()
        {
            try
            {
                var e = _loaders.GetEnumerator();
                while (e.MoveNext())
                {
                    AbLoaderManager.Unload(e.Current.Key);
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