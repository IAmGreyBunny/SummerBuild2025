using UnityEngine;
using UnityEngine.UI; // Required for Toggle, Image
using System.Collections; // Required for Coroutines
using System.Collections.Generic; // Required for List

/// <summary>
/// Manages male/female avatar selection toggles on the profile page.
/// Updates the database with the chosen avatar_sprite_id and refreshes the displayed avatar.
/// Makes the currently selected gender toggle uninteractable immediately on click.
/// </summary>
public class ProfileAvatarSelector : MonoBehaviour
{
    [Header("UI Toggles")]
    [Tooltip("Toggle for the Female avatar (corresponds to avatar_sprite_id 0).")]
    public Toggle femaleToggle;
    [Tooltip("Toggle for the Male avatar (corresponds to avatar_sprite_id 1).")]
    public Toggle maleToggle;

    [Header("Dependencies")]
    [Tooltip("Reference to the PlayerAvatarUpdater script on your avatar image GameObject.")]
    public PlayerAvatarUpdater playerAvatarUpdater;

    [Header("Debug Settings")]
    [Tooltip("The ID of the player to use when PlayerAuthSession indicates no login.")]
    public int debugPlayerId = 1; // Default for testing if not logged in


    // A flag to prevent toggle listeners from triggering updates recursively
    // when their 'isOn' state is changed programmatically by this script.
    private bool _isUpdatingTogglesInternally = false;


    void Start()
    {
        // Sanity checks
        if (femaleToggle == null || maleToggle == null)
        {
            Debug.LogError("ProfileAvatarSelector: Toggles (Female/Male) are not assigned! Please assign them in the Inspector.", this);
            return;
        }
        if (playerAvatarUpdater == null)
        {
            Debug.LogError("ProfileAvatarSelector: Player Avatar Updater is not assigned! Please assign the component from your avatar image GameObject.", this);
            return;
        }

        // Add listeners to the toggles. We use a single handler for both.
        femaleToggle.onValueChanged.AddListener(OnFemaleToggleChanged);
        maleToggle.onValueChanged.AddListener(OnMaleToggleChanged);

        // Fetch player data and update UI when the scene loads
        RefreshProfileDataAndUI();
    }

    void OnDestroy()
    {
        // Remove listeners to prevent memory leaks when the object is destroyed
        if (femaleToggle != null) femaleToggle.onValueChanged.RemoveListener(OnFemaleToggleChanged);
        if (maleToggle != null) maleToggle.onValueChanged.RemoveListener(OnMaleToggleChanged);
    }

    /// <summary>
    /// Called when the female toggle's value changes.
    /// </summary>
    /// <param name="isOn">True if the toggle is on, false otherwise.</param>
    private void OnFemaleToggleChanged(bool isOn)
    {
        if (_isUpdatingTogglesInternally) return; // Prevent recursive calls when script sets 'isOn'

        if (isOn) // Female is selected
        {
            UpdateAvatarAndDatabase(0); // 0 for female
        }
        else // Female is unselected
        {
            // If female is turned OFF, but male is also OFF (e.g., direct manipulation),
            // force male ON to maintain selection within the toggle group.
            if (!maleToggle.isOn)
            {
                _isUpdatingTogglesInternally = true; // Temporarily disable listener for male toggle
                maleToggle.isOn = true;
                _isUpdatingTogglesInternally = false; // Re-enable listener
            }
        }
    }

    /// <summary>
    /// Called when the male toggle's value changes.
    /// </summary>
    /// <param name="isOn">True if the toggle is on, false otherwise.</param>
    private void OnMaleToggleChanged(bool isOn)
    {
        if (_isUpdatingTogglesInternally) return; // Prevent recursive calls when script sets 'isOn'

        if (isOn) // Male is selected
        {
            UpdateAvatarAndDatabase(1); // 1 for male
        }
        else // Male is unselected
        {
            // If male is turned OFF, but female is also OFF,
            // force female ON to maintain selection within the toggle group.
            if (!femaleToggle.isOn)
            {
                _isUpdatingTogglesInternally = true; // Temporarily disable listener for female toggle
                femaleToggle.isOn = true;
                _isUpdatingTogglesInternally = false; // Re-enable listener
            }
        }
    }


