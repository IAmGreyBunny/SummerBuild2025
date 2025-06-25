using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class PetDatabaseHelper
{
    public static IEnumerator InsertPetToDatabase(int owner_id, int pet_type)
    {
        PetJsonPayload  _PetJsonPayload = new PetJsonPayload()
        {
            owner_id = owner_id,
            pet_type = pet_type
        };

        string jsonRequestBody = JsonUtility.ToJson(_PetJsonPayload);
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/insert_pet.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Insertion Failed");
            }
            else
            {
                var response = JsonUtility.FromJson<InsertPetResponse>(request.downloadHandler.text);
                if (response.status_code == 0)
                {
                    Debug.Log("Insertion Succeeded");
                }
                else
                {
                    Debug.Log($"Insertion failed with: {response.error_message}");
                }
            }
        }
    }

    [Serializable]
    private class PetJsonPayload
    {
        public int owner_id;
        public int pet_type;
    }

    [Serializable]
    private class InsertPetResponse
    {
        public int status_code;
        public string error_message;
    }
}

