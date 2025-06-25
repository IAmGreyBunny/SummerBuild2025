using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place this script on the root of your Registration Form PREFAB.
/// It handles loading the correct scene after a registration attempt.
/// </summary>
public class RegisterPrefabController : MonoBehaviour
{
    [Tooltip("The name of the scene to load after a successful REGISTRATION.")]
    public string sceneToLoadOnRegister = "NewUserTutorial";

    /// <summary>
    /// This public method should be linked to the 'OnClick' event of the
    /// Register Button that is INSIDE this prefab.
    /// </summary>
    public void AttemptRegistration()
    {
        // In a real app, you would validate the input fields and create a new user here.

        if (string.IsNullOrEmpty(sceneToLoadOnRegister))
        {
            Debug.LogError("Scene To Load On Register is not set in the Inspector for the Register Prefab!");
            return;
        }

        Debug.Log("Register button clicked. Loading scene: " + sceneToLoadOnRegister);
        SceneManager.LoadScene(sceneToLoadOnRegister);
    }
}
