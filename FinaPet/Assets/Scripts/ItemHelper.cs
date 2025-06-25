using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public static class ItemHelper
{
    public static Dictionary<int,string> itemIdNameMapping = new Dictionary<int, string>();
    public static IEnumerator GetItemNames()
    {
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        string fullUrl = apiPath + "/get_shop_items.php";
        // Using UnityWebRequest.PostWwwForm as per your original code.
        // This sends a POST request with an empty body.
        UnityWebRequest request = UnityWebRequest.PostWwwForm(fullUrl, ""); //
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($" Request error: {request.error}");
        }
        else
        {
            Debug.Log($"Raw JSON Response: {request.downloadHandler.text}");

            try
            {
                _GetShopItemsResponse getShopItemsResponse = JsonUtility.FromJson<_GetShopItemsResponse>(request.downloadHandler.text);

                if (getShopItemsResponse.status_code == 0) // Success status code from PHP
                {
                    foreach (ShopItem item in getShopItemsResponse.items) //
                    {
                        itemIdNameMapping.Add(item.item_id, item.item_name);
                    }
                }
                else
                {
                    Debug.LogWarning($"Get shop item Failed with status code: {getShopItemsResponse.status_code}, message: {getShopItemsResponse.error_message}"); //
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ShopController] Failed to parse shop items JSON: {e.Message}\nRaw Response: {request.downloadHandler.text}");
            }
        }
    }

    public static string getItemNameFromId(int item_id)
    {
        return itemIdNameMapping[item_id];
    }

    /// <summary>
    /// Helper class to map Item IDs to their Sprites in the Unity Inspector.
    /// </summary>
    [Serializable]
    private class ItemSpriteMapping
    {
        public int itemId;
        public Sprite itemSprite;
    }

    // --- Backend Response Data Structures (These should match your PHP script output) ---
    [Serializable]
    private class _GetShopItemsResponse
    {
        public int status_code; //
        public string error_message; //
        public ShopItem[] items; //
    }
}
