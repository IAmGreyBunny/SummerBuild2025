using UnityEngine;

public class ShopItemController : MonoBehaviour
{
    public ShopItem shopItem;

    public void PurchaseItem()
    {
        if (PlayerDataManager.CurrentPlayerMainData.coin - shopItem.item_price >= 0)
        {
            PlayerDataManager.CurrentPlayerMainData.coin -= shopItem.item_price;
            if (shopItem.item_type == "consumable")
            {
                // Code to add into inventory
                Debug.Log("Consumable purchased");
                int player_id = PlayerAuthSession.PlayerId;
                int item_id = shopItem.item_id;
                int item_quantity = 1;
                StartCoroutine(InventoryHelper.UpdateInventoryItem(player_id,item_id,item_quantity));
            }
            else if(shopItem.item_type == "pet")
            {
                // Code to create pet
                Debug.Log("Pet purchased");
                int pet_type = PetShopItemMap.GetPetTypeFromItemId(shopItem.item_id);
                int owner_id = PlayerAuthSession.PlayerId;
                StartCoroutine(PetDatabaseHelper.InsertPetToDatabase(owner_id,pet_type));
            }
            StartCoroutine(PlayerDataManager.UpdatePlayerDataOnServer(
                PlayerDataManager.CurrentPlayerMainData.player_id,
                PlayerDataManager.CurrentPlayerMainData.coin,
                PlayerDataManager.CurrentPlayerMainData.avatar_sprite_id
                ));
        }
        else
        {
            Debug.Log("Unable to purchase anything");
        }
    }
}

