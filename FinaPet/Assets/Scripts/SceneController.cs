using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance; // Singleton for persistence

    // A private class to store more info about our scene history
    private class SceneHistoryEntry
    {
        public string SceneName { get; }
        public LoadSceneMode LoadMode { get; }

        public SceneHistoryEntry(string sceneName, LoadSceneMode loadMode)
        {
            SceneName = sceneName;
            LoadMode = loadMode;
        }
    }

    private List<SceneHistoryEntry> sceneHistory = new List<SceneHistoryEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loads a new scene, replacing all current scenes.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        // Add the current scene to history before loading the new one
        var activeScene = SceneManager.GetActiveScene();
        sceneHistory.Add(new SceneHistoryEntry(activeScene.name, LoadSceneMode.Single));
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Loads a scene on top of the current scene(s).
    /// </summary>
    public void LoadSceneAdditive(string sceneName)
    {
        // Additive scenes don't replace the current scene, so we record the new scene itself as the "back" action
        sceneHistory.Add(new SceneHistoryEntry(sceneName, LoadSceneMode.Additive));
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    /// <summary>
    /// The "smart" back button. It either unloads an additive scene or reloads the previous single scene.
    /// </summary>
    public void GoBack()
    {
        if (sceneHistory.Count > 0)
        {
            // Get the last entry from our history
            SceneHistoryEntry previousScene = sceneHistory[sceneHistory.Count - 1];
            sceneHistory.RemoveAt(sceneHistory.Count - 1); // Remove it from the list

            if (previousScene.LoadMode == LoadSceneMode.Additive)
            {
                // If the last action was ADDITIVE, we just need to UNLOAD that scene
                SceneManager.UnloadSceneAsync(previousScene.SceneName);
            }
            else
            {
                // If the last action was a SINGLE load, we load the scene name from the history
                SceneManager.LoadScene(previousScene.SceneName, LoadSceneMode.Single);
            }
        }
        else
        {
            Debug.LogWarning("SCENE HISTORY: No previous scene to go back to.");
            // Optional: Add fallback behavior, like loading a main menu
            // SceneManager.LoadScene("MainMenu");
        }
    }

    // You no longer need a separate UnloadScene method, as GoBack() handles it.
    // However, if you need to unload a specific additive scene by name, you can keep it.
    // public void UnloadScene(string sceneName)
    // {
    //   SceneManager.UnloadSceneAsync(sceneName);
    // }
}