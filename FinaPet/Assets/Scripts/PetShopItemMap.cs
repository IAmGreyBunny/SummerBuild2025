using System.Collections.Generic;
using UnityEngine;

public class PetShopItemMap
{
    //<item_id,pet_type>
    static Dictionary<int, int> petShopDictionary = new Dictionary<int, int>
    {
        [2] = 0,
        [3] = 1,
        [4] = 2
    };
    

    public static int GetPetTypeFromItemId(int itemId)
    {
        if(petShopDictionary.ContainsKey(itemId))
        {
            return petShopDictionary[itemId];
        }
        else
        {
            return -1;
        }
        
    }

}
