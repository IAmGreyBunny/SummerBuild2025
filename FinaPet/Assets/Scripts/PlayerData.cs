using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Data structure for the player's main data.
[System.Serializable]
public class PlayerMainData
{
    public int player_id;
    public int coin;
    public int avatar_sprite_id;
}

// Data structure for the request to get player data.
[System.Serializable]
public class GetPlayerDataRequestData
{
    public int player_id;
}

// Data structure for the full JSON response from the server.
[System.Serializable]
public class GetPlayerDataResponseData
{
    public int status_code;
    public string error_message;
    public List<PlayerMainData> player_data;
}

// Data structure for the update response from the server.
[System.Serializable]
public class UpdatePlayerResponse
{
    public int status_code;
    public string error_message;
}


/// <summary>
/// A static class to manage fetching, holding, and updating the current player's data.
/// It is designed to prevent concurrent data requests to avoid race conditions.
/// </summary>
public static class PlayerDataManager
{
    // Public static properties to hold player data and status flags.
    public static PlayerMainData CurrentPlayerMainData { get; private set; }
    public static bool IsDataLoaded { get; private set; } = false;
    public static string LastErrorMessage { get; private set; } = "";
    public static bool IsUpdateSuccessful { get; private set; } = false;
    public static string LastUpdateMessage { get; private set; } = "";

    // Flag to prevent concurrent fetch operations.
    private static bool IsDataLoading { get; set; } = false;

    /// <summary>
    /// Resets all player data and status flags. Called before a new fetch.
    /// </summary>
    public static void ResetPlayerData()
    {
        CurrentPlayerMainData = null;
        IsDataLoaded = false;
        LastErrorMessage = "";
        IsUpdateSuccessful = false;
        LastUpdateMessage = "";
        Debug.Log("PlayerDataManager: All player data has been reset.");
    }

    /// <summary>
    /// Coroutine to fetch player data from the server. Prevents new fetches if one is already running.
    /// </summary>
    public static IEnumerator FetchPlayerData(int playerId)
    {
        // Prevent race conditions by skipping if a fetch is already in progress.
        if (IsDataLoading)
        {
            Debug.LogWarning("PlayerDataManager: A fetch operation is already in progress. Skipping new request.");
            yield break;
        }

        IsDataLoading = true;

        try
        {
            ResetPlayerData();

            GetPlayerDataRequestData requestData = new GetPlayerDataRequestData { player_id = playerId };
            string jsonRequestBody = JsonUtility.ToJson(requestData);
            string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();

            if (string.IsNullOrEmpty(apiPath))
            {
                LastErrorMessage = "Failed to load server configuration.";
                Debug.LogError($"PlayerDataManager: {LastErrorMessage}");
                yield break; // Exit the coroutine.
            }

            string fullUrl = apiPath + "/get_player_data.php";
            Debug.Log($"PlayerDataManager: Sending request for player ID: {playerId}, Body: {jsonRequestBody}");

            using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    LastErrorMessage = $"API Call Failed! Error: {request.error}";
                    Debug.LogError($"PlayerDataManager: {LastErrorMessage}");
                }
                else
                {
                    Debug.Log($"PlayerDataManager: API Response: {request.downloadHandler.text}");
                    var response = JsonUtility.FromJson<GetPlayerDataResponseData>(request.downloadHandler.text);

                    if (response.status_code == 0 && response.player_data != null && response.player_data.Count > 0)
                    {
                        CurrentPlayerMainData = response.player_data[0];
                        IsDataLoaded = true;
                        Debug.Log($"PlayerDataManager: Successfully loaded data for player ID: {CurrentPlayerMainData.player_id}.");
                    }
                    else
                    {
                        LastErrorMessage = response.error_message ?? "No player data found in response.";
                        Debug.LogError($"PlayerDataManager: {LastErrorMessage}");
                    }
                }
            }
        }
        finally
        {
            // This block is crucial. It ensures IsDataLoading is always reset to false,
            // even if the web request fails or an error occurs.
            IsDataLoading = false;
            Debug.Log($"PlayerDataManager: Fetch coroutine finished. IsDataLoading is now {IsDataLoading}.");
        }
    }

    /// <summary>
    /// Coroutine to update player data on the server.
    /// </summary>
    public static IEnumerator UpdatePlayerDataOnServer(int playerId, int coins, int avatarSpriteId)
    {
        IsUpdateSuccessful = false;
        LastUpdateMessage = "";

        PlayerMainData updateData = new PlayerMainData
        {
            player_id = playerId,
            coin = coins,
            avatar_sprite_id = avatarSpriteId
        };
        string jsonRequestBody = JsonUtility.ToJson(updateData);
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/update_player_data.php";

        Debug.Log($"PlayerDataManager: Sending update request for player ID: {playerId}, Body: {jsonRequestBody}");

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                LastUpdateMessage = $"Update API Call Failed! Error: {request.error}";
                Debug.LogError($"PlayerDataManager: {LastUpdateMessage}");
            }
            else
            {
                var response = JsonUtility.FromJson<UpdatePlayerResponse>(request.downloadHandler.text);
                if (response.status_code == 0)
                {
                    IsUpdateSuccessful = true;
                    LastUpdateMessage = "Player data updated successfully.";
                    Debug.Log("PlayerDataManager: Player data updated successfully on server.");
                }
                else
                {
                    LastUpdateMessage = $"Server returned an error during update: {response.error_message}";
                    Debug.LogError($"PlayerDataManager: {LastUpdateMessage}");
                }
            }
        }
    }
}