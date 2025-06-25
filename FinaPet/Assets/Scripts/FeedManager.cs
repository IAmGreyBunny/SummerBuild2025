using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic; // Required for List
using System.Text;
using System.IO; // Required for File.Exists and File.ReadAllText
using TMPro; // Assuming you are using TextMeshPro for UI text

// IMPORTANT: This script assumes that 'ServerConfig.cs' and 'PlayerData.cs'
// (containing PlayerMainData, GetPlayerDataRequestData, GetPlayerDataResponseData,
// and PlayerDataManager static class) are already defined in your Unity project
// in their respective files (e.g., Assets/Scripts/Config/ServerConfig.cs, Assets/Scripts/PlayerData.cs).
// If you encounter compile errors due to duplicate definitions, remove the corresponding
// class definitions from the bottom of THIS file.

/// <summary>
/// Manages fetching player inventory (specifically feed count) from the backend,
/// updating the UI display, and handling feed consumption.
/// This script integrates with existing ServerConfig and PlayerDataManager patterns.
/// </summary>
public class InventoryAndFeedManager : MonoBehaviour
{
    public static InventoryAndFeedManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI feedCountText; // Assign this in the Inspector

    // Backend URLs dynamically loaded from ServerConfig
    private string _getInventoryUrl;
    private string _updateInventoryUrl;

    private int _currentFeedCount;
    /// <summary>
    /// Gets and sets the current feed count. Automatically updates the UI when set.
    /// </summary>
    public int CurrentFeedCount
    {
        get { return _currentFeedCount; }
        private set
        {
            _currentFeedCount = value;
            UpdateFeedDisplay(); // Update UI whenever the count changes
        }
    }

    // The item ID for feed, as specified in your request.
    private const int FEED_ITEM_ID = 1;

