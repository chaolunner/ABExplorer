using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ABExplorer.Core
{
    public class MultiAbLoader : System.IDisposable
    {
        private AbManifest _abManifest;
        private Dictionary<Hash128, AbLoader> _loaders;
        private Dictionary<Hash128, AbRelation> _relations;

        public MultiAbLoader(AbManifest abManifest)
        {
            _loaders = new Dictionary<Hash128, AbLoader>();
            _relations = new Dictionary<Hash128, AbRelation>();
            _abManifest = abManifest;
        }

        private async Task ProcessRelationsAsync(string abName)
        {
            await ProcessRelationsByRecursiveAsync(abName);
        }

        private async Task ProcessRelationsByRecursiveAsync(string abName)
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);

            if (_relations.ContainsKey(abHash))
            {
                return;
            }

            var relation = new AbRelation();
            var dependencies = _abManifest.GetAllDependencies(abName);
            for (int i = 0; i < dependencies.Length; i++)
            {
                relation.AddDependence(dependencies[i]);
                await ProcessRelationsByRecursiveAsync(dependencies[i], abName);
            }

            _relations.Add(abHash, relation);
        }

        private async Task ProcessRelationsByRecursiveAsync(string abName, string refAbName)
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);

            if (_relations.ContainsKey(abHash))
            {
                _relations[abHash].AddReference(refAbName);
            }
            else
            {
                var relation = new AbRelation();
                relation.AddReference(refAbName);
                _relations.Add(abHash, relation);
                await ProcessRelationsByRecursiveAsync(abName);
            }
        }

        public async Task UpdateAbAsync(string abName)
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);
            
            if (_loaders.ContainsKey(abHash))
            {
                await _loaders[abHash].UpdateAbAsync();
                return;
            }
            
            await ProcessRelationsAsync(abName);
            
            if (_relations.ContainsKey(abHash))
            {
                var relation = _relations[abHash];
                var dependencies = relation.GetAllDependence();
                for (int i = 0; i < dependencies.Count; i++)
                {
                    await LoadAbAsync(dependencies[i]);
                }

                var loader = AbLoaderManager.Create(abName, abHash);
                await loader.UpdateAbAsync();
                _loaders.Add(abHash, loader);
            }
        }

        public async Task LoadAbAsync(string abName)
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);

            if (_loaders.ContainsKey(abHash))
            {
                await _loaders[abHash].LoadAbAsync();
                return;
            }

            await ProcessRelationsAsync(abName);

            if (_relations.ContainsKey(abHash))
            {
                var relation = _relations[abHash];
                var dependencies = relation.GetAllDependence();
                for (int i = 0; i < dependencies.Count; i++)
                {
                    await LoadAbAsync(dependencies[i]);
                }

                var loader = AbLoaderManager.Create(abName, abHash);
                await loader.LoadAbAsync();
                _loaders.Add(abHash, loader);
            }
        }

        public T LoadAsset<T>(string abName, string assetName, bool isCache) where T : Object
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);
            if (_loaders.ContainsKey(abHash))
            {
                return _loaders[abHash].LoadAsset<T>(assetName, isCache);
            }

            Debug.LogError(
                $"{GetType()}/LoadAsset<T>() can't found the AssetBundle, can't load the asset, plase check it! abName = {abName}, assetName = {assetName}");
            return null;
        }

        public Task<T> LoadAssetAsync<T>(string abName, string assetName, bool isCache) where T : Object
        {
            var abHash = _abManifest.GetAssetBundleHash(abName);
            if (_loaders.ContainsKey(abHash))
            {
                return _loaders[abHash].LoadAssetAsync<T>(assetName, isCache);
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
            }
        }
    }
}