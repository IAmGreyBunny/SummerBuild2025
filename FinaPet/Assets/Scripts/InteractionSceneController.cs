using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// This controller is the bridge between the data system and your pet spawner.
/// It ensures the correct data is available and then applies that data to the newly spawned pet.
/// </summary>
public class InteractionSceneController : MonoBehaviour
{
    void Start()
    {
        // 1. Get the selected pet data from the persistent manager.
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("No selected pet data found in GameDataManager! Cannot spawn pet.");
            return;
        }

        // 2. Bridge the data to the static variable your spawner uses.
        PetData petToSpawn = GameDataManager.Instance.selectedPet;
        PetSession.selectedPet = petToSpawn;
        Debug.Log($"Bridged data for pet type {petToSpawn.pet_type} to PetSession. Your spawner can now proceed.");

        // 3. Find your existing spawner in the scene to confirm it exists.
        // FIX: Replaced FindObjectOfType with FindFirstObjectByType
        IndivPetSpawner petSpawner = FindFirstObjectByType<IndivPetSpawner>();
        if (petSpawner == null)
        {
            Debug.LogError("Could not find an object with the 'IndivPetSpawner' script in the scene!");
            return;
        }

        // --- THIS IS THE FIX ---
        // We start a coroutine to wait one frame. This gives your IndivPetSpawner's Start()
        // method time to run and create the pet GameObject before we try to find its components.
        StartCoroutine(InitializeSpawnedPet());
    }

    private IEnumerator InitializeSpawnedPet()
    {
        // Wait until the end of the current frame.
        // By this time, your IndivPetSpawner should have created the pet.
        yield return new WaitForEndOfFrame();

        // Find the PetDetails script that is now in the scene.
        // NOTE: This assumes your script on the pet is named "PetDetails".
        // If it's named something else, change the type here.
        // FIX: Replaced FindObjectOfType with FindFirstObjectByType
        PetDetails petDetails = FindFirstObjectByType<PetDetails>();

        if (petDetails != null)
        {
            // Get the data again from our reliable data manager
            PetData data = GameDataManager.Instance.selectedPet;

            // Apply the saved data to the PetDetails component on the spawned pet.
            // NOTE: This assumes the variable names in your PetDetails script match
            // the variable names in your PetData class (e.g., pet_id, hunger).
            petDetails.petId = data.pet_id;
            petDetails.ownerId = data.owner_id;
            petDetails.petType = data.pet_type;
            petDetails.currentHunger = data.hunger;
            petDetails.currentAffection = data.affection;

            Debug.Log($"Successfully applied stats to spawned pet ID: {petDetails.petId}. Hunger: {petDetails.currentHunger}");
        }
        else
        {
            Debug.LogError("Initialization failed: Could not find a 'PetDetails' component in the scene after spawning. Make sure your pet prefab has this script attached and the class name is correct.");
        }
    }
}