using UnityEngine;

/// <summary>
/// A ScriptableObject to define music settings for a specific scene.
/// Create multiple assets of this type for different scenes.
/// </summary>
[CreateAssetMenu(fileName = "SceneMusicConfig", menuName = "Audio/Scene Music Configuration", order = 1)]
public class SceneMusicConfig : ScriptableObject
{
    [Tooltip("The exact name of the Unity scene this configuration applies to.")]
    public string sceneName;

    [Tooltip("The audio clip to play when this scene loads. Leave null to continue previous music, or stop if 'Continue Previous Music If None' is false.")]
    public AudioClip sceneMusicClip;

    [Tooltip("If true, music will start muted when this scene loads.")]
    public bool muteSceneMusicOnLoad = false;

    [Tooltip("If true and 'Scene Music Clip' is null, the music from the previous scene will continue playing. " +
             "If false and 'Scene Music Clip' is null, music will stop when this scene loads.")]
    public bool continuePreviousMusicIfNone = true;

    [Tooltip("If true, the assigned 'Scene Music Clip' will loop. Ignored if 'Scene Music Clip' is null.")]
    public bool loopSceneMusic = true;
}