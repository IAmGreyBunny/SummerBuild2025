using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class InventoryHelper
{
    public static IEnumerator UpdateInventoryItem(int player_id, int item_id, int quantity)
    {
        InventoryJsonPayload _InventoryJsonPayload = new InventoryJsonPayload()
        {
            player_id = player_id,
            item_id = item_id,
            quantity = quantity
        };

        string jsonRequestBody = JsonUtility.ToJson(_InventoryJsonPayload);
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/update_inventory_item.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Update Failed");
            }
            else
            {
                var response = JsonUtility.FromJson<InsertPetResponse>(request.downloadHandler.text);
                if (response.status_code == 0)
                {
                    Debug.Log("Update Succeeded");
                }
                else
                {
                    Debug.Log($"Update failed with: {response.error_message}");
                }
            }
        }
    }

    [Serializable]
    private class InventoryJsonPayload
{
        public int player_id;
        public int item_id;
        public int quantity;
    }

    [Serializable]
    private class InsertPetResponse
    {
        public int status_code;
        public string error_message;
    }
}
