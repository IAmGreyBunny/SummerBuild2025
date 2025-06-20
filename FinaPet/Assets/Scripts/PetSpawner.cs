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
    [Tooltip("The GameObject prefab for your pet. This prefab must have a SpriteLibrary component.")]
    public GameObject petPrefab;

    [Header("Sprite Libraries")]
    [Tooltip("A list of all the different Sprite Library Assets for your pets.")]
    public List<SpriteLibraryAsset> petSpriteLibraries;

    [Header("Spawn Settings")]
    [Tooltip("The position where the pet will be spawned.")]
    public Vector3 spawnPosition = Vector3.zero;

    // A reference to the currently spawned pet
    private GameObject currentPet;
    private int currentPetIndex = 0;

    /// <summary>
    /// Spawns the pet prefab with the initial sprite library (the first in the list).
    /// This method is ideal for being called by a UI Button's OnClick event.
    /// </summary>
    public void SpawnPet()
    {
        //// If a pet already exists, destroy it before spawning a new one.
        //if (currentPet != null)
        //{
        //    Destroy(currentPet);
        //}

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

        // Instantiate the pet prefab.
        currentPet = Instantiate(petPrefab, spawnPosition, Quaternion.identity);
        currentPet.name = "Pet_" + petSpriteLibraries[currentPetIndex].name;

        // Get the SpriteLibrary component from the new pet instance.
        SpriteLibrary spriteLibrary = currentPet.GetComponent<SpriteLibrary>();
        if (spriteLibrary != null)
        {
            // Assign the currently selected sprite library asset.
            spriteLibrary.spriteLibraryAsset = petSpriteLibraries[currentPetIndex];
        }
        else
        {
            Debug.LogError("The assigned Pet Prefab does not have a SpriteLibrary component!");
        }
    }

    /// <summary>
    /// Cycles to the next pet in the list for the next spawn.
    /// </summary>
    public void NextPet()
    {
        // Increment the index, and loop back to the start if we reach the end.
        currentPetIndex = (currentPetIndex + 1) % petSpriteLibraries.Count;

        // Spawn the pet with the new index
        SpawnPet();
    }

    /// <summary>
    /// Cycles to the previous pet in the list for the next spawn.
    /// </summary>
    public void PreviousPet()
    {
        // Decrement the index.
        currentPetIndex--;

        // If the index is less than 0, loop back to the end of the list.
        if (currentPetIndex < 0)
        {
            currentPetIndex = petSpriteLibraries.Count - 1;
        }

        // Spawn the pet with the new index
        SpawnPet();
    }

    /// <summary>
    /// Sets the pet type by a specific index and then spawns it.
    /// This is useful if you have UI buttons for each specific pet type.
    /// </summary>
    /// <param name="petIndex">The index of the pet's SpriteLibraryAsset in the list.</param>
    public void SetAndSpawnPet(int petIndex)
    {
        if (petIndex >= 0 && petIndex < petSpriteLibraries.Count)
        {
            currentPetIndex = petIndex;
            SpawnPet();
        }
        else
        {
            Debug.LogWarning("Invalid pet index provided: " + petIndex);
        }
    }
}
