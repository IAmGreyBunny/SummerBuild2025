using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;


/// <summary>
/// Represents the player's main data structure as returned from the server.
/// Fields match your 'player_data' table columns exactly.
/// </summary>
[System.Serializable]
public class PlayerMainData
{
    public int player_id;
    public int coin;
    public int avatar_sprite_id;
}

/// <summary>
/// Represents the structure of the request sent to get_player_data.php.
/// </summary>
[System.Serializable]
public class GetPlayerDataRequestData
{
    public int player_id;
}

/// <summary>
/// Represents the full JSON response structure from get_player_data.php.
/// </summary>
[System.Serializable]
public class GetPlayerDataResponseData
{
    public int status_code;
    public string error_message;
    public List<PlayerMainData> player_data; // 'player_data' is an array/list in PHP
}

/// <summary>
/// Static class to manage fetching and holding the current player's data.
/// Uses Unity's Coroutine system for web requests.
/// </summary>
public static class PlayerDataManager
{
    // Public static properties to hold the fetched player data and status
    public static PlayerMainData CurrentPlayerMainData { get; private set; }
    public static bool IsDataLoaded { get; private set; } = false;
    public static string LastErrorMessage { get; private set; } = "";
    // New: Properties to track the result of the update operation
    public static bool IsUpdateSuccessful { get; private set; } = false;
    public static string LastUpdateMessage { get; private set; } = "";


    /// <summary>
    /// Resets the loaded player data and status.
    /// Call this when a player logs out or before loading new player data.
    /// </summary>
    public static void ResetPlayerData()
    {
        CurrentPlayerMainData = null;
        IsDataLoaded = false;
        LastErrorMessage = "";
        IsUpdateSuccessful = false; // Reset update status too
        LastUpdateMessage = "";     // Reset update message
        Debug.Log("Player data has been reset.");
    }

