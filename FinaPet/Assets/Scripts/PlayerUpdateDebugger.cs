using UnityEngine;
using System.Collections.Generic; // Required for List

/// <summary>
/// Debug script to test the player data update functionality via the PlayerDataManager.
/// Attach this to any GameObject in your scene (e.g., an empty "Debugger" object).
/// </summary>
public class PlayerUpdateDebugger : MonoBehaviour
{
    [Header("Player Data to Update")]
    [Tooltip("The ID of the player whose data to update.")]
    public int targetPlayerId = 1;

    [Tooltip("New coin value to send.")]
    public int newCoinValue = 10;

    [Tooltip("New avatar sprite ID to send (0 or 1).")]
    [Range(0, 1)] // Assuming only 0 and 1 are valid avatar sprite IDs
    public int newAvatarSpriteId = 1;

    [Header("Live Debug Output")]
    public string fetchStatus = "Not fetched";
    public string updateStatus = "No update sent";

    // Update is called once per frame
    void Update()
    {
        // Display current fetch status from PlayerDataManager
        if (PlayerDataManager.IsDataLoaded)
        {
            fetchStatus = $"Data Loaded: ID {PlayerDataManager.CurrentPlayerMainData.player_id}, Coins {PlayerDataManager.CurrentPlayerMainData.coin}, Avatar {PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id}";
        }
        else if (!string.IsNullOrEmpty(PlayerDataManager.LastErrorMessage))
        {
            fetchStatus = $"Fetch Error: {PlayerDataManager.LastErrorMessage}";
        }
        else
        {
            fetchStatus = "Fetching data...";
        }

        // Display current update status from PlayerDataManager
        if (PlayerDataManager.IsUpdateSuccessful)
        {
            updateStatus = $"Update Success: {PlayerDataManager.LastUpdateMessage}";
        }
        else if (!string.IsNullOrEmpty(PlayerDataManager.LastUpdateMessage))
        {
            updateStatus = $"Update Failed: {PlayerDataManager.LastUpdateMessage}";
        }
        else
        {
            updateStatus = "Waiting for update...";
        }
    }

    /// <summary>
    /// Button callback to trigger fetching player data.
    /// </summary>
    [ContextMenu("Fetch Player Data")] // Allows calling from Inspector right-click
    public void TriggerFetchPlayerData()
    {
        Debug.Log("Attempting to fetch player data for debug.");
        // Use PlayerAuthSession.PlayerId if available, otherwise fallback to debugPlayerId
        int playerId = PlayerAuthSession.IsLoggedIn ? PlayerAuthSession.PlayerId : targetPlayerId;
        StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));
    }

    /// <summary>
    /// Button callback to trigger updating player data.
    /// </summary>
    [ContextMenu("Send Player Data Update")] // Allows calling from Inspector right-click
    public void TriggerUpdatePlayerData()
    {
        Debug.Log($"Attempting to send player data update: ID={targetPlayerId}, Coins={newCoinValue}, Avatar={newAvatarSpriteId}");
        StartCoroutine(PlayerDataManager.UpdatePlayerDataOnServer(targetPlayerId, newCoinValue, newAvatarSpriteId));
    }
}