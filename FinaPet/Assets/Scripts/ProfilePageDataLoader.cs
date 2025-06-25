using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using System.Collections.Generic; // Make sure this is included for List

public class ProfilePageDataLoader : MonoBehaviour
{
    // A static event that other scripts can subscribe to.
    public static event Action OnProfileDataLoaded;

    [Header("UI Text References")]
    [Tooltip("TextMeshProUGUI component to display the player's username.")]
    public TextMeshProUGUI usernameText;
    [Tooltip("TextMeshProUGUI component to display the player's coin count.")]
    public TextMeshProUGUI coinsText;
    [Tooltip("TextMeshProUGUI component to display the number of pets the player owns.")]
    public TextMeshProUGUI petsCountText;

    [Header("Debug Settings")]
    [Tooltip("The ID of the player to use when PlayerAuthSession indicates no login.")]
    public int debugPlayerId = 1;

    private int _fetchedPetsCount = 0;

    void OnEnable()
    {
        // Sanity checks for UI references
        if (usernameText == null || coinsText == null || petsCountText == null)
        {
            Debug.LogError("ProfilePageDataLoader: One or more UI Text references are not assigned!", this);
            return;
        }
        LoadProfileData();
    }

    /// <summary>
    /// Initiates the loading process for all profile data.
    /// </summary>
    public void LoadProfileData()
    {
        int playerIdToFetch = PlayerAuthSession.IsLoggedIn ? PlayerAuthSession.PlayerId : debugPlayerId;
        Debug.Log($"ProfilePageDataLoader: Starting data load for player ID: {playerIdToFetch}");
        StartCoroutine(Co_LoadAllProfileData(playerIdToFetch));
    }

    /// <summary>
    /// Coroutine to load all profile-related data in a specific order.
    /// </summary>
    private IEnumerator Co_LoadAllProfileData(int playerId)
    {
        // Step 1: Fetch the core player data and WAIT for it to complete.
        yield return PlayerDataManager.FetchPlayerData(playerId);

        // Step 2: Fetch the pet count (only runs after player data is fetched).
        yield return Co_FetchPetCount(playerId);

        // Step 3: Update this script's own UI elements.
        UpdateProfileUI();

        // Step 4: Check if the primary data load was successful, then broadcast the event.
        if (PlayerDataManager.IsDataLoaded)
        {
            Debug.Log("ProfilePageDataLoader: Data load successful. Broadcasting OnProfileDataLoaded event.");
            OnProfileDataLoaded?.Invoke();
        }
        else
        {
            Debug.LogError("ProfilePageDataLoader: Player data failed to load. The OnProfileDataLoaded event will NOT be broadcast.");
        }
    }

    /// <summary>
    /// Fetches the number of pets for a given player ID.
    /// </summary>
    private IEnumerator Co_FetchPetCount(int ownerId)
    {
        _fetchedPetsCount = 0; // Reset count before fetching
        var requestData = new OwnedPetsManager.GetPetsRequestData { owner_id = ownerId };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();

        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("ProfilePageDataLoader: Could not load server config for pet count fetch.");
            yield break;
        }

        string fullUrl = apiPath + "/get_pets.php";
        Debug.Log($"ProfilePageDataLoader: Sending request to get pet count for owner ID: {ownerId}");

        using (var request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<OwnedPetsManager.GetPetsResponseData>(request.downloadHandler.text);
                if (response.status_code == 0)
                {
                    _fetchedPetsCount = response.pets?.Count ?? 0;
                    Debug.Log($"ProfilePageDataLoader: Successfully fetched pet count: {_fetchedPetsCount}");
                }
            }
            else
            {
                Debug.LogError($"ProfilePageDataLoader: Failed to fetch pet count! Error: {request.error}");
            }
        }
    }

    /// <summary>
    /// Updates the UI Text components with the fetched player data.
    /// </summary>
    private void UpdateProfileUI()
    {
        usernameText.text = PlayerAuthSession.IsLoggedIn ? PlayerAuthSession.Username : "Guest " + debugPlayerId;

        if (PlayerDataManager.IsDataLoaded)
        {
            coinsText.text = "$" + PlayerDataManager.CurrentPlayerMainData.coin.ToString();
        }
        else
        {
            coinsText.text = "Coins: N/A";
            Debug.LogWarning("ProfilePageDataLoader: Player data not loaded or is null for coins display.");
        }

        petsCountText.text = _fetchedPetsCount.ToString();
    }
}