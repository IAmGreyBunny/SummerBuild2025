using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;

/// <summary>
/// This script handles the spawning of a pet prefab and allows for changing its appearance
/// by swapping its SpriteLibraryAsset.
/// </summary>
public class PetSpawner : MonoBehaviour
{
    [Header("Pet Prefab")]
    [Tooltip("The GameObject prefab for your pet. This prefab must have a SpriteLibrary component and a PetDetails component.")]
    public GameObject petPrefab;

    [Header("Sprite Libraries")]
    [Tooltip("A list of all the different Sprite Library Assets for your pets.")]
    public List<SpriteLibraryAsset> petSpriteLibraries;

    [Header("Spawn Settings")]
    [Tooltip("The position where the pet will be spawned.")]
    public Vector3 spawnPosition = Vector3.zero;

    // A list to keep track of all currently spawned pets
    private List<GameObject> spawnedPets = new List<GameObject>();
    private int currentPetIndex = 0; // This will now typically be set by SetAndSpawnPet or SpawnPetWithData

    /// <summary>
    /// Spawns the pet prefab with the provided pet data.
    /// This is the primary method to use when spawning pets with specific server-fetched data.
    /// </summary>
    /// <param name="petData">The full PetData object fetched from the server.</param>
    public void SpawnPetWithData(OwnedPetsManager.PetData petData)
    {
        // Check if the prefab and libraries are assigned to prevent errors.
        if (petPrefab == null)
        {
            Debug.LogError("Pet Prefab is not assigned in the PetSpawner!");
            return;
        }

        if (petSpriteLibraries == null || petSpriteLibraries.Count == 0)
        {
            Debug.LogError("No Sprite Library Assets have been assigned in the PetSpawner!");
            return;
        }

        // Ensure the petData.pet_type is a valid index for our sprite libraries
        if (petData.pet_type < 0 || petData.pet_type >= petSpriteLibraries.Count)
        {
            Debug.LogError($"Invalid pet_type '{petData.pet_type}' received from server. No corresponding SpriteLibraryAsset found.");
            return;
        }

        currentPetIndex = petData.pet_type; // Update the internal index based on server data

        // Instantiate the pet prefab.
        GameObject newPet = Instantiate(petPrefab, spawnPosition, Quaternion.identity);
        newPet.name = "Pet_" + petSpriteLibraries[currentPetIndex].name + "_ID" + petData.pet_id; // Naming for easier debugging

        // Add the newly spawned pet to our tracking list
        spawnedPets.Add(newPet);

        // Get the SpriteLibrary component from the new pet instance and assign the correct asset.
        SpriteLibrary spriteLibrary = newPet.GetComponent<SpriteLibrary>();
        if (spriteLibrary != null)
        {
            spriteLibrary.spriteLibraryAsset = petSpriteLibraries[currentPetIndex];
        }
        else
        {
            Debug.LogError("The assigned Pet Prefab does not have a SpriteLibrary component!");
        }

        // Get the PetDetails component and initialize it with the fetched data.
        PetDetails petDetails = newPet.GetComponent<PetDetails>();
        if (petDetails != null)
        {
            petDetails.Initialize(petData);
        }
        else
        {
            Debug.LogError("The assigned Pet Prefab does not have a PetDetails component! Please add PetDetails.cs to your pet prefab.");
        }
    }

    /// <summary>
    /// Destroys all pets currently spawned by this spawner.
    /// Call this before spawning a new set of pets.
    /// </summary>
    public void ClearAllSpawnedPets()
    {
        foreach (GameObject pet in spawnedPets)
        {
            if (pet != null)
            {
                Destroy(pet);
            }
        }
        spawnedPets.Clear(); // Clear the list after destroying the GameObjects
        Debug.Log("All previously spawned pets cleared.");
    }


