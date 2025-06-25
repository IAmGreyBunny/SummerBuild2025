using UnityEngine;

public class InteractionSceneController : MonoBehaviour
{
    void Start()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("ERROR: No pet data found. Cannot give spawn command.");
            return;
        }
        PetData petDataToSpawn = GameDataManager.Instance.selectedPet;

        IndivPetSpawner petSpawner = FindObjectOfType<IndivPetSpawner>();
        if (petSpawner == null)
        {
            Debug.LogError("ERROR: Could not find your 'IndivPetSpawner' script in the scene!");
            return;
        }

        Debug.Log("Data is ready. Sending spawn command to IndivPetSpawner.");
        petSpawner.SpawnPet(petDataToSpawn);
    }
}
