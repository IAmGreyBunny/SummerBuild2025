using UnityEngine;

// A versatile handler for UI buttons that load a specific scene.
public class SceneChangeButtonHandler : MonoBehaviour
{
    [Tooltip("The name of the scene you want to load.")]
    public string sceneNameToLoad;

    [Tooltip("If checked, the scene will be loaded additively on top of the current scene.")]
    public bool isAdditive = false;

    /// <summary>
    /// This public method should be linked to the button's OnClick event.
    /// </summary>
    public void PerformSceneChange()
    {
        // First, check if a scene name has been provided in the Inspector.
        if (string.IsNullOrEmpty(sceneNameToLoad))
        {
            Debug.LogError("SceneNameToLoad is not set on the button handler. Please specify a scene name in the Inspector.");
            return;
        }

        // Check if the SceneController instance exists.
        if (SceneController.Instance != null)
        {
            // Decide whether to load the scene additively or normally based on the 'isAdditive' flag.
            if (isAdditive)
            {
                SceneController.Instance.LoadSceneAdditive(sceneNameToLoad);
            }
            else
            {
                SceneController.Instance.LoadScene(sceneNameToLoad);
            }
        }
        else
        {
            Debug.LogError("SceneController.Instance not found. Make sure your SceneController is in the initial scene.");
        }
    }
}