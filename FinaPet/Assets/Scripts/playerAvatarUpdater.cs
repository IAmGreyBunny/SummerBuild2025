using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Handles updating the player's avatar sprite on a UI Image component.
/// This script LISTENS for data to be loaded before updating the avatar.
/// </summary>
public class PlayerAvatarUpdater : MonoBehaviour
{
    [Header("UI Image Component")]
    public Image playerAvatarImage;
    public List<Sprite> avatarSprites;

    void OnEnable()
    {
        // Subscribe to the event.
        ProfilePageDataLoader.OnProfileDataLoaded += RefreshAvatar;
        Debug.Log("PlayerAvatarUpdater enabled and subscribed to OnProfileDataLoaded.");
    }

    void OnDisable()
    {
        // Unsubscribe from the event.
        ProfilePageDataLoader.OnProfileDataLoaded -= RefreshAvatar;
        Debug.Log("PlayerAvatarUpdater disabled and unsubscribed from OnProfileDataLoaded.");
    }

    /// <summary>
    /// This method is called by the event from ProfilePageDataLoader
    /// and updates the avatar sprite based on the loaded data.
    /// </summary>
    public void RefreshAvatar()
    {
        Debug.Log("PlayerAvatarUpdater: Received OnProfileDataLoaded event. Refreshing avatar sprite.");
        if (playerAvatarImage == null) return;

        if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
        {
            int avatarSpriteId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;

            if (avatarSpriteId >= 0 && avatarSpriteId < avatarSprites.Count)
            {
                playerAvatarImage.sprite = avatarSprites[avatarSpriteId];
                Debug.Log($"PlayerAvatarUpdater: Avatar sprite updated to index {avatarSpriteId}.");
            }
            else
            {
                Debug.LogWarning($"PlayerAvatarUpdater: Invalid avatar_sprite_id ({avatarSpriteId}). No sprite assigned at that index.");
                playerAvatarImage.sprite = null; // Or set a default "missing" sprite
            }
        }
        else
        {
            Debug.LogError("PlayerAvatarUpdater: RefreshAvatar called, but no player data was loaded.");
            playerAvatarImage.sprite = null; // Clear the avatar if data is missing
        }
    }
}