using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

/// <summary>
/// Manages background music playback, ensuring persistence across scenes.
/// Implements a Singleton pattern to guarantee only one instance exists.
/// </summary>
[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource component is always present
public class MusicController : MonoBehaviour
{
    // Singleton instance to ensure only one MusicController exists
    public static MusicController Instance { get; private set; }

    [Header("Audio Settings")]
    [Tooltip("The AudioSource component on this GameObject.")]
    private AudioSource audioSource;

    [Tooltip("Default background music to play when the controller starts and no scene-specific config applies.")]
    public AudioClip defaultMusic;

    [Tooltip("Set to true if the music should loop by default. This is overridden by SceneMusicConfig if specified.")]
    public bool loopMusic = true;

    [Range(0f, 1f)]
    [Tooltip("Master volume for the music.")]
    public float masterVolume = 0.5f;

    // This stores the last known audible volume before a potential mute.
    // It's used internally to restore volume after muting/unmuting.
    private float _lastKnownVolume = 0.5f;

    // New Public Property to check mute status
    public bool IsMuted => audioSource != null && audioSource.volume <= 0.001f;

    // --- Scene-specific music configurations ---
    [Header("Scene Music Configurations")]
    [Tooltip("Drag all your SceneMusicConfig ScriptableObjects here to apply scene-specific music rules.")]
    public System.Collections.Generic.List<SceneMusicConfig> sceneMusicConfigs;

    [Tooltip("If true, music will stop if no specific SceneMusicConfig is found for the loaded scene. If false, existing music will continue.")]
    public bool stopMusicIfNoConfig = false;


    void Awake()
    {
        // --- Singleton Implementation ---
        // If an instance already exists and it's not this one, destroy this duplicate.
        // This ensures only one MusicController persists across scene loads.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, set this as the singleton instance.
        Instance = this;

        // Ensure this GameObject persists across scene loads.
        DontDestroyOnLoad(gameObject);

        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("MusicController requires an AudioSource component! Adding one automatically.", this);
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set initial AudioSource properties based on Inspector settings
        audioSource.loop = loopMusic;
        audioSource.volume = masterVolume;
        _lastKnownVolume = masterVolume; // Initialize _lastKnownVolume with the master volume

        // Play default music if assigned and not already playing.
        // This serves as a fallback or initial music if no scene-specific config
        // is found for the very first loaded scene.
        if (defaultMusic != null && !audioSource.isPlaying)
        {
            audioSource.clip = defaultMusic;
            audioSource.Play();
            Debug.Log("MusicController: Playing default music in Awake (initial load).");
        }

        // Subscribe to scene changes so we can react when a new scene loads.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// This method is called whenever a new scene is loaded.
    /// It applies scene-specific music configurations based on the `sceneMusicConfigs` list.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicController: Scene loaded: {scene.name}");
        SceneMusicConfig currentSceneConfig = null;

        // Attempt to find a configuration for the newly loaded scene.
        if (sceneMusicConfigs != null)
        {
            foreach (var config in sceneMusicConfigs)
            {
                if (config != null && config.sceneName == scene.name)
                {
                    currentSceneConfig = config;
                    break; // Found a config for this scene
                }
            }
        }

        if (currentSceneConfig != null)
        {
            Debug.Log($"MusicController: Applying config for scene: {scene.name}.");

            // Handle music clip change or continuation based on the config.
            if (currentSceneConfig.sceneMusicClip != null)
            {
                // CRUCIAL CHANGE: Only play (and potentially reset) the music if it's a different clip
                // or if the same clip is not currently playing. This prevents restarting music
                // that is already correctly playing for the scene.
                if (audioSource.clip != currentSceneConfig.sceneMusicClip || !audioSource.isPlaying)
                {
                    PlayMusic(currentSceneConfig.sceneMusicClip, true); // Play the new clip, resetting its time
                    Debug.Log($"MusicController: Switched to music: {currentSceneConfig.sceneMusicClip.name} for {scene.name}.");
                }
                else
                {
                    // The correct music is already playing and should continue seamlessly.
                    Debug.Log($"MusicController: Music '{currentSceneConfig.sceneMusicClip.name}' already playing in '{scene.name}', continuing.");
                }
                // Always apply the loop setting specified in the scene's config.
                SetLoop(currentSceneConfig.loopSceneMusic);
            }
            else if (!currentSceneConfig.continuePreviousMusicIfNone)
            {
                // If the config specifies no specific clip AND also says NOT to continue previous music, stop it.
                StopMusic();
                Debug.Log($"MusicController: No specific clip for '{scene.name}' and 'continuePreviousMusicIfNone' is false, music stopped.");
            }
            // Else (if currentSceneConfig.sceneMusicClip is null but continuePreviousMusicIfNone is true),
            // the current music will simply continue playing without change, which is the desired behavior.

            // Handle mute/volume restoration based on the scene's config.
            if (currentSceneConfig.muteSceneMusicOnLoad)
            {
                SetVolume(0f); // Mute the music if specified in the config
                Debug.Log($"MusicController: Music muted on load for '{scene.name}'.");
            }
            else
            {
                // If not muted, restore the volume.
                // Prioritize _lastKnownVolume (what it was before any mute),
                // then masterVolume (default), and finally a safe fallback.
                float targetVolume = _lastKnownVolume > 0.001f ? _lastKnownVolume : masterVolume;
                // If masterVolume itself is effectively zero, ensure a playable volume if there's music
                if (targetVolume <= 0.001f && (audioSource.clip != null || defaultMusic != null))
                {
                    targetVolume = 0.5f; // Fallback to a sensible default if all stored volumes are zero
                }
                SetVolume(targetVolume);
                Debug.Log($"MusicController: Volume restored for '{scene.name}' to {targetVolume}.");
            }
        }
        else
        {
            // No specific music config found for the loaded scene.
            Debug.Log($"MusicController: No specific music config found for scene: {scene.name}.");
            if (stopMusicIfNoConfig)
            {
                StopMusic();
                Debug.Log("MusicController: Music stopped as no config found and 'stopMusicIfNoConfig' is true.");
            }
            // If stopMusicIfNoConfig is false, existing music will continue.
            // In this case, ensure the global loop setting is applied.
            SetLoop(loopMusic);
            Debug.Log($"MusicController: Applied default loop setting ({loopMusic}) as no scene config was found for '{scene.name}'.");
        }
    }

    /// <summary>
    /// Plays a specific AudioClip.
    /// If the same clip is already playing and `resetTime` is false, it continues.
    /// </summary>
    /// <param name="clipToPlay">The audio clip to play. If null, stops current music.</param>
    /// <param name="resetTime">If true, the music will start from the beginning. If false, it will continue from its current time if the clip is the same.</param>
    public void PlayMusic(AudioClip clipToPlay, bool resetTime = true)
    {
        if (audioSource == null) return;

        // If the same clip is already playing and we don't want to reset, just return.
        if (audioSource.clip == clipToPlay && audioSource.isPlaying && !resetTime)
        {
            Debug.Log($"MusicController: '{clipToPlay?.name}' is already playing and not resetting.");
            return;
        }

        // Always stop before playing a new or reset clip to ensure a clean start.
        audioSource.Stop();

        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
            Debug.Log($"MusicController: Playing music: {clipToPlay.name}");
        }
        else
        {
            Debug.Log("MusicController: Music stopped (clipToPlay was null).");
        }
    }

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("MusicController: Music stopped.");
        }
    }

    /// <summary>
    /// Pauses the currently playing music.
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("MusicController: Music paused.");
        }
    }

    /// <summary>
    /// Resumes the currently paused music. If music was stopped but has a clip, it will play from the beginning.
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying && audioSource.time > 0)
            {
                // If paused, unpause.
                audioSource.UnPause();
                Debug.Log("MusicController: Music resumed.");
            }
            else if (!audioSource.isPlaying && audioSource.time == 0 && audioSource.clip != null)
            {
                // If stopped (time is 0) but has a clip, play it from the start.
                audioSource.Play();
                Debug.Log("MusicController: Music started from beginning (was stopped).");
            }
        }
    }

    /// <summary>
    /// Sets the volume of the music. Also updates `_lastKnownVolume` if the new volume is audible.
    /// </summary>
    /// <param name="volume">Volume level (0 to 1).</param>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
            // Only update _lastKnownVolume if the new volume is not effectively zero.
            // This preserves the last audible volume when muting (SetVolume(0f)).
            if (audioSource.volume > 0.001f)
            {
                _lastKnownVolume = audioSource.volume;
            }
            masterVolume = audioSource.volume; // Keep public masterVolume field updated for Inspector
            Debug.Log($"MusicController: Music volume set to: {audioSource.volume}");
        }
    }

    /// <summary>
    /// Toggles mute/unmute for the music.
    /// When unmuting, it restores to the `_lastKnownVolume` or `masterVolume`.
    /// </summary>
    public void ToggleMute()
    {
        if (audioSource != null)
        {
            if (audioSource.volume > 0.001f) // Currently audible, so mute
            {
                _lastKnownVolume = audioSource.volume; // Store current volume before muting
                audioSource.volume = 0f;
                Debug.Log("MusicController: Music muted.");
            }
            else // Currently muted, so unmute
            {
                // Restore to the last known audible volume, or fallback to masterVolume.
                // Ensures it doesn't stay muted if _lastKnownVolume was somehow 0.
                float restoredVolume = _lastKnownVolume > 0.001f ? _lastKnownVolume : masterVolume;
                if (restoredVolume <= 0.001f) restoredVolume = 0.5f; // Absolute fallback if both are zero
                audioSource.volume = restoredVolume;
                Debug.Log("MusicController: Music unmuted. Volume restored to: " + audioSource.volume);
            }
        }
    }

    /// <summary>
    /// Sets whether the music should loop.
    /// </summary>
    /// <param name="loop">True to loop, false otherwise.</param>
    public void SetLoop(bool loop)
    {
        if (audioSource != null)
        {
            audioSource.loop = loop;
            loopMusic = loop; // Keep public loopMusic field updated for Inspector
            Debug.Log($"MusicController: Music looping set to: {loop}");
        }
    }

    // Clean up when the object is destroyed to prevent memory leaks, especially with singletons.
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null; // Clear the singleton instance
        }
        // Unsubscribe from sceneLoaded to prevent potential memory leaks or issues with disabled objects.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
