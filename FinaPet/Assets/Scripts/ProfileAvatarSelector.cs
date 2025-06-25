using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages male/female avatar selection toggles using direct click events for maximum reliability.
/// This final version defensively retrieves player data just-in-time for updates.
/// </summary>
public class ProfileAvatarSelector : MonoBehaviour
{
    [Header("UI Toggles")]
    public Toggle femaleToggle;
    public Toggle maleToggle;

    [Header("Dependencies")]
    public PlayerAvatarUpdater playerAvatarUpdater;

    // A flag to prevent the user from spamming the update button.
    private bool _isUpdating = false;

    void OnEnable()
    {
        ProfilePageDataLoader.OnProfileDataLoaded += HandleProfileDataReady;
    }

    void OnDisable()
    {
        ProfilePageDataLoader.OnProfileDataLoaded -= HandleProfileDataReady;
    }

    private void HandleProfileDataReady()
    {
        Debug.Log("ProfileAvatarSelector: Initial data loaded. Setting toggle visual state.");
        _isUpdating = false; // Ensure the flag is reset on data load.
        if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
        {
            SetTogglesVisualState(PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id);
        }
    }

    /// <summary>
    /// PUBLIC method linked to a PointerClick event from the FEMALE toggle.
    /// </summary>
    public void OnFemaleToggleClicked()
    {
        Debug.Log("Female Toggle Clicked!");
        if (_isUpdating) return; // Prevent new updates while one is in progress.

        if (PlayerDataManager.CurrentPlayerMainData != null && PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id != 0)
        {
            StartCoroutine(Co_UpdateAvatar(0));
        }
    }

    /// <summary>
    /// PUBLIC method linked to a PointerClick event from the MALE toggle.
    /// </summary>
    public void OnMaleToggleClicked()
    {
        Debug.Log("Male Toggle Clicked!");
        if (_isUpdating) return; // Prevent new updates while one is in progress.

        if (PlayerDataManager.CurrentPlayerMainData != null && PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id != 1)
        {
            StartCoroutine(Co_UpdateAvatar(1));
        }
    }

    /// <summary>
    /// The master coroutine that handles the entire update process.
    /// </summary>
    private IEnumerator Co_UpdateAvatar(int newAvatarSpriteId)
    {
        // --- THIS IS THE DEFINITIVE FIX ---
        // 1. Immediately block new updates and check for required data.
        _isUpdating = true;
        if (!PlayerAuthSession.IsLoggedIn || PlayerDataManager.CurrentPlayerMainData == null)
        {
            Debug.LogError("Cannot update: Player not logged in or data is missing.");
            _isUpdating = false;
            yield break; // Exit the coroutine.
        }

        // 2. Defensively get the MOST CURRENT player data just before sending the request.
        int playerId = PlayerAuthSession.PlayerId;
        int currentCoins = PlayerDataManager.CurrentPlayerMainData.coin;

        Debug.Log($"ProfileAvatarSelector: Sending update for Player ID: {playerId}, Avatar ID: {newAvatarSpriteId}, Coins: {currentCoins}");

        // 3. Disable UI and send the request.
        femaleToggle.interactable = false;
        maleToggle.interactable = false;

        yield return StartCoroutine(PlayerDataManager.UpdatePlayerDataOnServer(playerId, currentCoins, newAvatarSpriteId));

        // 4. Handle the result.
        if (PlayerDataManager.IsUpdateSuccessful)
        {
            Debug.Log("Update successful. Reloading all profile data to ensure UI consistency.");
            // Re-run the scene's main loader to get fresh, confirmed data for all components.
            FindObjectOfType<ProfilePageDataLoader>()?.LoadProfileData();
        }
        else
        {
            Debug.LogError("Update failed. Reverting UI to previous state.");
            // If the update fails, restore the UI to its last known good state.
            SetTogglesVisualState(PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id);
            _isUpdating = false; // Allow the user to try again.
        }
        // The _isUpdating flag will be reset in HandleProfileDataReady after a successful reload.
    }

    /// <summary>
    /// Sets the visual and interactable state of the toggles.
    /// </summary>
    private void SetTogglesVisualState(int avatarId)
    {
        femaleToggle.SetIsOnWithoutNotify(avatarId == 0);
        maleToggle.SetIsOnWithoutNotify(avatarId == 1);

        femaleToggle.interactable = (avatarId != 0);
        maleToggle.interactable = (avatarId != 1);
    }
}