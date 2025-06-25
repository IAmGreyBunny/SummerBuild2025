using System;
using System.Collections;
using System.Collections.Generic; // Required for Dictionary and List
using System.IO; // Required for File.Exists and File.ReadAllText in ServerConfig context
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Required for Image component

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryItemCardPrefab;
    public Transform inventoryItemParent;

    [Header("Item Sprites")]
    [Tooltip("Map Item IDs to their corresponding Sprites in the Inspector.")]
    [SerializeField] private List<ItemSpriteMapping> itemSpriteMappings = new List<ItemSpriteMapping>();
    private Dictionary<int, Sprite> _itemSpritesDictionary = new Dictionary<int, Sprite>();

    [Tooltip("The relative path to the Image component within your inventoryItemCardPrefab. E.g., 'Box Body/ItemImage'")]
    [SerializeField] private string itemImageRelativePath = "Box Body/Image"; // Default guess path

    // Backend URL dynamically loaded from ServerConfig
    private string _getPlayerInventoryUrl;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        // Initialize the dictionary for efficient sprite lookup at the earliest possible stage.
        PopulateItemSpritesDictionary();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("InventoryController Start called");
        StartCoroutine(InitializeInventoryAndFetchItems()); // Renamed for clarity on initialization flow
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
                        Debug.LogWarning($"[InventoryController] Duplicate sprite mapping found for Item ID: {mapping.itemId}. Overwriting existing sprite.");
                        _itemSpritesDictionary[mapping.itemId] = mapping.itemSprite;
                    }
                }
                else
                {
                    Debug.LogWarning($"[InventoryController] Sprite is null for Item ID: {mapping.itemId}. It will not be displayed.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[InventoryController] itemSpriteMappings list is not assigned or is empty. inventory item images may not appear.");
        }
        Debug.Log($"[InventoryController] Populated sprite dictionary with {_itemSpritesDictionary.Count} entries.");
    }

    /// <summary>
    /// Initializes the inventory by loading server config and then fetching inventory items.
    /// </summary>
    private IEnumerator InitializeInventoryAndFetchItems()
    {
        // 1. Load Server Configuration synchronously using the existing ServerConfig.LoadFromFile() method.
        // This relies on your ServerConfig.cs having a public static ServerConfig LoadFromFile(string) method.
        ServerConfig loadedConfig = ServerConfig.LoadFromFile("Config/ServerConfig.json"); //
        if (loadedConfig == null)
        {
            Debug.LogError("[InventoryController] Failed to load ServerConfig. Cannot proceed with fetching inventory items. Ensure ServerConfig.json is in StreamingAssets/Config/ and ServerConfig.cs is correctly defined.");
            yield break;
        }

        // Construct full URL for get_shop_items.php using the loaded config's GetApiPath method.
        // This pattern matches how PlayerDataManager constructs its URLs.
        string apiBaseUrl = loadedConfig.GetApiPath(); //
        _getPlayerInventoryUrl = apiBaseUrl + "/get_player_inventory.php"; //

        Debug.Log($"[InventoryController] Constructed Get Player Inventory URL: {_getPlayerInventoryUrl}");

        // 2. Call GetShopItems to fetch and display inventory items
        yield return GetInventoryItems();
    }

    public void callGetInventoryItems()
    {
        Debug.Log("callGetInventoryItems called from public method. Re-fetching inventory items.");
        // Clear existing items before re-fetching to prevent duplicates if this is called again.
        foreach (Transform child in inventoryItemParent)
        {
            Destroy(child.gameObject);
        }
        StartCoroutine(GetInventoryItems());
    }

    IEnumerator GetInventoryItems()
    {
        Debug.Log("GetInventoryItems Coroutine started.");
        StartCoroutine(ItemHelper.GetItemNames());

        if (string.IsNullOrEmpty(_getPlayerInventoryUrl))
        {
            Debug.LogError("[InventoryController] Get Inventory Items URL is not set. Cannot fetch items. Ensure InitializeShopAndFetchItems ran successfully.");
            yield break;
        }

        GetInventoryJsonPayload _GetInventoryJsonPayload = new GetInventoryJsonPayload()
        {
            player_id = PlayerAuthSession.PlayerId
        };

        string jsonRequestBody = JsonUtility.ToJson(_GetInventoryJsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(_getPlayerInventoryUrl, "POST"))
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
                var response = JsonUtility.FromJson<_GetInventoryItemsResponse>(request.downloadHandler.text);
                if (response.status_code == 0)
                {
                    Debug.Log("Update Succeeded");
                    try
                    {
                        _GetInventoryItemsResponse getInventoryItemsResponse = JsonUtility.FromJson<_GetInventoryItemsResponse>(request.downloadHandler.text);

                        if (getInventoryItemsResponse.status_code == 0) // Success status code from PHP
                        {
                            Debug.Log($"Number of items fetched: {getInventoryItemsResponse.items.Length}");

                            foreach (InventoryItem item in getInventoryItemsResponse.items) //
                            {
                                GameObject currentInventoryCard = Instantiate(inventoryItemCardPrefab, inventoryItemParent);
                                // Find the TextMeshPro component for the item name and price
                                currentInventoryCard.transform.Find("Box Body").Find("Label").GetComponent<TMP_Text>().text = $"{ItemHelper.getItemNameFromId(item.item_id)} \n Quantity: {item.quantity}";

                                // --- NEW: Set the item image ---
                                // Find the Image component using the specified relative path.
                                // Example path: "Box Body/ItemImage" if 'ItemImage' is a child of 'Box Body'.
                                Transform itemImageTransform = currentInventoryCard.transform.Find(itemImageRelativePath);
                                if (itemImageTransform != null)
                                {
                                    Image itemImage = itemImageTransform.GetComponent<Image>();
                                    if (itemImage != null)
                                    {
                                        // Try to get the sprite from the dictionary using the item_id
                                        if (_itemSpritesDictionary.TryGetValue(item.item_id, out Sprite itemSprite)) //
                                        {
                                            itemImage.sprite = itemSprite;
                                            Debug.Log($"[InventoryController] Set sprite for item ID: {item.item_id}"); //
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[InventoryController] No sprite found for item ID: {item.item_id}. Please add it to 'Item Sprite Mappings' in the Inspector. Using default or leaving blank."); //
                                                                                                                                                                                                                               // Optionally set a default sprite here: itemImage.sprite = defaultSprite;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"[InventoryController] No Image component found on GameObject at path '{itemImageRelativePath}' within inventoryItemCardPrefab. Cannot set item image.");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning($"[InventoryController] Child object at path '{itemImageRelativePath}' not found in inventoryItemCardPrefab. Cannot set item image.");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Get inventory item Failed with status code: {getInventoryItemsResponse.status_code}, message: {getInventoryItemsResponse.error_message}"); //
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[InventoryController] Failed to parse inventory items JSON: {e.Message}\nRaw Response: {request.downloadHandler.text}");
                    }
                }
                else
                {
                    Debug.Log($"Update failed with: {response.error_message}");
                }
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
    private class _GetInventoryItemsResponse
    {
        public int status_code; //
        public string error_message; //
        public InventoryItem[] items; //
    }

    [Serializable]
    private class GetInventoryJsonPayload
    {
        public int player_id;
    }


}
