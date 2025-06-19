using UnityEngine;
using UnityEngine.U2D.Animation; // Required for SpriteLibraryAsset
using System.Collections.Generic; // Required for Dictionary and List
using UnityEngine.UI; // Required for Button

/// <summary>
/// Manages the spawning (instantiation) of different pet GameObjects.
/// Pets are instantiated from a prefab, and their appearance is determined
/// by a SpriteLibraryAsset which is applied to the spawned pet.
/// </summary>
public class PetSpawner : MonoBehaviour
{
    // Public fields to be assigned in the Unity Editor
    [Header("Pet Configuration")]
    [Tooltip("The base prefab for all pets. It should have a SpriteRenderer and a SpriteResolver component.")]
    public GameObject petPrefab;

    [Tooltip("A list of all available SpriteLibraryAssets. Each asset represents a different pet type (e.g., Cat, Dog, Bird).")]
    public List<SpriteLibraryAsset> petSpriteLibraries;

    [Tooltip("The position where new pets will be spawned.")]
    public Transform spawnPoint;

    // A dictionary to easily access SpriteLibraryAssets by name
    // This allows for easy extensibility and selection of pet types.
    private Dictionary<string, SpriteLibraryAsset> _petLibraryLookup;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the pet library lookup dictionary.
    /// </summary>
    void Awake()
    {
        _petLibraryLookup = new Dictionary<string, SpriteLibraryAsset>();
        // Populate the dictionary for quick lookups
        foreach (SpriteLibraryAsset library in petSpriteLibraries)
        {
            // Use the name of the SpriteLibraryAsset as the key
            if (!_petLibraryLookup.ContainsKey(library.name))
            {
                _petLibraryLookup.Add(library.name, library);
            }
            else
            {
                Debug.LogWarning($"Duplicate SpriteLibraryAsset name found: {library.name}. Only the first will be used.");
            }
        }

        // Ensure a spawn point is set, otherwise use the spawner's position
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
            Debug.LogWarning("Spawn Point not set for PetSpawner. Using PetSpawner's current position as spawn point.");
        }

        // Basic validation for the pet prefab
        if (petPrefab == null)
        {
            Debug.LogError("Pet Prefab is not assigned in the PetSpawner. Please assign a GameObject prefab.");
        }
    }

    /// <summary>
    /// Spawns a pet of the specified type.
    /// This method is designed to be called by UI buttons.
    /// </summary>
    /// <param name="petTypeName">The name of the SpriteLibraryAsset corresponding to the desired pet type.</param>
    public void SpawnPet(string petTypeName)
    {
        // Check if the pet prefab is assigned before trying to instantiate
        if (petPrefab == null)
        {
            Debug.LogError("Cannot spawn pet: Pet Prefab is not assigned.");
            return;
        }

        // Check if the requested pet type exists in our lookup dictionary
        if (_petLibraryLookup.TryGetValue(petTypeName, out SpriteLibraryAsset selectedLibrary))
        {
            // Instantiate the pet prefab at the spawn point's position and rotation
            GameObject newPet = Instantiate(petPrefab, spawnPoint.position, spawnPoint.rotation);
            newPet.name = $"{petTypeName}Pet"; // Name the spawned object for easier identification

            // Attempt to get the PetDisplayManager component from the new pet
            // This component is responsible for applying the SpriteLibraryAsset
            PetDisplayManager petDisplayManager = newPet.GetComponent<PetDisplayManager>();
            if (petDisplayManager != null)
            {
                // Set the SpriteLibraryAsset on the spawned pet
                petDisplayManager.SetSpriteLibrary(selectedLibrary);
                Debug.Log($"Successfully spawned a {petTypeName} pet!");
            }
            else
            {
                Debug.LogError($"Spawned pet prefab '{petPrefab.name}' does not have a PetDisplayManager component. Cannot set sprite library.");
            }
        }
        else
        {
            Debug.LogWarning($"Pet type '{petTypeName}' not found in available SpriteLibraryAssets. Make sure the name matches the asset name and it's assigned in the inspector.");
        }
    }
}
