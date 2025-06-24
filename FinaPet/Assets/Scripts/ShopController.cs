using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ShopController : MonoBehaviour
{
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
        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.PostWwwForm(apiPath + "/get_shop_items.php", "");
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request error: " + request.error);
        }
        else
        {
            _GetShopItemsResponse getShopItemsResponse = JsonUtility.FromJson<_GetShopItemsResponse>(request.downloadHandler.text);

            if (getShopItemsResponse.status_code == 0)
            {
                Debug.Log(getShopItemsResponse.items.Length);
            }
            else
            {
                Debug.Log("Get shop item Failed with message: " + getShopItemsResponse.error_message);
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
