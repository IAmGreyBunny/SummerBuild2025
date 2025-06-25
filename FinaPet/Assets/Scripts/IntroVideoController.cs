using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

/// <summary>
/// A simple, reusable script to load a new scene when a UI Button is clicked.
/// Attach this directly to the Button GameObject.
/// </summary>
public class SceneLoaderButton : MonoBehaviour
{
    [Tooltip("MainMenu")]
    public string sceneNameToLoad;

    /// <summary>
    /// This public method should be linked to the 'OnClick' event of the button.
    /// </summary>
    public void LoadTargetScene()
    {
        // First, check if a scene name has been provided in the Inspector.
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("Scene Name to Load is not set on the button script!");
            return;
        }

        // Load the specified scene.
        Debug.Log("Loading scene: " + sceneNameToLoad);
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