    // Debug Player ID - Used as a fallback if PlayerDataManager hasn't loaded data yet.
    // IMPORTANT: In a live game, always ensure PlayerDataManager has loaded real data first.
    public int debug_player_id = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // As per previous instruction and project flow, this manager should NOT persist across scenes.
        // Therefore, DontDestroyOnLoad(gameObject); is intentionally omitted.
    }

    void Start()
    {
        // Start the initialization coroutine
        StartCoroutine(InitializeInventoryManager());
    }

    /// <summary>
    /// Orchestrates the loading of server config and player inventory.
    /// Waits for PlayerDataManager to be ready before fetching player-specific inventory.
    /// </summary>
    private IEnumerator InitializeInventoryManager()
    {
        // 1. Load Server Configuration synchronously using the existing ServerConfig.LoadFromFile()
        // This relies on your ServerConfig.cs having a public static ServerConfig LoadFromFile(string) method.
        ServerConfig loadedConfig = ServerConfig.LoadFromFile("Config/ServerConfig.json"); //
        if (loadedConfig == null)
        {
            Debug.LogError("[InventoryAndFeedManager] Failed to load ServerConfig. Cannot proceed. Ensure ServerConfig.json is in StreamingAssets/Config/ and ServerConfig.cs is correctly defined.");
            yield break;
        }

        // Construct full URLs using the loaded config's GetApiPath method
        // This pattern matches how PlayerDataManager constructs its URLs.
        string apiBaseUrl = loadedConfig.GetApiPath(); //
        _getInventoryUrl = apiBaseUrl + "/get_player_inventory.php"; //
        _updateInventoryUrl = apiBaseUrl + "/update_inventory.php"; // Assumed path for your update script

        Debug.Log($"[InventoryAndFeedManager] Constructed URLs: GetInventoryUrl={_getInventoryUrl}, UpdateInventoryUrl={_updateInventoryUrl}");

        // 2. Wait for PlayerDataManager to load player data
        // This is crucial to ensure player_id is available before fetching inventory.
        Debug.Log("[InventoryAndFeedManager] Waiting for PlayerDataManager to load data...");
        // Yield until next frame if data is not loaded. Add a timeout or error handling for production.
        while (!PlayerDataManager.IsDataLoaded) //
        {
            yield return null;
        }

        // 3. Get Player ID
        int currentPlayerId;
        if (PlayerDataManager.CurrentPlayerMainData != null) //
        {
            currentPlayerId = PlayerDataManager.CurrentPlayerMainData.player_id; //
            Debug.Log($"[InventoryAndFeedManager] Player ID obtained from PlayerDataManager: {currentPlayerId}");
        }
        else
        {
            currentPlayerId = debug_player_id;
            Debug.LogWarning($"[InventoryAndFeedManager] PlayerDataManager.CurrentPlayerMainData is null. Using debug player ID: {currentPlayerId}. Ensure PlayerDataManager successfully loads player data.");
        }

        // 4. Fetch Player Inventory
        StartCoroutine(FetchPlayerInventory(currentPlayerId));
    }

    /// <summary>
    /// Fetches the player's inventory from the backend PHP script.
    /// </summary>
    private IEnumerator FetchPlayerInventory(int playerId)
    {
        if (string.IsNullOrEmpty(_getInventoryUrl))
        {
            Debug.LogError("[InventoryAndFeedManager] Get Inventory URL is not set. Cannot fetch inventory. Check ServerConfig loading.");
            yield break;
        }

        using (UnityWebRequest www = new UnityWebRequest(_getInventoryUrl, "POST"))
        {
            // Reusing GetPlayerDataRequestData structure as it only contains player_id
            GetPlayerDataRequestData requestData = new GetPlayerDataRequestData //
            {
                player_id = playerId //
            };
            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[InventoryAndFeedManager] Sending inventory request for player ID: {playerId}, Body: {jsonData}");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[InventoryAndFeedManager] Error fetching inventory: {www.error}");
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log($"[InventoryAndFeedManager] Inventory Response: {jsonResponse}");

                try
                {
                    GetInventoryResponse response = JsonUtility.FromJson<GetInventoryResponse>(jsonResponse);

                    if (response.status_code == 0) // Success status code from PHP
                    {
                        int feedFound = 0;
                        if (response.items != null) //
                        {
                            foreach (InventoryItem item in response.items) //
                            {
                                if (item.item_id == FEED_ITEM_ID) //
                                {
                                    feedFound = item.quantity;
                                    break;
                                }
                            }
                        }
                        CurrentFeedCount = feedFound; // This will trigger UI update
                        Debug.Log($"[InventoryAndFeedManager] Player {playerId} has {CurrentFeedCount} feed items.");
                    }
                    else
                    {
                        Debug.LogError($"[InventoryAndFeedManager] Backend Error: {response.error_message} (Status Code: {response.status_code})"); //
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[InventoryAndFeedManager] Failed to parse inventory JSON: {e.Message}\nRaw Response: {jsonResponse}");
                }
            }
        }
    }

    /// <summary>
    /// Attempts to consume a feed item.
    /// This will update the local count and trigger a backend update.
    /// </summary>
    /// <param name="amount">The number of feed items to use. Default is 1.</param>
    /// <returns>True if feed was successfully consumed, false otherwise (e.g., not enough feed).</returns>
    public bool UseFeed(int amount = 1)
    {
        if (_currentFeedCount >= amount) // Use internal field to avoid recursive UI update during check
        {
            CurrentFeedCount -= amount; // Use property to trigger UI update

            // Get current player ID for the update request
            int currentPlayerId;
            if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null) //
            {
                currentPlayerId = PlayerDataManager.CurrentPlayerMainData.player_id; //
            }
            else
            {
                currentPlayerId = debug_player_id; // Fallback to debug ID if player data is missing
                Debug.LogWarning($"[InventoryAndFeedManager] PlayerDataManager data not available for update. Using debug player ID: {currentPlayerId}");
            }

            // Start the coroutine to update the database
            StartCoroutine(SendUpdateInventoryRequest(currentPlayerId, FEED_ITEM_ID, CurrentFeedCount));

            return true;
        }
        else
        {
            Debug.LogWarning("[InventoryAndFeedManager] Not enough feed to consume.");
            return false;
        }
    }

    /// <summary>
    /// Updates the UI TextMeshPro element with the current feed count.
    /// </summary>
    private void UpdateFeedDisplay()
    {
        if (feedCountText != null)
        {
            feedCountText.text = CurrentFeedCount.ToString();
        }
        else
        {
            Debug.LogWarning("[InventoryAndFeedManager] Feed Count Text (TextMeshProUGUI) is not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Coroutine for sending a request to update the inventory in the database.
    /// </summary>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="itemId">The ID of the item to update (feed item ID).</param>
    /// <param name="newQuantity">The new quantity of the item.</param>
    private IEnumerator SendUpdateInventoryRequest(int playerId, int itemId, int newQuantity)
    {
        if (string.IsNullOrEmpty(_updateInventoryUrl))
        {
            Debug.LogError("[InventoryAndFeedManager] Update Inventory URL is not set. Cannot update inventory. Check ServerConfig loading.");
            yield break;
        }

        using (UnityWebRequest www = new UnityWebRequest(_updateInventoryUrl, "POST"))
        {
            // Prepare JSON payload for the POST request
            // This structure should match what your update_inventory.php expects.
            string jsonData = JsonUtility.ToJson(new { player_id = playerId, item_id = itemId, new_quantity = newQuantity });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[InventoryAndFeedManager] Sending DB update request for Player: {playerId}, Item ID: {itemId}, New Quantity: {newQuantity}. Body: {jsonData}");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[InventoryAndFeedManager] Error updating inventory in DB: {www.error}");
            }
            else
            {
                string responseText = www.downloadHandler.text;
                Debug.Log($"[InventoryAndFeedManager] DB Update response: {responseText}");
                try
                {
                    // Assuming update_inventory.php returns a simple status_code and error_message
                    SimpleBackendResponse updateResponse = JsonUtility.FromJson<SimpleBackendResponse>(responseText);
                    if (updateResponse.status_code == 0)
                    {
                        Debug.Log($"[InventoryAndFeedManager] Database update successful.");
                    }
                    else
                    {
                        Debug.LogError($"[InventoryAndFeedManager] Database update failed: {updateResponse.error_message} (Code: {updateResponse.status_code})");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[InventoryAndFeedManager] Failed to parse DB update response: {e.Message}\nRaw Response: {responseText}");
                }
            }
        }
    }
}

// --- Data Structures for InventoryAndFeedManager specific responses ---
// These classes are defined here, but if you have a shared 'DataModels.cs' or similar file,
// it's better to move them there.

/// <summary>
/// Represents the JSON structure for a simple backend response (e.g., for updates).
/// </summary>
[System.Serializable]
public class SimpleBackendResponse
{
    public int status_code;
    public string error_message;
}

/// <summary>
/// Data class to deserialize the JSON response from get_player_inventory.php.
/// </summary>
[System.Serializable]
public class GetInventoryResponse
{
    public int status_code; //
    public string error_message; //
    public InventoryItem[] items; //
}

/// <summary>
/// Represents a single item within the player's inventory.
/// Fields derived from 'inventory' and 'items' tables join in get_player_inventory.php.
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public int item_id; //
    public int quantity; // Assumed field in 'inventory' table for item count
    public int inventory_id;
    public int player_id;
}