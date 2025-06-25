using UnityEngine;
using TMPro; // Required for TextMeshProUGUI
using System.Collections; // Required for Coroutines
using UnityEngine.Networking; // Required for UnityWebRequest
using System.Text; // Required for Encoding

/// <summary>
/// Loads and displays player profile data (username, coins, pet count)
/// on the profile page.
/// </summary>
public class ProfilePageDataLoader : MonoBehaviour
{
    [Header("UI Text References")]
    [Tooltip("TextMeshProUGUI component to display the player's username.")]
    public TextMeshProUGUI usernameText;
    [Tooltip("TextMeshProUGUI component to display the player's coin count.")]
    public TextMeshProUGUI coinsText;
    [Tooltip("TextMeshProUGUI component to display the number of pets the player owns.")]
    public TextMeshProUGUI petsCountText;

    [Header("Debug Settings")]
    [Tooltip("The ID of the player to use when PlayerAuthSession indicates no login.")]
    public int debugPlayerId = 1; // Default for testing if not logged in

    // Private variable to store the fetched pet count
    private int _fetchedPetsCount = 0;

    void OnEnable() // Using OnEnable to refresh data every time the object becomes active (e.g., scene loads)
    {
        // Sanity checks for UI references
        if (usernameText == null || coinsText == null || petsCountText == null)
        {
            Debug.LogError("ProfilePageDataLoader: One or more UI Text references are not assigned! Please assign them in the Inspector.", this);
            return;
        }

        // Initiate data loading
        LoadProfileData();
    }

    /// <summary>
    /// Initiates the loading process for all profile data.
    /// </summary>
    public void LoadProfileData()
    {
        int playerIdToFetch;
        if (PlayerAuthSession.IsLoggedIn)
        {
            playerIdToFetch = PlayerAuthSession.PlayerId;
            Debug.Log($"ProfilePageDataLoader: Loading profile data for logged-in player ID: {playerIdToFetch}");
        }
        else
        {
            playerIdToFetch = debugPlayerId;
            Debug.LogWarning($"ProfilePageDataLoader: Player not logged in. Using debugPlayerId: {playerIdToFetch}");
        }

        StartCoroutine(Co_LoadAllProfileData(playerIdToFetch));
    }

    /// <summary>
    /// Coroutine to load all profile-related data concurrently or in sequence.
    /// </summary>
    private IEnumerator Co_LoadAllProfileData(int playerId)
    {
        // --- 1. Fetch Player Main Data (Coins) ---
        // PlayerDataManager.FetchPlayerData also resets previous errors/flags internally.
        yield return StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));

        // --- 2. Fetch Pet Count ---
        yield return StartCoroutine(Co_FetchPetCount(playerId));

        // --- 3. Update UI based on fetched data ---
        UpdateProfileUI();
    }

    /// <summary>
    /// Fetches the number of pets for a given player ID.
    /// </summary>
    private IEnumerator Co_FetchPetCount(int ownerId)
    {
        _fetchedPetsCount = 0; // Reset count before fetching

        // Reuse data structures from OwnedPetsManager
        OwnedPetsManager.GetPetsRequestData requestData = new OwnedPetsManager.GetPetsRequestData
        {
            owner_id = ownerId
        };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        Debug.Log($"ProfilePageDataLoader: Sending request to get pet count for owner ID: {ownerId}, Body: {jsonRequestBody}");

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/get_pets.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ProfilePageDataLoader: Failed to fetch pet count! Error: {request.error}");
                // Keep _fetchedPetsCount at 0 or set to a specific error value
            }
            else
            {
                Debug.Log($"ProfilePageDataLoader: Pet count API response: {request.downloadHandler.text}");
                try
                {
                    OwnedPetsManager.GetPetsResponseData response = JsonUtility.FromJson<OwnedPetsManager.GetPetsResponseData>(request.downloadHandler.text);

                    if (response.status_code == 0)
                    {
                        if (response.pets != null)
                        {
                            _fetchedPetsCount = response.pets.Count;
                            Debug.Log($"ProfilePageDataLoader: Successfully fetched pet count: {_fetchedPetsCount}");
                        }
                        else
                        {
                            _fetchedPetsCount = 0;
                            Debug.LogWarning("ProfilePageDataLoader: Pet count response 'pets' array was null.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"ProfilePageDataLoader: Pet count API returned an error: {response.error_message} (Status Code: {response.status_code})");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"ProfilePageDataLoader: Failed to parse pet count JSON: {e.Message}\nRaw Response: {request.downloadHandler.text}");
                }
            }
        }
    }

    /// <summary>
    /// Updates the UI Text components with the fetched player data.
    /// </summary>
    private void UpdateProfileUI()
    {
        // Update Username
        // Assuming PlayerAuthSession has a static property for username (e.g., PlayerAuthSession.Username)
        // You'll need to ensure PlayerAuthSession exposes the username if it doesn't already.
        // If not, you might need to fetch it as part of player_data or elsewhere.
        if (PlayerAuthSession.IsLoggedIn)
        {
            // Placeholder: Replace with actual username from PlayerAuthSession if it exists
            // For now, if no explicit username, we'll indicate if logged in.
            usernameText.text = PlayerAuthSession.Username;
            // Or if PlayerAuthSession provides username: usernameText.text = PlayerAuthSession.Username;
        }
        else
        {
            usernameText.text = "Guest " + debugPlayerId;
        }


        // Update Coins
        if (PlayerDataManager.IsDataLoaded && PlayerDataManager.CurrentPlayerMainData != null)
        {
            coinsText.text ="$" + PlayerDataManager.CurrentPlayerMainData.coin.ToString();
        }
        else
        {
            coinsText.text = "Coins: N/A";
            Debug.LogWarning("ProfilePageDataLoader: Player data not loaded or is null for coins display.");
        }

        // Update Pet Count
        petsCountText.text = _fetchedPetsCount.ToString();
    }
}