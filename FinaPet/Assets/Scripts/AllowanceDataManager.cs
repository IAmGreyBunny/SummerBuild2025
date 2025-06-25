using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Globalization;

/// <summary>
/// Manages GETTING and UPDATING player allowance data from the server.
/// </summary>
public class AllowanceDataManager : MonoBehaviour
{
    public static AllowanceDataManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public IEnumerator Co_GetMonthlyIncome(int playerId, Action<float> onComplete)
    {
        var requestData = new GetTrackerRequestData { player_id = playerId };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/get_player_tracker_setting.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            request.timeout = 10;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[DataManager] API Call Failed! Error: {request.error}");
                onComplete?.Invoke(0f);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"[DataManager] Attempting to parse response: {jsonResponse}");

                // --- MANUAL PARSING LOGIC ---
                try
                {
                    // Define the key we are looking for in the text
                    string key = "\"monthly_income\":\"";
                    int keyIndex = jsonResponse.IndexOf(key);

                    if (keyIndex != -1)
                    {
                        // The key was found. Now find where the value starts and ends.
                        int valueStartIndex = keyIndex + key.Length;
                        int valueEndIndex = jsonResponse.IndexOf('"', valueStartIndex);

                        if (valueEndIndex != -1)
                        {
                            // Extract the value substring (e.g., "200.00")
                            string incomeString = jsonResponse.Substring(valueStartIndex, valueEndIndex - valueStartIndex);

                            // Parse the extracted string into a float
                            if (float.TryParse(incomeString, NumberStyles.Any, CultureInfo.InvariantCulture, out float income))
                            {
                                Debug.Log($"[DataManager] Manual parse SUCCESS. Found income: {income}");
                                onComplete?.Invoke(income);
                                yield break; // Exit because we succeeded
                            }
                        }
                    }

                    // If we get here, it means the manual search failed.
                    Debug.LogError("[DataManager] Manual search for 'monthly_income' key in JSON response failed. The response may not contain the expected data.");
                    onComplete?.Invoke(0f);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[DataManager] An exception occurred during manual parsing. Error: {e.Message}");
                    onComplete?.Invoke(0f);
                }
            }
        }
    }

    // Update function remains the same
    public IEnumerator Co_UpdateMonthlyIncome(int playerId, float monthlyIncome)
    {
        var requestData = new UpdateTrackerRequestData { player_id = playerId, monthly_income = monthlyIncome };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/update_player_tracker_setting.php";
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            request.timeout = 10;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) { Debug.LogError($"[DataManager] Update API Call Failed! Error: {request.error}"); }
            else { Debug.Log("[DataManager] Update API Call Successful!"); }
        }
    }

    #region Data Transfer Classes
    // We no longer use these specific DTOs for parsing the GET response, but they are kept for the POST request.
    [Serializable] private class GetTrackerRequestData { public int player_id; }
    [Serializable] private class UpdateTrackerRequestData { public int player_id; public float monthly_income; }
    #endregion
}
