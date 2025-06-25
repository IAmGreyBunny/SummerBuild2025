using UnityEngine;

public class ShopItemController : MonoBehaviour
{
    public ShopItem shopItem;

    public bool PurchaseItem()
    {
        if (PlayerDataManager.changeCurrentPlayerCoin(-(shopItem.item_price)))
        {
            if(shopItem.item_type == "consumable")
            {
                // Code to add into inventory
            }
            else if(shopItem.item_type == "pet")
            {
                // Code to create pet
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}

