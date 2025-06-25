using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A robust, reusable component that handles updating the player's avatar sprite.
/// It works in two ways:
/// 1. It attempts to update immediately when it becomes active.
/// 2. It also subscribes to the OnProfileDataLoaded event to catch data loads that happen after it's active.
/// This ensures it works in any scene, regardless of data loading timing.
/// </summary>
public class PlayerAvatarUpdater : MonoBehaviour
{
    [Header("UI Image Component")]
    [Tooltip("The Image component on this avatar GameObject that will display the player's avatar.")]
    public Image playerAvatarImage;

    [Tooltip("A list of Sprite assets corresponding to avatar_sprite_id values (0-indexed).")]
    public List<Sprite> avatarSprites;

    /// <summary>
    /// When the component becomes active, it subscribes to the central data loading event
    /// and also attempts an immediate refresh.
    /// </summary>
    void OnEnable()
    {
        // Subscribe to the event. This is crucial for scenes where data is loaded after this script is active.
        ProfilePageDataLoader.OnProfileDataLoaded += RefreshAvatar;
        Debug.Log("PlayerAvatarUpdater enabled and subscribed to OnProfileDataLoaded.");

        // Also attempt an immediate refresh. This handles scenes where data is already loaded (like the Main Menu).
        RefreshAvatar();
    }

    /// <summary>
    /// Unsubscribes from the event when the component is disabled to prevent memory leaks.
    /// </summary>
    void OnDisable()
    {
        ProfilePageDataLoader.OnProfileDataLoaded -= RefreshAvatar;
        Debug.Log("PlayerAvatarUpdater disabled and unsubscribed from OnProfileDataLoaded.");
    }

    /// <summary>
    /// The master refresh method. It checks for loaded data and updates the UI Image accordingly.
    /// This method is safe to call multiple times.
    /// </summary>
    public void RefreshAvatar()
    {
        Debug.Log("PlayerAvatarUpdater: RefreshAvatar() called.");

        // Perform sanity checks first.
        if (playerAvatarImage == null) return;
        if (avatarSprites == null || avatarSprites.Count == 0) return;

        // Check if the required player data is available in our static manager.
        if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
        {
            int avatarSpriteId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;
            Debug.Log($"PlayerAvatarUpdater: Data is loaded. Updating sprite to ID: {avatarSpriteId}.");

            if (avatarSpriteId >= 0 && avatarSpriteId < avatarSprites.Count)
            {
                // Only update the sprite if it's different, to avoid unnecessary work.
                if (playerAvatarImage.sprite != avatarSprites[avatarSpriteId])
                {
                    playerAvatarImage.sprite = avatarSprites[avatarSpriteId];
                    Debug.Log("PlayerAvatarUpdater: Sprite has been updated successfully.");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerAvatarUpdater: Invalid avatar_sprite_id ({avatarSpriteId}) in player data.");
                playerAvatarImage.sprite = null;
            }
        }
        else
        {
            Debug.LogWarning("PlayerAvatarUpdater: Could not refresh avatar because PlayerDataManager has no data loaded yet.");
        }
    }
}