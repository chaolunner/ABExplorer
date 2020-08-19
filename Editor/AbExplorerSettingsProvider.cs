using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ABExplorer.Editor
{
    public static class AbExplorerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Preferences Settings window.
            var provider = new SettingsProvider("Preferences/AB Explorer", SettingsScope.User)
            {
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = AbExplorerSettings.ToSerializedObject();
                    EditorGUILayout.PropertyField(settings.FindProperty("playMode"), new GUIContent("Play Mode"));
                    EditorGUILayout.PropertyField(settings.FindProperty("address"), new GUIContent("Address"));
                    EditorGUILayout.PropertyField(settings.FindProperty("port"), new GUIContent("Port"));
                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] {"Address", "Port"})
            };

            return provider;
        }
    }
}