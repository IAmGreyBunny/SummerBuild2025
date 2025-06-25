using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place this script on the root of your Login Form PREFAB.
/// It handles loading the correct scene after a login attempt.
/// </summary>
public class LoginPrefabController : MonoBehaviour
{
    [Tooltip("The name of the scene to load after a successful LOGIN.")]
    public string sceneToLoadOnLogin = "PlayerDashboard";

    /// <summary>
    /// This public method should be linked to the 'OnClick' event of the
    /// Login Button that is INSIDE this prefab.
    /// </summary>
    public void AttemptLogin()
    {
        // In a real app, you would validate the username/password fields here first.

        if (string.IsNullOrEmpty(sceneToLoadOnLogin))
        {
            Debug.LogError("Scene To Load On Login is not set in the Inspector for the Login Prefab!");
            return;
        }

        Debug.Log("Login button clicked. Loading scene: " + sceneToLoadOnLogin);
        SceneManager.LoadScene(sceneToLoadOnLogin);
    }
}
