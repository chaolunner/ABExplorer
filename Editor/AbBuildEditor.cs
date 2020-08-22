using System.IO;
using UnityEditor;
using UnityEngine;
using ABExplorer.Utilities;

namespace ABExplorer.Editor
{
    public class AbBuildEditor
    {
        [MenuItem("ABExplorer/Prepare For Build")]
        public static void PrepareForBuild()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AbResources.GetAllPaths(out var absPaths, out _);
            foreach (var rootPath in absPaths)
            {
                var rootInfo = new DirectoryInfo(rootPath);
                var sceneInfos = rootInfo.GetDirectories();

                foreach (var sceneInfo in sceneInfos)
                {
                    SetAbLabelsByRecursive(sceneInfo, sceneInfo.Name);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Prepare for build completed!");
        }

        private static void SetAbLabelsByRecursive(FileSystemInfo fileSystemInfo, string sceneName)
        {
            if (!fileSystemInfo.Exists)
            {
                Debug.LogError("Directory or file: " + fileSystemInfo + " don't exists, please check it!");
                return;
            }

            var directoryInfo = fileSystemInfo as DirectoryInfo;
            var fileSystemInfos = directoryInfo.GetFileSystemInfos();
            foreach (var info in fileSystemInfos)
            {
                var fileInfo = info as FileInfo;
                if (fileInfo != null)
                {
                    SetFileAbLabel(fileInfo, sceneName);
                }
                else
                {
                    SetAbLabelsByRecursive(info, sceneName);
                }
            }
        }

        private static void SetFileAbLabel(FileInfo fileInfo, string sceneName)
        {
            if (fileInfo.Extension == ".meta")
            {
                return;
            }

            var assetBundleName = GetAbName(fileInfo, sceneName);
            var fullName = fileInfo.FullName.Replace('\\', '/');
            var relativePathIndex = fullName.IndexOf("Assets");
            var relativePath = fullName.Substring(relativePathIndex);
            var assetImporter = AssetImporter.GetAtPath(relativePath);
            assetImporter.assetBundleName = assetBundleName;

            if (fileInfo.Extension == ".unity")
            {
                assetImporter.assetBundleVariant = "u3d";
            }
            else
            {
                assetImporter.assetBundleVariant = "ab";
            }
        }

        private static string GetAbName(FileInfo fileInfo, string sceneName)
        {
            var fullName = fileInfo.FullName.Replace('\\', '/');
            var position = fullName.LastIndexOf(sceneName) + sceneName.Length + 1;
            var path = fullName.Substring(position);
            var index = path.IndexOf('/');
            if (index == -1)
            {
                return sceneName + "/" + sceneName;
            }

            return sceneName + "/" + path.Substring(0, index);
        }

        [MenuItem("ABExplorer/Build AssetBundles")]
        public static void BuildAssetBundles()
        {
            BuildAssetBundles(BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }

        private static void BuildAssetBundles(BuildAssetBundleOptions options, BuildTarget target)
        {
            var outPath = PathUtility.GetAbOutPath();
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            BuildPipeline.BuildAssetBundles(outPath, options, target);
            AssetDatabase.Refresh();
            Debug.Log("Build AssetBundles has completed!");
        }

        [MenuItem("ABExplorer/Clear Cache")]
        public static void ClearCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("Successfully cleaned the cache.");
            }
            else
            {
                Debug.Log("Cache is being used.");
            }

            var cachePath = PathUtility.GetAbCachePath();
            if (!string.IsNullOrEmpty(cachePath) && Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
                File.Delete(cachePath + ".meta");
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("ABExplorer/Delete AssetBundles")]
        public static void DeleteAssetBundles()
        {
            var outPath = PathUtility.GetAbOutPath();
            if (!string.IsNullOrEmpty(outPath) && Directory.Exists(outPath))
            {
                Directory.Delete(outPath, true);
                File.Delete(outPath + ".meta");
                AssetDatabase.Refresh();
            }

            Debug.Log("Delete all AssetBundles has completed!");
        }
    }
}