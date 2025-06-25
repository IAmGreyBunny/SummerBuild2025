using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System; // For Exception handling

/// <summary>
/// Manages sending player tracking data, like monthly income, to the server.
/// Follows the same JSON-based communication pattern as PetManager.
/// </summary>
public class AllowanceDataManager : MonoBehaviour
{
    // --- Singleton Instance ---
    public static AllowanceDataManager Instance { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // Standard singleton pattern to ensure only one instance exists.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: Uncomment if this needs to persist across scenes.
        }
    }

    /// <summary>
    /// Coroutine to update the player's monthly income in the database.
    /// </summary>
    /// <param name="playerId">The ID of the player to update.</param>
    /// <param name="monthlyIncome">The monthly income value.</param>
    public IEnumerator Co_UpdateMonthlyIncome(int playerId, float monthlyIncome)
    {
        // Create the request data object.
        var requestData = new UpdateTrackerRequestData
        {
            player_id = playerId,
            // Format the income to two decimal places right here.
            monthly_income = monthlyIncome
        };

        // Serialize the request data to a JSON string.
        string jsonRequestBody = JsonUtility.ToJson(requestData);

        // Get the full URL for the API endpoint.
        // I'm assuming a similar ServerConfig setup as your PetManager.
        // If not, you can replace this with your direct URL string.
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/update_player_tracker_setting.php";

        // Create and configure the UnityWebRequest for a POST request.
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            // Attach the JSON data to the request.
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"Sending JSON to {fullUrl}: {jsonRequestBody}");

            // Send the request and wait for a response.
            yield return request.SendWebRequest();

            // --- Handle Response ---
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Call Failed! Network Error: {request.error}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("API Call Successful! Response: " + jsonResponse);
                try
                {
                    // Deserialize the response to check for server-side status codes and messages.
                    UpdateTrackerResponseData responseDTO = JsonUtility.FromJson<UpdateTrackerResponseData>(jsonResponse);

                    // Check for a success status code from your server (e.g., "0" or "200").
                    if (responseDTO.status_code == "0" || responseDTO.status_code == "200")
                    {
                        Debug.Log("Server confirmed: Monthly income updated successfully!");
                        // You could invoke a UnityEvent here to notify other systems if needed.
                    }
                    else
                    {
                        // Handle server-side errors (e.g., "Invalid player ID").
                        Debug.LogError($"Server returned an error: {responseDTO.error_message}");
                    }
                }
                catch (Exception e)
                {
                    // This catches errors if the server response is not valid JSON.
                    Debug.LogError($"Failed to parse JSON response. Error: {e.Message}");
                }
            }
        }
    }

    #region Data Transfer Classes (DTOs)
    // --- Request Data Structure ---
    [System.Serializable]
    private class UpdateTrackerRequestData
    {
        public int player_id;
        public float monthly_income;
    }

    // --- Response Data Structure ---
    [System.Serializable]
    private class UpdateTrackerResponseData
    {
        // These fields should match the JSON keys your PHP script returns.
        public string status_code;
        public string error_message;
    }
    #endregion
}