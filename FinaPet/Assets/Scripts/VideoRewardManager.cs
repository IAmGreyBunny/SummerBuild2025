using UnityEngine;
using UnityEngine.UI; // Required for Button if you add one to the panel
using UnityEngine.Video;
using System.Collections; // Required for Coroutines
using UnityEngine.Events; // Required for UnityEvent
using TMPro; // Assuming you might use TextMeshPro for status messages

/// <summary>
/// Manages video playback and rewards the player upon completion.
/// This script now integrates with PlayerDataManager to add coins
/// and provides an event for scene transitions after reward is claimed.
/// </summary>
public class VideoRewardManager : MonoBehaviour
{
    [Header("Video Player Settings")]
    [Tooltip("Drag your VideoPlayer component here in the Inspector.")]
    public VideoPlayer videoPlayer;

    [Header("Reward Panel Settings")]
    [Tooltip("Drag your reward popup panel UI GameObject here.")]
    public GameObject rewardPopupPanel;

    [Tooltip("The amount of coins to reward the player when they claim the video reward.")]
    public int rewardCoinsAmount = 20; // Set your desired reward amount here

    [Tooltip("The player ID to use if no player is logged in via PlayerAuthSession (for testing).")]
    public int debugPlayerId = 1;

    [Header("UI Feedback")]
    [Tooltip("Reference to a TextMeshProUGUI component to display reward status (e.g., 'Coins Added!', 'Error!').")]
    public TextMeshProUGUI rewardStatusText; // Assign in Inspector

    [Header("Events")]
    [Tooltip("This event is invoked when the reward claiming process (including database update) successfully completes.")]
    public UnityEvent OnRewardClaimedComplete; // New event for scene change or other follow-up actions

    private bool _isClaimingReward = false; // Flag to prevent multiple reward claims

    void Start()
    {
        // Register the event when the video finishes playing
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("VideoRewardManager: VideoPlayer component is not assigned!");
        }

        // Ensure the reward popup is initially hidden
        if (rewardPopupPanel != null)
        {
            rewardPopupPanel.SetActive(false);
        }

        // Initialize status text
        if (rewardStatusText != null)
        {
            rewardStatusText.text = "";
        }
    }

    /// <summary>
    /// Event handler for when the video playback reaches its loop point (i.e., finishes).
    /// </summary>
    /// <param name="vp">The VideoPlayer component that triggered the event.</param>
    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video finished playing. Displaying reward popup.");
        // Show the reward popup
        if (rewardPopupPanel != null)
        {
            rewardPopupPanel.SetActive(true);
            SetRewardStatusMessage("", Color.white); // Clear previous status
        }
    }

    /// <summary>
    /// Public method to be called by a UI Button (e.g., "Claim Reward" button on the popup).
    /// This method now handles adding coins to the player's balance.
    /// </summary>
    public void ClaimReward()
    {
        if (_isClaimingReward)
        {
            Debug.LogWarning("VideoRewardManager: Reward claim already in progress. Please wait.");
            SetRewardStatusMessage("Processing...", Color.yellow);
            return;
        }

        Debug.Log($"Attempting to claim {rewardCoinsAmount} coins...");
        StartCoroutine(Co_ClaimRewardAndAddCoins());
    }

    /// <summary>
    /// Coroutine to handle the reward claiming process, including adding coins to the player.
    /// </summary>
    private IEnumerator Co_ClaimRewardAndAddCoins()
    {
        _isClaimingReward = true; // Set flag to true to prevent re-entry
        SetRewardStatusMessage("Claiming reward...", Color.white);

        // Determine the player ID to operate on
        int currentPlayerId = PlayerAuthSession.IsLoggedIn ? PlayerAuthSession.PlayerId : debugPlayerId;
        Debug.Log($"VideoRewardManager: Claiming reward for player ID: {currentPlayerId}");

        // --- Step 1: Fetch current player data ---
        Debug.Log("VideoRewardManager: Fetching current player data...");
        yield return PlayerDataManager.FetchPlayerData(currentPlayerId);

        if (!PlayerDataManager.IsDataLoaded || PlayerDataManager.CurrentPlayerMainData == null)
        {
            string errorMessage = $"VideoRewardManager: Failed to fetch player data for ID {currentPlayerId}. Error: {PlayerDataManager.LastErrorMessage}";
            Debug.LogError(errorMessage);
            SetRewardStatusMessage("Error: Could not get player data!", Color.red);
            _isClaimingReward = false; // Reset flag
            yield break; // Exit coroutine if data fetch fails
        }

        // Get current coins and avatar ID
        int currentCoins = PlayerDataManager.CurrentPlayerMainData.coin;
        int currentAvatarId = PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id;
        int newTotalCoins = currentCoins + rewardCoinsAmount;

        Debug.Log($"VideoRewardManager: Player current coins: {currentCoins}, New total coins after reward: {newTotalCoins}");

        // --- Step 2: Update player data on the server ---
        Debug.Log($"VideoRewardManager: Sending update to server for player ID {currentPlayerId} with new coins {newTotalCoins}.");
        yield return PlayerDataManager.UpdatePlayerDataOnServer(currentPlayerId, newTotalCoins, currentAvatarId);

        if (PlayerDataManager.IsUpdateSuccessful)
        {
            Debug.Log($"VideoRewardManager: Successfully awarded {rewardCoinsAmount} coins. New total: {newTotalCoins}");
            SetRewardStatusMessage($"Earned {rewardCoinsAmount} coins!", Color.green);

            // Invoke the event *after* successful database update
            OnRewardClaimedComplete?.Invoke();
            Debug.Log("VideoRewardManager: OnRewardClaimedComplete event invoked.");
        }
        else
        {
            string errorMessage = $"VideoRewardManager: Failed to add coins. Error: {PlayerDataManager.LastUpdateMessage}";
            Debug.LogError(errorMessage);
            SetRewardStatusMessage($"Failed to add coins: {PlayerDataManager.LastUpdateMessage}", Color.red);
        }

        // Hide the panel after processing the claim (regardless of success/failure)
        // You might want a slight delay here if the status message needs to be seen.
        yield return new WaitForSeconds(1.5f); // Display message for 1.5 seconds
        if (rewardPopupPanel != null)
        {
            rewardPopupPanel.SetActive(false);
        }

        _isClaimingReward = false; // Reset flag
    }

    /// <summary>
    /// Helper method to set the text and color of the reward status UI.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="color">The color of the message.</param>
    private void SetRewardStatusMessage(string message, Color color)
    {
        if (rewardStatusText != null)
        {
            rewardStatusText.text = message;
            rewardStatusText.color = color;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the video player event to prevent memory leaks
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}
