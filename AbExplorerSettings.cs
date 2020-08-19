#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

namespace ABExplorer
{
    public enum PlayMode
    {
        FastMode,
        VirtualMode,
        PackedPlayMode,
    }

    public class AbExplorerSettings : ScriptableObject
    {
        public PlayMode playMode;
        public string address = "localhost";
        public int port = 4747;

        public string URL => string.Format("http://{0}:{1}/", address, port);

        private const string AbExplorerSettingsPath = "Assets/ABExplorer/Resources/ABExplorerSettings.asset";

        public static AbExplorerSettings Settings
        {
            get
            {
                var settings = Resources.Load<AbExplorerSettings>("ABExplorerSettings");
#if UNITY_EDITOR
                if (settings == null)
                {
                    settings = CreateInstance<AbExplorerSettings>();
                    if (!Directory.Exists(AbExplorerSettingsPath))
                    {
                        Directory.CreateDirectory(AbExplorerSettingsPath);
                    }

                    AssetDatabase.CreateAsset(settings, AbExplorerSettingsPath);
                    AssetDatabase.SaveAssets();
                }
#endif
                return settings;
            }
        }

#if UNITY_EDITOR
        public static SerializedObject ToSerializedObject()
        {
            return new SerializedObject(Settings);
        }
#endif
    }
}