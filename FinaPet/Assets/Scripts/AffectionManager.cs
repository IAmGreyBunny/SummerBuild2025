using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

/// <summary>
/// Manages the pet "petting" interaction. This includes updating the UI
/// and sending the new affection value to the server.
/// </summary>
public class AffectionManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("A reference to the AttentionManager script which controls the affection slider.")]
    public AttentionManager attentionManager;

    [Header("Petting Settings")]
    [Tooltip("How many affection points are restored per pet.")]
    public int affectionPointsPerPet = 15;

    // --- Private State ---
    private bool _isPetting = false; // Prevents spamming the pet button

    /// <summary>
    /// PUBLIC method to be called by your "Pet" button's OnClick event.
    /// </summary>
    public void OnPetButtonClicked()
    {
        // 1. Pre-computation Checks
        if (_isPetting)
        {
            Debug.LogWarning("AffectionManager: Already processing a pet action.");
            return;
        }

        if (attentionManager.IsAffectionFull())
        {
            Debug.Log("AffectionManager: Pet's affection is already full.");
            return;
        }

        // 2. Start the Petting Process
        StartCoroutine(Co_PetProcess());
    }

    /// <summary>
    /// The master coroutine that handles the entire petting sequence.
    /// </summary>
    private IEnumerator Co_PetProcess()
    {
        _isPetting = true; // Lock the process

        // 3. Local Calculations
        int newAffection = attentionManager.GetCurrentAffection() + affectionPointsPerPet;

        // 4. Optimistic UI Update
        attentionManager.SetAffection(newAffection);

        // 5. Send Update to the Server
        yield return StartCoroutine(Co_UpdateAffectionOnServer(newAffection));

        _isPetting = false; // Unlock the process
    }

    /// <summary>
    /// Coroutine that sends the updated affection stat to the PHP backend.
    /// </summary>
    private IEnumerator Co_UpdateAffectionOnServer(int newAffection)
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("AffectionManager: Cannot update server, GameDataManager has no selected pet.");
            _isPetting = false;
            yield break;
        }

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json")?.GetApiPath();
        if (string.IsNullOrEmpty(apiPath))
        {
            Debug.LogError("AffectionManager: Cannot update server, failed to load API path.");
            yield break;
        }

        string url = apiPath + "/update_pet_affection.php";
        var requestData = new UpdatePetAffectionRequest
        {
            pet_id = GameDataManager.Instance.selectedPet.pet_id,
            affection = newAffection
        };
        string json = JsonUtility.ToJson(requestData);

        Debug.Log($"AffectionManager: Sending affection update to {url} with body: {json}");

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"AffectionManager: Error sending request: {request.error}");
            }
            else
            {
                Debug.Log($"AffectionManager: Successfully received response. Response: {request.downloadHandler.text}");
            }
        }
    }

    #region Data Transfer Classes
    [Serializable]
    private class UpdatePetAffectionRequest
    {
        public int pet_id;
        public int affection;
    }
    #endregion
}
