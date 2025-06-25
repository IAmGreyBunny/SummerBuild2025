using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class IndivPetSpawner : MonoBehaviour
{
    public GameObject petPrefab;
    public SpriteLibraryAsset[] spriteLibraries;
    public GameObject interactionPanelPrefab;
    public Canvas canvas;

    // --- THIS IS THE FIX ---
    // We declare petInstance here, at the class level, so it's accessible
    // to all methods within this script.
    private GameObject petInstance;

    public void SpawnPet(PetData data)
    {
        if (data == null) { Debug.LogError("Spawner received no data!"); return; }

        int type = data.pet_type;
        if (type < 0 || type >= spriteLibraries.Length) { Debug.LogError("No SpriteLibrary for pet type: " + type); return; }

        // Spawn pet and assign it to our class-level variable.
        Vector3 spawnPosition = new Vector3(0f, -3.48f, 0f);
        petInstance = Instantiate(petPrefab, spawnPosition, Quaternion.identity);

        // Assign sprite
        SpriteLibrary spriteLibrary = petInstance.GetComponent<SpriteLibrary>();
        if (spriteLibrary != null) { spriteLibrary.spriteLibraryAsset = spriteLibraries[type]; }
        else { Debug.LogError("Pet prefab is missing a SpriteLibrary component!"); }

        // Apply stats
        PetDetails petDetails = petInstance.GetComponent<PetDetails>();
        if (petDetails != null)
        {
            petDetails.petId = data.pet_id;
            petDetails.ownerId = data.owner_id;
            petDetails.petType = data.pet_type;
            petDetails.currentHunger = data.hunger;
            petDetails.currentAffection = data.affection;
        }
        else { Debug.LogError("Pet prefab is missing a PetDetails component!"); }

        // Link interaction panel
        GameObject panelInstance = Instantiate(interactionPanelPrefab, canvas.transform);
        panelInstance.SetActive(false);

        AnimalHighlighter highlighter = petInstance.GetComponent<AnimalHighlighter>();
        if (highlighter != null)
        {
            highlighter.interactionPanel = panelInstance;
            highlighter.canvas = canvas;
            highlighter.InitializeButtons();
            // ... your button finding logic ...
        }
    }

    // Any other methods in your script that need to use petInstance can now access it without error.
}