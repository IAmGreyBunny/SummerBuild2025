using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ShopController : MonoBehaviour
{
    public GameObject shopItemCardPrefab;
    public Transform shopItemParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("ShopController Start called");
        callGetShopItems();
    }


    public void callGetShopItems()
    {
        Debug.Log("GetShopItems called from public method");
        StartCoroutine(GetShopItems());
    }
    IEnumerator GetShopItems()
    {

        Debug.Log("GetShopItems called");
        // Ensure ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() returns a valid URL
        // For demonstration, let's assume a placeholder if ServerConfig is not available
        string apiPath = "";
        try
        {
            apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load ServerConfig: " + e.Message + ". Using placeholder API path.");
            // Fallback for demonstration if ServerConfig isn't properly set up
            apiPath = "http://localhost:80/your_game_api";
        }

        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.PostWwwForm(apiPath + "/get_shop_items.php", "");
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request error: " + request.error);
        }
        else
        {
            // First, print the raw JSON response to see exactly what you're getting from the server
            Debug.Log("Raw JSON Response: " + request.downloadHandler.text);

            _GetShopItemsResponse getShopItemsResponse = JsonUtility.FromJson<_GetShopItemsResponse>(request.downloadHandler.text);

            if (getShopItemsResponse.status_code == 0)
            {
                Debug.Log("Number of items fetched: " + getShopItemsResponse.items.Length);

                // Iterate through the items array and print each item's details
                foreach (_ShopItem item in getShopItemsResponse.items)
                {
                    GameObject currentShopCard = Instantiate(shopItemCardPrefab, shopItemParent);
                    currentShopCard.transform.Find("Box Body").Find("Label").GetComponent<TMP_Text>().text = item.item_name;
                }
            }
            else
            {
                Debug.LogWarning("Get shop item Failed with status code: " + getShopItemsResponse.status_code + ", message: " + getShopItemsResponse.error_message);
            }
        }
    }

    [Serializable]
    private class _GetShopItemsResponse
    {
        public int status_code;
        public string error_message;
        public _ShopItem[] items;
    }

    [Serializable]
    private class _ShopItem
    {
        public int item_id;
        public string item_name;
    }
}