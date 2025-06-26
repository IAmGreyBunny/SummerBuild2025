using UnityEngine;


[System.Serializable]
public class InventoryItem
{
    public int item_id; //
    public int quantity; // Assumed field in 'inventory' table for item count
    public int inventory_id;
    public int player_id;
}