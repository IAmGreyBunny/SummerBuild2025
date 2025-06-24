using System.Diagnostics;
using UnityEngine;
using UnityEngine.U2D.Animation; // Required for SpriteResolver

public class IndivPetSpawner : MonoBehaviour
{
    public GameObject petPrefab; // One shared prefab
    public SpriteLibraryAsset[] spriteLibraries; // Indexed by pet_type

    private void Start()
    {
        if (PetSession.selectedPet == null)
        {
            UnityEngine.Debug.LogWarning("No pet selected!");
            return;
        }

        int type = PetSession.selectedPet.pet_type;
        UnityEngine.Debug.Log("Selected pet type: " + type);

        if (type < 0 || type >= spriteLibraries.Length)
        {
            UnityEngine.Debug.LogError("No SpriteLibrary for pet type: " + type);
            return;
        }

        Vector3 spawnPosition = new Vector3(-2.55f, -3.48f, 0f); // replace with your coordinates
        GameObject petInstance = Instantiate(petPrefab, spawnPosition, Quaternion.identity);

        UnityEngine.Debug.Log("Pet prefab instantiated");

        SpriteLibrary spriteLibrary = petInstance.GetComponent<SpriteLibrary>();
        if (spriteLibrary != null)
        {
            spriteLibrary.spriteLibraryAsset = spriteLibraries[type];
            UnityEngine.Debug.Log("SpriteLibrary assigned");
        }
        else
        {
            UnityEngine.Debug.LogError("Pet prefab is missing a SpriteLibrary component!");
        }
        
    }
}