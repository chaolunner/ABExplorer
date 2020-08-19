using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

namespace ABExplorer
{
    public class AbFacade : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return AbResources.DownloadAbAsync();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                OnSceneLoaded(SceneManager.GetSceneAt(i), i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
        }
    }
}