    /// <summary>
    /// Initiates the process of updating the player's avatar_sprite_id in the database
    /// and then refreshes the displayed avatar on the UI.
    /// </summary>
    /// <param name="newAvatarSpriteId">The new avatar sprite ID (0 for female, 1 for male).</param>
    private void UpdateAvatarAndDatabase(int newAvatarSpriteId)
    {
        int playerIdToUse;
        int currentCoinValue = 0; // Default or fallback coin value for update request

        // Determine which player ID to use for the update
        if (PlayerAuthSession.IsLoggedIn)
        {
            playerIdToUse = PlayerAuthSession.PlayerId;
            if (PlayerDataManager.CurrentPlayerMainData == null)
            {
                Debug.LogError("ProfileAvatarSelector: Player logged in but CurrentPlayerMainData is null! Cannot get current coin value for update.");
                return; // Prevent update if essential data is missing
            }
            currentCoinValue = PlayerDataManager.CurrentPlayerMainData.coin;
        }
        else // Not logged in, use debug mode
        {
            playerIdToUse = debugPlayerId;
            Debug.LogWarning($"ProfileAvatarSelector: Player not logged in. Performing debug update for ID: {playerIdToUse}.");

            // Attempt to get the current coin value from already loaded debug data, if any
            if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null && PlayerDataManager.CurrentPlayerMainData.player_id == playerIdToUse)
            {
                currentCoinValue = PlayerDataManager.CurrentPlayerMainData.coin;
                Debug.Log($"ProfileAvatarSelector: Using loaded coin value for debug update: {currentCoinValue}");
            }
            else
            {
                Debug.LogWarning("ProfileAvatarSelector: Debug update initiated with default coin value (0) as no existing data was loaded for this ID.");
            }
        }

