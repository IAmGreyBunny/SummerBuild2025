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

    // --- NEW: Scene-specific music configurations ---
    [Header("Scene Music Configurations")]
    [Tooltip("Drag all your SceneMusicConfig ScriptableObjects here to apply scene-specific music rules.")]
    public System.Collections.Generic.List<SceneMusicConfig> sceneMusicConfigs;

    [Tooltip("If true, music will stop if no specific SceneMusicConfig is found for the loaded scene. If false, existing music will continue.")]
    public bool stopMusicIfNoConfig = false;


    void Awake()
    {
        // --- Singleton Implementation ---
        // If an instance already exists and it's not this one, destroy this duplicate.
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
            Debug.LogError("MusicController requires an AudioSource component!", this);
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set initial AudioSource properties based on Inspector settings
        audioSource.loop = loopMusic;
        audioSource.volume = masterVolume;
        _lastKnownVolume = masterVolume; // Initialize _lastKnownVolume

        // Play default music if assigned and not already playing (only if starting in a scene without specific config)
        // This will be overridden by OnSceneLoaded if the first scene has a config.
        if (defaultMusic != null && !audioSource.isPlaying)
        {
            audioSource.clip = defaultMusic;
            audioSource.Play();
            Debug.Log("Playing default music in Awake (initial load)!");
        }

        // Subscribe to scene changes so we can react when a new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// This method is called whenever a new scene is loaded.
    /// It applies scene-specific music configurations.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"MusicController: Scene loaded: {scene.name}");
        SceneMusicConfig currentSceneConfig = null;

        // Find configuration for the loaded scene
        if (sceneMusicConfigs != null)
        {
            foreach (var config in sceneMusicConfigs)
            {
                if (config != null && config.sceneName == scene.name)
                {
                    currentSceneConfig = config;
                    break;
                }
            }
        }

        if (currentSceneConfig != null)
        {
            Debug.Log($"MusicController: Applying config for scene: {scene.name}.");

            // Handle music clip change or continuation
            if (currentSceneConfig.sceneMusicClip != null)
            {
                // Play new clip, resetting its time, and apply its looping setting
                PlayMusic(currentSceneConfig.sceneMusicClip, true);
                SetLoop(currentSceneConfig.loopSceneMusic);
            }
            else if (!currentSceneConfig.continuePreviousMusicIfNone)
            {
                // No specific clip AND we should NOT continue previous music, so stop it.
                StopMusic();
            }
            // Else (no specific clip, but continuePreviousMusicIfNone is true), do nothing, current music keeps playing.

            // Handle mute on load
            if (currentSceneConfig.muteSceneMusicOnLoad)
            {
                SetVolume(0f); // Mute the music
            }
            else
            {
                // Ensure volume is restored to its proper level if it was muted by a *previous scene's config* or ToggleMute.
                // We use _lastKnownVolume to restore the last audible volume.
                SetVolume(_lastKnownVolume);
            }
        }
        else
        {
            Debug.Log($"MusicController: No specific music config found for scene: {scene.name}.");
            if (stopMusicIfNoConfig)
            {
                StopMusic();
                Debug.Log("MusicController: Music stopped as no config found and 'stopMusicIfNoConfig' is true.");
            }
            // Else, if no config and stopMusicIfNoConfig is false, current music continues (default behavior)
            // Ensure global loop setting is applied if no scene-specific config overrides it.
            SetLoop(loopMusic); // Re-apply the general loop setting
        }
    }

    /// <summary>
    /// Plays a specific AudioClip.
    /// </summary>
    /// <param name="clipToPlay">The audio clip to play. If null, stops current music.</param>
    /// <param name="resetTime">If true, the music will start from the beginning. If false, it will continue from its current time if the clip is the same.</param>
    public void PlayMusic(AudioClip clipToPlay, bool resetTime = true)
    {
        if (audioSource == null) return;

        // If the same clip is already playing and we don't want to reset, just return.
        if (audioSource.clip == clipToPlay && audioSource.isPlaying && !resetTime)
        {
            return;
        }

        audioSource.Stop(); // Always stop before playing a new or reset clip.

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
    /// Resumes the currently paused music.
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.time > 0)
        {
            audioSource.UnPause();
            Debug.Log("MusicController: Music resumed.");
        }
        else if (audioSource != null && !audioSource.isPlaying && audioSource.time == 0 && audioSource.clip != null)
        {
            // If it was stopped, and has a clip, play it from the start
            audioSource.Play();
            Debug.Log("MusicController: Music started from beginning (was stopped).");
        }
    }

    /// <summary>
    /// Sets the volume of the music.
    /// </summary>
    /// <param name="volume">Volume level (0 to 1).</param>
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
            // Only update _lastKnownVolume if the new volume is not effectively zero.
            // This prevents _lastKnownVolume from becoming zero if SetVolume(0f) is explicitly called for mute.
            if (audioSource.volume > 0.001f)
            {
                _lastKnownVolume = audioSource.volume;
            }
            masterVolume = audioSource.volume; // Keep public masterVolume field updated
            Debug.Log($"MusicController: Music volume set to: {audioSource.volume}");
        }
    }

    /// <summary>
    /// Toggles mute/unmute for the music.
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
                // Restore to the last known audible volume, or fallback to masterVolume if _lastKnownVolume was also zero (e.g., initially muted)
                audioSource.volume = _lastKnownVolume > 0.001f ? _lastKnownVolume : masterVolume;
                // If it was somehow muted and _lastKnownVolume was also 0, default to 0.5f to ensure it plays.
                if (audioSource.volume <= 0.001f) audioSource.volume = 0.5f;
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
            loopMusic = loop; // Keep public loopMusic field updated
            Debug.Log($"MusicController: Music looping set to: {loop}");
        }
    }

    // Clean up when the object is destroyed
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null; // Clear the singleton instance
        }
        // Unsubscribe from sceneLoaded to prevent potential memory leaks or issues with disabled objects
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}