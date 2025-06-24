using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System;

/// <summary>
/// Fetches pet data from the server, handles server-specific data formats,
/// and populates the clean data into the GameDataManager.
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

        if (PlayerAuthSession.IsLoggedIn)
        {
            ownerId = PlayerAuthSession.PlayerId;
        }
        else
        {
            Debug.LogError("Player not logged in, using default ID: " + ownerId);
        }

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
                    // --- FIX ---
                    // Parse the JSON into our DTO that expects strings.
                    GetPetsResponseData_DTO responseDTO = JsonUtility.FromJson<GetPetsResponseData_DTO>(request.downloadHandler.text);

                    // Check the string status code
                    if (responseDTO.status_code == "0" || responseDTO.status_code == "200")
                    {
                        // Create a new, clean list for our final PetData
                        List<PetData> cleanPetsList = new List<PetData>();

                        // Loop through each pet from the server
                        foreach (var petDTO in responseDTO.pets)
                        {
                            // Convert the string data into a clean PetData object with integers
                            PetData cleanPet = new PetData
                            {
                                pet_id = int.Parse(petDTO.pet_id),
                                owner_id = int.Parse(petDTO.owner_id),
                                pet_type = int.Parse(petDTO.pet_type),
                                hunger = int.Parse(petDTO.hunger),
                                affection = int.Parse(petDTO.affection)
                            };
                            cleanPetsList.Add(cleanPet);
                        }

                        // Store the final, clean list in the GameDataManager
                        GameDataManager.Instance.ownerPets = cleanPetsList;

                        Debug.Log("Pet data parsed and updated successfully. Invoking OnPetsUpdated event.");
                        OnPetsUpdated.Invoke();
                    }
                    else
                    {
                        Debug.LogError("Server returned an error: " + responseDTO.error_message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response. Is the format correct? Error: {e.Message}");
                }
            }
        }
    }

    #region Data Transfer Classes
    // --- FIX ---
    // These new DTO (Data Transfer Object) classes match your server's JSON exactly (with strings).
    [System.Serializable]
    private class PetData_DTO
    {
        public string pet_id;
        public string owner_id;
        public string pet_type;
        public string hunger;
        public string affection;
        // The DTO can also include fields you don't use, like last_fed, without causing issues.
        public string last_fed;
    }

    [System.Serializable]
    private class GetPetsResponseData_DTO
    {
        public string status_code;
        public string error_message;
        public List<PetData_DTO> pets;
    }

    // This is the original request class, it's still correct.
    [System.Serializable]
    private class GetPetsRequestData { public int owner_id; }
    #endregion
}