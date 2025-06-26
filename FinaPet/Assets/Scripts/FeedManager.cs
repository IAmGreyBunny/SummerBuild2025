using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using TMPro;

/// <summary>
/// Manages the entire pet feeding process. This includes updating the UI,
/// calculating local changes, and sending updates for pet hunger and
/// player inventory to the server.
/// </summary>
public class FeedManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Text element that displays the remaining feed count.")]
    public TextMeshProUGUI feedCountText;

    [Tooltip("A reference to the PetNeedsManager script which controls the hunger slider.")]
    public PetNeedsManager hungerManager;

    [Header("Feeding Settings")]
    [Tooltip("How many hunger points are restored per feeding.")]
    public int hungerPointsPerFeed = 20;

    [Header("Debug")]
    [Tooltip("The player ID to use for testing when not logged in.")]
    public int debug_id = 10;

    // --- Private State ---
    private int _currentFeedCount = 0;
    private bool _isFeeding = false; // Prevents spamming the feed button
    private const int FEED_ITEM_ID = 1; // The item_id for 'Feed' in your database
    private int player_id; // The active player ID, determined at Start.

    void Start()
    {
        // Determine which player ID to use for this session.
        if (PlayerAuthSession.IsLoggedIn)
        {
            player_id = PlayerAuthSession.PlayerId;
            Debug.Log($"FeedManager: Player is logged in. Using Player ID: {player_id}");
        }
        else
        {
            player_id = debug_id;
            Debug.LogWarning($"FeedManager: Player not logged in. Using Debug ID: {player_id}");
        }

        // When the scene starts, fetch the player's inventory to get the initial feed count.
        StartCoroutine(FetchPlayerInventory());
    }

    /// <summary>
    /// PUBLIC method to be called by your "Feed" button's OnClick event.
    /// </summary>
    public void OnFeedButtonClicked()
    {
        // --- 1. Pre-computation Checks ---
        // Block if a feeding action is already in progress.
        if (_isFeeding)
        {
            Debug.LogWarning("FeedManager: Already processing a feed action.");
            return;
        }

        // Check if the player has any feed left.
        if (_currentFeedCount <= 0)
        {
            Debug.LogWarning("FeedManager: No feed left!");
            // TODO: Optionally show a "You have no feed!" message to the player.
            return;
        }

        // Check if the pet's hunger is already full.
        if (hungerManager.IsHungerFull())
        {
            Debug.Log("FeedManager: Pet is already full, no need to feed.");
            // TODO: Optionally show a "Your pet is full!" message.
            return;
        }

        // --- 2. Start the Feeding Process ---
        // If all checks pass, begin the feeding sequence.
        StartCoroutine(Co_FeedProcess());
    }

    /// <summary>
    /// The master coroutine that handles the entire feeding sequence step-by-step.
    /// </summary>
    private IEnumerator Co_FeedProcess()
    {
        _isFeeding = true; // Lock the process to prevent spamming.

        // --- 3. Perform Local Calculations ---
        // Determine the new values before updating anything.
        int newFeedCount = _currentFeedCount - 1;
        int newHunger = hungerManager.GetCurrentHunger() + hungerPointsPerFeed;

        // --- 4. Optimistic UI Updates ---
        // Update the UI immediately to give the player instant feedback.
        _currentFeedCount = newFeedCount;
        UpdateFeedCountUI();
        hungerManager.SetHunger(newHunger);

        // --- 5. Send Updates to the Server ---
        // This coroutine will handle the two separate web requests.
        yield return StartCoroutine(Co_UpdateStatsOnServer(newHunger, newFeedCount));

        _isFeeding = false; // Unlock the process once everything is complete.
    }

    /// <summary>
    /// Coroutine that sends the updated stats to the PHP backend.
    /// It sends two separate requests: one for pet hunger and one for player inventory.
    /// </summary>
    private IEnumerator Co_UpdateStatsOnServer(int newHunger, int newFeedCount)
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("FeedManager: Cannot update server, GameDataManager has no selected pet. Make sure pet data is being passed correctly between scenes.");
            _isFeeding = false; // Unlock the feed button
            yield break;
        }
        if (player_id <= 0)
        {
            Debug.LogError($"FeedManager: Cannot update server, invalid player_id: {player_id}.");
            _isFeeding = false; // Unlock the feed button
            yield break;
        }

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("FeedManager: Cannot update server, failed to load API path.");
            yield break; // Exit if the API path can't be found.
        }

        // --- First Request: Update Pet Hunger ---
        string hungerUrl = apiPath + "/update_pet_hunger.php";
        var hungerRequestData = new UpdatePetHungerRequest
        {
            pet_id = GameDataManager.Instance.selectedPet.pet_id,
            hunger = newHunger,
            // THIS IS THE FIX: Format the UTC time to match the standard SQL DATETIME format.
            last_fed = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };
        string hungerJson = JsonUtility.ToJson(hungerRequestData);

        Debug.Log($"FeedManager: Sending [Hunger Update] request to '{hungerUrl}'. JSON Body: {hungerJson}");
        yield return SendPostRequest(hungerUrl, hungerJson, "Hunger Update");

        // --- Second Request: Update Player Inventory ---
        string inventoryUrl = apiPath + "/update_inventory_item.php";
        var inventoryRequestData = new UpdateInventoryRequest
        {
            player_id = this.player_id, // Use the player_id determined at Start
            item_id = FEED_ITEM_ID,
            quantity = -1
        };
        string inventoryJson = JsonUtility.ToJson(inventoryRequestData);

        Debug.Log($"FeedManager: Sending [Inventory Update] request to '{inventoryUrl}'. JSON Body: {inventoryJson}");
        yield return SendPostRequest(inventoryUrl, inventoryJson, "Inventory Update");
    }

    /// <summary>
    /// A reusable helper method to send POST requests and log the outcome.
    /// </summary>
    private IEnumerator SendPostRequest(string url, string jsonBody, string requestName)
    {
        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"FeedManager ({requestName}): Error sending request to {url}: {request.error}");
            }
            else
            {
                Debug.Log($"FeedManager ({requestName}): Successfully received response from {url}. Response: {request.downloadHandler.text}");
            }
        }
    }

    /// <summary>
    /// Fetches the player's full inventory from the server to find the initial feed count.
    /// </summary>
    private IEnumerator FetchPlayerInventory()
    {
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("FeedManager: API path not found. Cannot fetch inventory.");
            yield break;
        }

        string url = apiPath + "/get_player_inventory.php";

        var requestData = new GetInventoryRequest { player_id = this.player_id };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"FeedManager: Fetching inventory with JSON body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            _currentFeedCount = 0; // Default to 0 before processing the response
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GetInventoryResponse>(request.downloadHandler.text);
                if (response.status_code == 0 && response.items != null)
                {
                    foreach (var item in response.items)
                    {
                        if (item.item_id == FEED_ITEM_ID)
                        {
                            _currentFeedCount = item.quantity;
                            break;
                        }
                    }
                }
                Debug.Log($"FeedManager: Inventory fetched for player {this.player_id}. Player has {_currentFeedCount} feed items.");
            }
            else
            {
                Debug.LogError($"FeedManager: Failed to fetch inventory. Error: {request.error}");
            }
            UpdateFeedCountUI();
        }
    }

    /// <summary>
    /// Updates the UI text for the feed count.
    /// </summary>
    private void UpdateFeedCountUI()
    {
        if (feedCountText != null)
        {
            feedCountText.text = _currentFeedCount.ToString();
        }
    }

    #region Data Transfer Classes
    // --- Helper classes for creating JSON to send to the server ---

    [Serializable]
    private class UpdatePetHungerRequest
    {
        public int pet_id;
        public int hunger;
        public string last_fed;
    }

    [Serializable]
    private class UpdateInventoryRequest
    {
        public int player_id;
        public int item_id;
        public int quantity;
    }

    // --- Helper classes for reading JSON from the server ---

    [Serializable]
    private class GetInventoryRequest
    {
        public int player_id;
    }

    [Serializable]
    private class GetInventoryResponse
    {
        public int status_code;
        public InventoryItem[] items;
    }

    [Serializable]
    private class InventoryItem
    {
        public int item_id;
        public int quantity;
    }
    #endregion
}
