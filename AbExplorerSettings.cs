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

        public string URL => $"http://{address}:{port}/";

        private const string AbExplorerSettingsPath = "Assets/ABExplorer/Resources/ABExplorerSettings.asset";

        private static AbExplorerSettings _instance;

        public static AbExplorerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    Initialize();
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            _instance = Resources.Load<AbExplorerSettings>("ABExplorerSettings");
#if UNITY_EDITOR
            if (_instance == null)
            {
                _instance = CreateInstance<AbExplorerSettings>();
                if (!Directory.Exists(AbExplorerSettingsPath))
                {
                    Directory.CreateDirectory(AbExplorerSettingsPath);
                }

                AssetDatabase.CreateAsset(_instance, AbExplorerSettingsPath);
                AssetDatabase.SaveAssets();
            }
#endif
        }

#if UNITY_EDITOR
        public static SerializedObject ToSerializedObject()
        {
            return new SerializedObject(Instance);
        }
#endif
    }
}