    /// <summary>
    /// Spawns the pet prefab with the initial sprite library (the first in the list).
    /// This method is primarily for initial setup or when specific pet data isn't needed.
    /// </summary>
    public void SpawnPetDefault()
    {
        // Clear existing pets before spawning a default one
        ClearAllSpawnedPets();

        // Create a dummy PetData for the default spawn (e.g., for creating a new pet)
        OwnedPetsManager.PetData defaultPetData = new OwnedPetsManager.PetData
        {
            pet_id = -1, // Indicate a new/uninitialized pet ID
            owner_id = -1, // Indicate no owner assigned yet
            pet_type = 0, // Default to the first pet type
            hunger = 100, // Default hunger
            affection = 100 // Default affection
        };
        SpawnPetWithData(defaultPetData);
    }


    /// <summary>
    /// Cycles to the next pet in the list for the next spawn.
    /// NOTE: This will clear existing pets and spawn a new one of the cycled type.
    /// It's intended for cycling through available pet types for display/selection.
    /// </summary>
    public void NextPet()
    {
        ClearAllSpawnedPets(); // Clear existing pets before spawning a new one of the cycled type

        // The previous currentPet reference is now likely destroyed by ClearAllSpawnedPets,
        // so we derive the next index based on the previously stored index or a default.
        currentPetIndex = (currentPetIndex + 1) % petSpriteLibraries.Count;

        OwnedPetsManager.PetData newPetData = new OwnedPetsManager.PetData
        {
            pet_id = -1, // Placeholder for new pet (as this is a cycle function, not loading a specific saved pet)
            owner_id = -1, // Placeholder for new pet
            pet_type = currentPetIndex,
            hunger = 100, // Default for new pet
            affection = 100 // Default for new pet
        };

        SpawnPetWithData(newPetData);
    }

    /// <summary>
    /// Cycles to the previous pet in the list for the next spawn.
    /// NOTE: This will clear existing pets and spawn a new one of the cycled type.
    /// It's intended for cycling through available pet types for display/selection.
    /// </summary>
    public void PreviousPet()
    {
        ClearAllSpawnedPets(); // Clear existing pets before spawning a new one of the cycled type

        // The previous currentPet reference is now likely destroyed by ClearAllSpawnedPets,
        // so we derive the next index based on the previously stored index or a default.
        currentPetIndex--;
        if (currentPetIndex < 0)
        {
            currentPetIndex = petSpriteLibraries.Count - 1;
        }

        OwnedPetsManager.PetData newPetData = new OwnedPetsManager.PetData
        {
            pet_id = -1, // Placeholder for new pet
            owner_id = -1, // Placeholder for new pet
            pet_type = currentPetIndex,
            hunger = 100, // Default for new pet
            affection = 100 // Default for new pet
        };

        SpawnPetWithData(newPetData);
    }

    /// <summary>
    /// Sets the pet type by a specific index and then spawns it.
    /// This is useful if you have UI buttons for each specific pet type.
    /// NOTE: This will clear existing pets and spawn a new one of the specified type.
    /// It's intended for setting a specific pet type from a list of options.
    /// </summary>
    /// <param name="petIndex">The index of the pet's SpriteLibraryAsset in the list.</param>
    public void SetAndSpawnPet(int petIndex)
    {
        ClearAllSpawnedPets(); // Clear existing pets before spawning a new one

        if (petIndex >= 0 && petIndex < petSpriteLibraries.Count)
        {
            // Create a dummy PetData for this function as it's meant to set a type, not apply full saved data.
            OwnedPetsManager.PetData tempPetData = new OwnedPetsManager.PetData
            {
                pet_id = -1, // Indicate this is a temporary/new pet
                owner_id = -1, // Indicate no owner assigned yet
                pet_type = petIndex,
                hunger = 100, // Default values for a new pet
                affection = 100
            };
            SpawnPetWithData(tempPetData);
        }
        else
        {
            Debug.LogWarning("Invalid pet index provided: " + petIndex);
        }
    }
}