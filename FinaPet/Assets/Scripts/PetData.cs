using UnityEngine;

[System.Serializable]
public class PetData
{
    public int pet_id;
    public int owner_id;
    public int pet_type; // This is the crucial field we need!
    public int hunger;
    public int affection;
}
