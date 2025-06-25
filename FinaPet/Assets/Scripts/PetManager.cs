using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System;

/// <summary>
/// Fetches pet data from the server and populates the GameDataManager.
/// </summary>
public class PetManager : MonoBehaviour
{
    public int ownerId = 1;
    public UnityEvent OnPetsUpdated = new UnityEvent();

    void Start()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("FATAL ERROR: GameDataManager is not present in the scene.");
            return;
        }

        if (PlayerAuthSession.IsLoggedIn) { ownerId = PlayerAuthSession.PlayerId; }
        else { Debug.LogError("Player not logged in, using default ID: " + ownerId); }

        StartCoroutine(Co_FetchPets());
    }

    private IEnumerator Co_FetchPets()
    {
        var requestData = new GetPetsRequestData { owner_id = this.ownerId };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/get_pets.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Call Failed! Error: {request.error}");
            }
            else
            {
                Debug.Log("API Call Successful! Response: " + request.downloadHandler.text);
                try
                {
                    GetPetsResponseData_DTO responseDTO = JsonUtility.FromJson<GetPetsResponseData_DTO>(request.downloadHandler.text);
                    if (responseDTO.status_code == "0" || responseDTO.status_code == "200")
                    {
                        List<PetData> cleanPetsList = new List<PetData>();
                        foreach (var petDTO in responseDTO.pets)
                        {
                            cleanPetsList.Add(new PetData
                            {
                                pet_id = int.Parse(petDTO.pet_id),
                                owner_id = int.Parse(petDTO.owner_id),
                                pet_type = int.Parse(petDTO.pet_type),
                                hunger = int.Parse(petDTO.hunger),
                                affection = int.Parse(petDTO.affection)
                            });
                        }
                        GameDataManager.Instance.ownerPets = cleanPetsList;
                        OnPetsUpdated.Invoke();
                    }
                    else
                    {
                        Debug.LogError("Server returned an error: " + responseDTO.error_message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response. Error: {e.Message}");
                }
            }
        }
    }
    #region Data Transfer Classes
    [System.Serializable]
    private class PetData_DTO { public string pet_id, owner_id, pet_type, hunger, affection, last_fed; }
    [System.Serializable]
    private class GetPetsResponseData_DTO { public string status_code, error_message; public List<PetData_DTO> pets; }
    [System.Serializable]
    private class GetPetsRequestData { public int owner_id; }
    #endregion
}
