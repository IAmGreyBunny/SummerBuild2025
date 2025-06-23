using UnityEngine;

/// <summary>
/// A simple, robust script that quits the application directly when a button is pressed.
/// It handles quitting correctly in both the Unity Editor and in a built game.
/// </summary>
public class DirectQuitHandler : MonoBehaviour
{
    /// <summary>
    /// This performs the actual quit action.
    /// This method should be called directly by the "Quit" button's OnClick event.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Application is exiting...");

        // If we are running in the Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If we are running in a built application
        Application.Quit();
#endif
    }
}