        // Only proceed if the selected avatar ID is different from what's currently displayed/intended
        // If PlayerDataManager.CurrentPlayerMainData is null, assume it's different to force an update.
        if (PlayerDataManager.CurrentPlayerMainData == null || PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id != newAvatarSpriteId)
        {
            Debug.Log($"ProfileAvatarSelector: Initiating update to avatar_sprite_id: {newAvatarSpriteId} for player ID: {playerIdToUse}");

            // Start the chained coroutine for update, then fetch, then UI update
            StartCoroutine(Co_UpdateDataAndRefreshUI(playerIdToUse, currentCoinValue, newAvatarSpriteId));
        }
        else
        {
            Debug.Log("ProfileAvatarSelector: Avatar ID is already the same as selected. No update sent.");
            // Even if no update is sent, ensure toggles are correctly set (e.g., if user clicked the already selected one)
            SetTogglesFromAvatarId(newAvatarSpriteId);
        }
    }

    /// <summary>
    /// Coroutine that chains the player data update, followed by a data fetch,
    /// and finally updates the UI based on confirmed data.
    /// </summary>
    private IEnumerator Co_UpdateDataAndRefreshUI(int playerId, int coinValue, int newAvatarSpriteId)
    {
        // Step 1: Optimistically update UI first (immediate feedback)
        // This makes the toggle immediately uninteractable and changes the avatar image
        SetTogglesFromAvatarId(newAvatarSpriteId);
        playerAvatarUpdater.playerAvatarImage.sprite = playerAvatarUpdater.avatarSprites[newAvatarSpriteId];
        Debug.Log("ProfileAvatarSelector: Optimistically updated UI toggles and avatar image.");

        // Step 2: Send the update request to the server
        yield return StartCoroutine(PlayerDataManager.UpdatePlayerDataOnServer(playerId, coinValue, newAvatarSpriteId));

        if (PlayerDataManager.IsUpdateSuccessful)
        {
            Debug.Log("ProfileAvatarSelector: Server update reported success. Now re-fetching data to confirm.");
            // Step 3: Server update successful, now fetch the latest data from the server
            yield return StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));

            if (PlayerDataManager.IsDataLoaded)
            {
                Debug.Log("ProfileAvatarSelector: Player data successfully re-fetched after update. Updating UI based on server data.");
                // Step 4: Data re-fetched, now update UI based on confirmed server data
                SetTogglesFromAvatarId(PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id);
                playerAvatarUpdater.playerAvatarImage.sprite = playerAvatarUpdater.avatarSprites[PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id];
            }
            else
            {
                Debug.LogError($"ProfileAvatarSelector: Failed to re-fetch player data after update: {PlayerDataManager.LastErrorMessage}");
                // Handle error: maybe revert UI or show error message
                // For now, it will keep the optimistic update but log the issue.
            }
        }
        else
        {
            Debug.LogError($"ProfileAvatarSelector: Player data update failed on server: {PlayerDataManager.LastUpdateMessage}");
            // Handle update failure: maybe revert UI to original state or show error to user
            // For now, it will keep the optimistic update but log the issue.
            // A more robust solution might re-fetch original data and revert UI:
            // yield return StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));
            // if (PlayerDataManager.IsDataLoaded) SetTogglesFromAvatarId(PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id);
        }
    }


    /// <summary>
    /// Fetches player data and updates the UI (toggles and avatar image).
    /// Called on scene load and after data updates.
    /// </summary>
    public void RefreshProfileDataAndUI()
    {
        int playerIdToFetch;
        if (PlayerAuthSession.IsLoggedIn)
        {
            playerIdToFetch = PlayerAuthSession.PlayerId;
        }
        else
        {
            playerIdToFetch = debugPlayerId; // Use debug ID if not logged in
            Debug.LogWarning($"ProfileAvatarSelector: Player not logged in. Using debugPlayerId: {playerIdToFetch}");
        }

        Debug.Log($"ProfileAvatarSelector: Fetching player data for UI update for ID: {playerIdToFetch}");

        StartCoroutine(Co_WaitForPlayerDataThenUpdateUI(playerIdToFetch));
    }

    /// <summary>
    /// Coroutine to wait for PlayerDataManager to fetch data, then update the UI.
    /// This is used for initial load or manual refresh, not after an update action.
    /// </summary>
    private IEnumerator Co_WaitForPlayerDataThenUpdateUI(int playerId)
    {
        // First, explicitly request a data fetch
        yield return StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));

        // Now, check if data was successfully loaded
        if (PlayerDataManager.IsDataLoaded)
        {
            Debug.Log("ProfileAvatarSelector: Player data successfully fetched for initial UI setup.");
            SetTogglesFromAvatarId(PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id);
            // Also update the avatar image based on the fetched data
            playerAvatarUpdater.playerAvatarImage.sprite = playerAvatarUpdater.avatarSprites[PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id];
        }
        else
        {
            Debug.LogError($"ProfileAvatarSelector: Failed to fetch player data for UI setup: {PlayerDataManager.LastErrorMessage}");
            // Optionally, set toggles to a default or disabled state if data cannot be loaded
            _isUpdatingTogglesInternally = true;
            femaleToggle.isOn = false;
            maleToggle.isOn = false;
            femaleToggle.interactable = false; // Disable toggles on fetch error
            maleToggle.interactable = false;
            _isUpdatingTogglesInternally = false;

            // Also set avatar image to null or a "missing" sprite if data couldn't load
            playerAvatarUpdater.playerAvatarImage.sprite = null;
        }
    }

    /// <summary>
    /// Sets the UI toggles based on the provided avatar sprite ID.
    /// Makes the currently selected toggle uninteractable and the other interactable.
    /// </summary>
    /// <param name="avatarId">The avatar_sprite_id (0 for female, 1 for male).</param>
    private void SetTogglesFromAvatarId(int avatarId)
    {
        _isUpdatingTogglesInternally = true; // Prevent toggle listeners from triggering updates

        // Check if toggles are valid before accessing
        if (femaleToggle == null || maleToggle == null)
        {
            Debug.LogError("ProfileAvatarSelector: Cannot set toggles, references are null!");
            _isUpdatingTogglesInternally = false;
            return;
        }

        if (avatarId == 0) // Female selected
        {
            femaleToggle.isOn = true;
            femaleToggle.interactable = false; // Make female uninteractable
            maleToggle.isOn = false;
            maleToggle.interactable = true; // Make male interactable
            Debug.Log("ProfileAvatarSelector: Toggles set for Female (uninteractable).");
        }
        else if (avatarId == 1) // Male selected
        {
            maleToggle.isOn = true;
            maleToggle.interactable = false; // Make male uninteractable
            femaleToggle.isOn = false;
            femaleToggle.interactable = true; // Make female interactable
            Debug.Log("ProfileAvatarSelector: Toggles set for Male (uninteractable).");
        }
        else
        {
            Debug.LogWarning($"ProfileAvatarSelector: Unknown avatar_sprite_id: {avatarId}. Setting both toggles OFF and interactable.");
            femaleToggle.isOn = false; // Ensure neither is on for unknown ID
            maleToggle.isOn = false;
            femaleToggle.interactable = true; // Ensure they are interactable if neither is selected
            maleToggle.interactable = true;
            // You might want to set a specific default here (e.g., femaleToggle.isOn = true;)
            // if you always want one to be selected by default, even for invalid IDs.
        }

        _isUpdatingTogglesInternally = false; // Re-enable toggle listeners
    }
}