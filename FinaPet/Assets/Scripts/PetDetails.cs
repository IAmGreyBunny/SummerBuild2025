using UnityEngine;

/// <summary>
/// This script holds the specific data (like hunger and affection) for an individual pet instance.
/// Attach this script to your actual Pet Prefab.
/// </summary>
public class PetDetails : MonoBehaviour
{
    public int petId;
    public int ownerId;
    public int petType; // Corresponds to the index in SpriteLibraryAssets
    public int currentHunger;
    public int currentAffection;

    /// <summary>
    /// Initializes the pet's details with data fetched from the server.
    /// </summary>
    /// <param name="data">The PetData object containing server-fetched details.</param>
    public void Initialize(OwnedPetsManager.PetData data)
    {
        petId = data.pet_id;
        ownerId = data.owner_id;
        petType = data.pet_type;
        currentHunger = data.hunger;
        currentAffection = data.affection;

        Debug.Log($"Pet '{gameObject.name}' initialized: ID={petId}, Type={petType}, Hunger={currentHunger}, Affection={currentAffection}");
    }

    // You can add other methods here later to, for example, display these stats,
    // update them based on interactions, or save them back to the server.
}