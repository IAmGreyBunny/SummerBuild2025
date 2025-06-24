using UnityEngine;
using UnityEngine.UI; // Required for the Image component
using System.Collections.Generic; // Required for List

/// <summary>
/// Handles refreshing player data and updating the player's avatar sprite
/// when the Main Menu scene is loaded, using a UI Image component.
/// </summary>
public class PlayerAvatarUpdater : MonoBehaviour
{
    [Header("UI Image Component")]
    [Tooltip("The Image component on this avatar GameObject that will display the player's avatar.")]
    public Image playerAvatarImage; // Changed from SpriteLibrary to Image

    [Tooltip("A list of Sprite assets corresponding to avatar_sprite_id values (0-indexed).")]
    public List<Sprite> avatarSprites; // Changed from List<SpriteLibraryAsset> to List<Sprite>

    [Header("Player ID Source")]
    [Tooltip("The ID of the player to fetch data for. Will use PlayerAuthSession.PlayerId if logged in.")]
    public int debugPlayerId = 1; // Default for testing if not logged in

    // Internal flag to track if the data has been processed by this updater
    private bool _hasProcessedData = false;

    void Start()
    {
        // Sanity check
        if (playerAvatarImage == null) // Check the Image component
        {
            Debug.LogError("PlayerAvatarUpdater: playerAvatarImage (UI Image component) is not assigned on this GameObject!", this);
            return;
        }

        if (avatarSprites == null || avatarSprites.Count == 0) // Check the list of Sprites
        {
            Debug.LogError("PlayerAvatarUpdater: No Avatar Sprites assigned! Cannot update avatar.", this);
            return;
        }

        // Fetch player data when the scene loads
        FetchAndDisplayPlayerData();
    }

    void Update()
    {
        // Check if player data has been freshly loaded by PlayerDataManager
        // and if this updater hasn't processed it yet.
        if (PlayerDataManager.IsDataLoaded && !_hasProcessedData)
        {
            UpdatePlayerAvatarSprite();
            _hasProcessedData = true; // Mark as processed so it doesn't update every frame for the same data
        }
        else if (!string.IsNullOrEmpty(PlayerDataManager.LastErrorMessage))
        {
            // Log any errors from data fetching
            // The error message is reset by PlayerDataManager.FetchPlayerData() at the start of a new fetch.
            Debug.LogError($"PlayerAvatarUpdater: Error fetching player data: {PlayerDataManager.LastErrorMessage}");
            // Optionally, display a UI error message to the user here.
        }
    }

    /// <summary>
    /// Fetches player data and initiates the avatar update process.
    /// </summary>
    private void FetchAndDisplayPlayerData()
    {
        int playerIdToFetch;
        if (PlayerAuthSession.IsLoggedIn)
        {
            playerIdToFetch = PlayerAuthSession.PlayerId;
            Debug.Log($"PlayerAvatarUpdater: Fetching data for logged-in player ID: {playerIdToFetch}");
        }
        else
        {
            playerIdToFetch = debugPlayerId;
            Debug.LogWarning($"PlayerAvatarUpdater: Player not logged in. Using debugPlayerId: {playerIdToFetch}");
        }

        // Reset our internal flag before starting a new fetch
        _hasProcessedData = false;

        // Start the coroutine to fetch player data
        StartCoroutine(PlayerDataManager.FetchPlayerData(playerIdToFetch));
    }


    /// <summary>
    /// Updates the player's avatar sprite based on the fetched PlayerMainData.
    /// </summary>
    private void UpdatePlayerAvatarSprite()
    {
        if (PlayerDataManager.CurrentPlayerMainData == null)
        {
            Debug.LogWarning("PlayerAvatarUpdater: PlayerMainData is null after data load. Cannot update avatar sprite.");
            return;
        }

        int avatarSpriteId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;
        Debug.Log($"PlayerAvatarUpdater: Fetched avatar_sprite_id: {avatarSpriteId}");

        if (avatarSpriteId >= 0 && avatarSpriteId < avatarSprites.Count)
        {
            // Assign the correct Sprite directly to the UI Image component
            playerAvatarImage.sprite = avatarSprites[avatarSpriteId];
            Debug.Log($"PlayerAvatarUpdater: Avatar sprite updated to index {avatarSpriteId} ({avatarSprites[avatarSpriteId].name}).");
        }
        else
        {
            Debug.LogWarning($"PlayerAvatarUpdater: Invalid avatar_sprite_id ({avatarSpriteId}) received. No corresponding sprite found at that index. Setting to default (index 0).");
            // Set a default sprite if the ID is out of bounds, ensure list is not empty
            if (avatarSprites.Count > 0)
            {
                playerAvatarImage.sprite = avatarSprites[0];
            }
            else
            {
                Debug.LogError("PlayerAvatarUpdater: No default avatar sprite available at index 0.");
            }
        }
    }

    /// <summary>
    /// Public method to manually trigger a refresh of player data and avatar update.
    /// Can be hooked up to a UI button if desired.
    /// </summary>
    public void RefreshPlayerAndAvatar()
    {
        Debug.Log("PlayerAvatarUpdater: Manually refreshing player data and avatar.");
        FetchAndDisplayPlayerData();
    }
}