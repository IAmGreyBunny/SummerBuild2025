using System;
using System.Collections;
using System.Collections.Generic; // Required for Dictionary and List
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Required for Image component
using System.IO; // Required for File.Exists and File.ReadAllText in ServerConfig context

public class ShopController : MonoBehaviour
{
    public GameObject shopItemCardPrefab;
    public Transform shopItemParent;

    [Header("Item Sprites")]
    [Tooltip("Map Item IDs to their corresponding Sprites in the Inspector.")]
    [SerializeField] private List<ItemSpriteMapping> itemSpriteMappings = new List<ItemSpriteMapping>();
    private Dictionary<int, Sprite> _itemSpritesDictionary = new Dictionary<int, Sprite>();

    [Tooltip("The relative path to the Image component within your shopItemCardPrefab. E.g., 'Box Body/ItemImage'")]
    [SerializeField] private string itemImageRelativePath = "Box Body/ItemImage"; // Default guess path

    // Backend URL dynamically loaded from ServerConfig
    private string _getShopItemsUrl;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        // Initialize the dictionary for efficient sprite lookup at the earliest possible stage.
        PopulateItemSpritesDictionary();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("ShopController Start called");
        StartCoroutine(InitializeShopAndFetchItems()); // Renamed for clarity on initialization flow
    }

    /// <summary>
    /// Populates the internal dictionary for quick sprite lookup based on configured mappings.
    /// </summary>
    private void PopulateItemSpritesDictionary()
    {
        _itemSpritesDictionary.Clear();
        if (itemSpriteMappings != null)
        {
            foreach (var mapping in itemSpriteMappings)
            {
                if (mapping.itemSprite != null)
                {
                    if (!_itemSpritesDictionary.ContainsKey(mapping.itemId))
                    {
                        _itemSpritesDictionary.Add(mapping.itemId, mapping.itemSprite);
                    }
                    else
                    {
                        Debug.LogWarning($"[ShopController] Duplicate sprite mapping found for Item ID: {mapping.itemId}. Overwriting existing sprite.");
                        _itemSpritesDictionary[mapping.itemId] = mapping.itemSprite;
                    }
                }
                else
                {
                    Debug.LogWarning($"[ShopController] Sprite is null for Item ID: {mapping.itemId}. It will not be displayed.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[ShopController] itemSpriteMappings list is not assigned or is empty. Shop item images may not appear.");
        }
        Debug.Log($"[ShopController] Populated sprite dictionary with {_itemSpritesDictionary.Count} entries.");
    }

    /// <summary>
    /// Initializes the shop by loading server config and then fetching shop items.
    /// </summary>
    private IEnumerator InitializeShopAndFetchItems()
    {
        // 1. Load Server Configuration synchronously using the existing ServerConfig.LoadFromFile() method.
        // This relies on your ServerConfig.cs having a public static ServerConfig LoadFromFile(string) method.
        ServerConfig loadedConfig = ServerConfig.LoadFromFile("Config/ServerConfig.json"); //
        if (loadedConfig == null)
        {
            Debug.LogError("[ShopController] Failed to load ServerConfig. Cannot proceed with fetching shop items. Ensure ServerConfig.json is in StreamingAssets/Config/ and ServerConfig.cs is correctly defined.");
            yield break;
        }

        // Construct full URL for get_shop_items.php using the loaded config's GetApiPath method.
        // This pattern matches how PlayerDataManager constructs its URLs.
        string apiBaseUrl = loadedConfig.GetApiPath(); //
        _getShopItemsUrl = apiBaseUrl + "/get_shop_items.php"; //

        Debug.Log($"[ShopController] Constructed Get Shop Items URL: {_getShopItemsUrl}");

        // 2. Call GetShopItems to fetch and display shop items
        yield return GetShopItems();
    }

    public void callGetShopItems()
    {
        Debug.Log("callGetShopItems called from public method. Re-fetching shop items.");
        // Clear existing items before re-fetching to prevent duplicates if this is called again.
        foreach (Transform child in shopItemParent)
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(GetShopItems());
    }

    IEnumerator GetShopItems()
    {
        Debug.Log("[ShopController] GetShopItems Coroutine started.");

        if (string.IsNullOrEmpty(_getShopItemsUrl))
        {
            Debug.LogError("[ShopController] Get Shop Items URL is not set. Cannot fetch items. Ensure InitializeShopAndFetchItems ran successfully.");
            yield break;
        }

        // Using UnityWebRequest.PostWwwForm as per your original code.
        // This sends a POST request with an empty body.
        UnityWebRequest request = UnityWebRequest.PostWwwForm(_getShopItemsUrl, ""); //
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[ShopController] Request error: {request.error}");
        }
        else
        {
            Debug.Log($"[ShopController] Raw JSON Response: {request.downloadHandler.text}");

            try
            {
                _GetShopItemsResponse getShopItemsResponse = JsonUtility.FromJson<_GetShopItemsResponse>(request.downloadHandler.text);

                if (getShopItemsResponse.status_code == 0) // Success status code from PHP
                {
                    Debug.Log($"[ShopController] Number of items fetched: {getShopItemsResponse.items.Length}");

                    foreach (ShopItem item in getShopItemsResponse.items) //
                    {
                        GameObject currentShopCard = Instantiate(shopItemCardPrefab, shopItemParent);
                        // Find the TextMeshPro component for the item name and price
                        currentShopCard.transform.Find("Box Body").Find("Label").GetComponent<TMP_Text>().text = $"{item.item_name} - ${item.item_price}"; //
                        currentShopCard.transform.Find("ShopItemController").GetComponent<ShopItemData>().shopItem = item;

                        // --- NEW: Set the item image ---
                        // Find the Image component using the specified relative path.
                        // Example path: "Box Body/ItemImage" if 'ItemImage' is a child of 'Box Body'.
                        Transform itemImageTransform = currentShopCard.transform.Find(itemImageRelativePath);
                        if (itemImageTransform != null)
                        {
                            Image itemImage = itemImageTransform.GetComponent<Image>();
                            if (itemImage != null)
                            {
                                // Try to get the sprite from the dictionary using the item_id
                                if (_itemSpritesDictionary.TryGetValue(item.item_id, out Sprite itemSprite)) //
                                {
                                    itemImage.sprite = itemSprite;
                                    Debug.Log($"[ShopController] Set sprite for item ID: {item.item_id} ({item.item_name})"); //
                                }
                                else
                                {
                                    Debug.LogWarning($"[ShopController] No sprite found for item ID: {item.item_id} ({item.item_name}). Please add it to 'Item Sprite Mappings' in the Inspector. Using default or leaving blank."); //
                                    // Optionally set a default sprite here: itemImage.sprite = defaultSprite;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[ShopController] No Image component found on GameObject at path '{itemImageRelativePath}' within shopItemCardPrefab. Cannot set item image.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[ShopController] Child object at path '{itemImageRelativePath}' not found in shopItemCardPrefab. Cannot set item image.");
                        }
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
