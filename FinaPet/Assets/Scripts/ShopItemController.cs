using UnityEngine;

public class ShopItemController : MonoBehaviour
{
    public ShopItem shopItem;

    public void PurchaseItem()
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
        }
        else
        {
            // Code to show error or watever
        }
    }
}

