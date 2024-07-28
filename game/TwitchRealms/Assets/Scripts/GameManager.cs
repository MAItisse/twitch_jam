using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //public Slider progressBar; // Reference to a UI slider to show progress

    public void LoadScene(string sceneToLoad)
    {
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneToLoad)
    {
        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // While the scene is still loading
        while (!operation.isDone)
        {
            // Optionally, update the progress bar
            /*if (progressBar != null)
            {
                progressBar.value = operation.progress;
            }*/

            yield return null;
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a standalone build
        Application.Quit();
#endif
    }
}
