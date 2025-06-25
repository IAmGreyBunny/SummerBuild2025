using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple helper script that holds a scene name and can be told to load it.
/// Attach this to the same GameObject as your LoginFormController.
/// </summary>
public class SceneLoaderHelper : MonoBehaviour
{
    [Tooltip("The name of the scene to load after a successful action.")]
    public string sceneNameToLoad = "MainMenu";

    /// <summary>
    /// Loads the scene specified in the 'sceneNameToLoad' variable.
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("Scene Name to Load is not set in the SceneLoaderHelper script!");
            return;
        }

        Debug.Log("Helper is loading scene: " + sceneNameToLoad);
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