    /// <summary>
    /// Initiates a coroutine to fetch player data from the server.
    /// This method should be called from a MonoBehaviour using StartCoroutine.
    /// Example: `StartCoroutine(PlayerDataManager.FetchPlayerData(playerId));`
    /// </summary>
    /// <param name="playerId">The ID of the player whose data to fetch.</param>
    /// <returns>IEnumerator for use in a Coroutine.</returns>
    public static IEnumerator FetchPlayerData(int playerId)
    {
        ResetPlayerData(); // Clear any existing data before fetching new.

        // --- 1. Prepare the JSON Request ---
        GetPlayerDataRequestData requestData = new GetPlayerDataRequestData
        {
            player_id = playerId
        };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        Debug.Log($"PlayerDataManager: Sending request for player ID: {playerId}, Body: {jsonRequestBody}");

        // --- 2. Create and Send the UnityWebRequest ---
        // Assuming ServerConfig.LoadFromFile().GetApiPath() is accessible and provides the base URL.
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/get_player_data.php"; // Your PHP script endpoint

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest(); // Wait for the request to complete

            // --- 3. Handle the Response ---
            if (request.result != UnityWebRequest.Result.Success)
            {
                LastErrorMessage = $"API Call Failed! Error: {request.error}";
                Debug.LogError($"PlayerDataManager: {LastErrorMessage}");
                IsDataLoaded = false;
            }
            else
            {
                Debug.Log($"PlayerDataManager: API Call Successful! Response: {request.downloadHandler.text}");
                try
                {
                    GetPlayerDataResponseData response = JsonUtility.FromJson<GetPlayerDataResponseData>(request.downloadHandler.text);

                    if (response.status_code == 0)
                    {
                        if (response.player_data != null && response.player_data.Count > 0)
                        {
                            // Assuming get_player_data returns data for a single player_id
                            CurrentPlayerMainData = response.player_data[0];
                            IsDataLoaded = true;
                            LastErrorMessage = ""; // Clear any previous errors
                            Debug.Log($"PlayerDataManager: Successfully loaded data for player ID: {CurrentPlayerMainData.player_id}. Coins: {CurrentPlayerMainData.coin}, Avatar Sprite ID: {CurrentPlayerMainData.avatar_sprite_id}");
                        }
                        else
                        {
                            LastErrorMessage = $"No player data found for ID: {playerId}";
                            Debug.LogWarning($"PlayerDataManager: {LastErrorMessage}");
                            IsDataLoaded = false;
                        }
                    }
                    else
                    {
                        LastErrorMessage = $"Server returned an error: {response.error_message} (Status Code: {response.status_code})";
                        Debug.LogError($"PlayerDataManager: {LastErrorMessage}");
                        IsDataLoaded = false;
                    }
                }
                catch (System.Exception e)
                {
                    LastErrorMessage = $"Failed to parse player data JSON response: {e.Message}";
                    Debug.LogError($"PlayerDataManager: {LastErrorMessage}\nRaw Response: {request.downloadHandler.text}");
                    IsDataLoaded = false;
                }
            }
        }
    }

    /// <summary>
    /// Initiates a coroutine to update player data on the server.
    /// This method should be called from a MonoBehaviour using StartCoroutine.
    /// Example: `StartCoroutine(PlayerDataManager.UpdatePlayerDataOnServer(10, 10, 1));`
    /// </summary>
    /// <param name="playerId">The ID of the player to update.</param>
    /// <param name="coins">The new coin value.</param>
    /// <param name="avatarSpriteId">The new avatar sprite ID.</param>
    /// <returns>IEnumerator for use in a Coroutine.</returns>
    public static IEnumerator UpdatePlayerDataOnServer(int playerId, int coins, int avatarSpriteId)
    {
        IsUpdateSuccessful = false; // Reset update status
        LastUpdateMessage = "";     // Clear previous update message

        // --- 1. Prepare the JSON Request ---
        // The structure of the update request matches PlayerMainData
        PlayerMainData updateData = new PlayerMainData
        {
            player_id = playerId,
            coin = coins,
            avatar_sprite_id = avatarSpriteId
        };
        string jsonRequestBody = JsonUtility.ToJson(updateData);
        Debug.Log($"PlayerDataManager: Sending update request for player ID: {playerId}, Body: {jsonRequestBody}");

        // --- 2. Create and Send the UnityWebRequest ---
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/update_player_data.php"; // Your PHP script endpoint for updating

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest(); // Wait for the request to complete

            // --- 3. Handle the Response ---
            if (request.result != UnityWebRequest.Result.Success)
            {
                LastUpdateMessage = $"Update API Call Failed! Error: {request.error}";
                Debug.LogError($"PlayerDataManager: {LastUpdateMessage}");
                IsUpdateSuccessful = false;
            }
            else
            {
                Debug.Log($"PlayerDataManager: Update API Call Successful! Response: {request.downloadHandler.text}");
                try
                {
                    // Assuming update_player.php returns a simple status response
                    UpdatePlayerResponse response = JsonUtility.FromJson<UpdatePlayerResponse>(request.downloadHandler.text);

                    if (response.status_code == 0)
                    {
                        IsUpdateSuccessful = true;
                        LastUpdateMessage = "Player data updated successfully.";
                        Debug.Log("PlayerDataManager: Player data updated successfully on server.");

                        // Optional: If the update was successful, refresh the local CurrentPlayerMainData
                        // This might be done by immediately calling FetchPlayerData again,
                        // or by manually updating CurrentPlayerMainData if the response includes the new data.
                        // For simplicity, we'll just log success here.
                        // If you want the local data to reflect the change, you might call:
                        // StartCoroutine(FetchPlayerData(playerId)); // You need a reference to a MonoBehaviour to call StartCoroutine
                    }
                    else
                    {
                        IsUpdateSuccessful = false;
                        LastUpdateMessage = $"Server returned an error during update: {response.error_message} (Status Code: {response.status_code})";
                        Debug.LogError($"PlayerDataManager: {LastUpdateMessage}");
                    }
                }
                catch (System.Exception e)
                {
                    LastUpdateMessage = $"Failed to parse update response JSON: {e.Message}";
                    Debug.LogError($"PlayerDataManager: {LastUpdateMessage}\nRaw Response: {request.downloadHandler.text}");
                    IsUpdateSuccessful = false;
                }
            }
        }
    }

    // --- Helper classes for Update Player Response ---
    [System.Serializable]
    public class UpdatePlayerResponse
    {
        public int status_code;
        public string error_message;
        // Your PHP script returns an empty array for player_data on success.
        // If you remove that, this class works fine. If it still returns it,
        // you might need to add `public List<PlayerMainData> player_data;` here.
    }
}