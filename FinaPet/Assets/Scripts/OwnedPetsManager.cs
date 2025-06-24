using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting; // Required for encoding the JSON string

/// <summary>
/// Manages fetching pet data from the server and spawning them using the PetSpawner.
/// </summary>
public class OwnedPetsManager : MonoBehaviour
{
    public PetSpawner petSpawner;
    public int ownerId = 1; // Example ID. This should be set dynamically after a player logs in.

    // This is called when the script instance is being loaded.
    void Start()
    {
        // Automatically fetch and spawn pets when the scene starts.
        if(PlayerAuthSession.IsLoggedIn == true)
        {
            Debug.Log("Player Logged in: " + PlayerAuthSession.PlayerId);
            ownerId = PlayerAuthSession.PlayerId;
        }
        else
        {
            Debug.LogError("Player not logged in using default: " + ownerId);
        }

        FetchAndSpawnPlayerPets();
    }

    /// <summary>
    /// Public method to initiate the process. Can be called from a UI button or other scripts.
    /// </summary>
    public void FetchAndSpawnPlayerPets()
    {
        StartCoroutine(Co_FetchAndSpawnPets());
    }

    /// <summary>
    /// The coroutine that handles the web request to get pet data.
    /// </summary>
    private IEnumerator Co_FetchAndSpawnPets()
    {
        // --- 1. Sanity Checks ---
        if (petSpawner == null)
        {
            Debug.LogError("PetSpawner is not assigned in the OwnedPetsManager Inspector!");
            yield break; // Stop the coroutine
        }

        // --- 2. Prepare the JSON Request ---
        // Create a request object to be converted to JSON.
        GetPetsRequestData requestData = new GetPetsRequestData
        {
            owner_id = this.ownerId
        };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        Debug.Log("Sending Request: " + jsonRequestBody);

        // --- 3. Create and Send the UnityWebRequest ---
        // Get the API path from your server configuration file.
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/get_pets.php";

        // Using POST, as the PHP script reads from `php://input`
        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            // Set the body of the request
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set the content type header to specify we're sending JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            // --- 4. Handle the Response ---
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Call Failed! Error: " + request.error);
            }
            else
            {
                Debug.Log("API Call Successful! Response: " + request.downloadHandler.text);
                GetPetsResponseData response = JsonUtility.FromJson<GetPetsResponseData>(request.downloadHandler.text);

                if (response.status_code == 0)
                {
                    Debug.Log($"Successfully fetched {response.pets.Count} pet(s) for owner ID {ownerId}.");
                    // Loop through each pet in the response and spawn it
                    foreach (var pet in response.pets)
                    {
                        // Use the existing PetSpawner to create the pet by its type/index
                        Debug.Log($"Spawning pet of type: {pet.pet_type}");
                        petSpawner.SetAndSpawnPet(pet.pet_type);
                    }
                }
                else
                {
                    // Handle server-side errors (e.g., owner not found)
                    Debug.LogError("API returned an error: " + response.error_message);
                }
            }
        }
    }

    // --- Helper classes to match the JSON structure ---

    [System.Serializable]
    private class GetPetsRequestData
    {
        public int owner_id;
    }

    [System.Serializable]
    private class GetPetsResponseData
    {
        public int status_code;
        public string error_message;
        public List<PetData> pets;
    }

    [System.Serializable]
    private class PetData
    {
        public int pet_id;
        public int owner_id;
        public int pet_type; // This is the crucial field we need!
        public int hunger;
        public int affection;
    }
}