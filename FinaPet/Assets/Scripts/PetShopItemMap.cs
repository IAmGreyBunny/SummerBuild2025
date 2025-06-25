using System.Collections.Generic;
using UnityEngine;

public class PetShopItemMap
{
    //<item_id,pet_type>
    Dictionary<int, int> petShopDictionary = new Dictionary<int, int>
    {
        [2] = 0,
        [3] = 1,
        [4] = 2
    };
    

    public int GetPetTypeFromItemId(int itemId)
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
