using System.Diagnostics;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class IndivPetSpawner : MonoBehaviour
{
    public GameObject petPrefab;                 // Shared pet prefab
    public SpriteLibraryAsset[] spriteLibraries; // Indexed by pet_type
    public GameObject interactionPanelPrefab;    // UI Panel prefab
    public Canvas canvas;                        // Main canvas

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

        // Spawn the pet
        Vector3 spawnPosition = new Vector3(-2.55f, -3.48f, 0f);
        GameObject petInstance = Instantiate(petPrefab, spawnPosition, Quaternion.identity);
        UnityEngine.Debug.Log("Pet prefab instantiated");

        // Assign the SpriteLibraryAsset
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

        // Instantiate and link the interaction panel
        GameObject panelInstance = Instantiate(interactionPanelPrefab, canvas.transform);
        panelInstance.SetActive(false); // hide by default

        AnimalHighlighter highlighter = petInstance.GetComponent<AnimalHighlighter>();
        if (highlighter != null)
        {
            highlighter.interactionPanel = panelInstance;
            highlighter.canvas = canvas;

            highlighter.feedButton = panelInstance.transform.Find("FeedButton").GetComponent<Button>();
            highlighter.petButton = panelInstance.transform.Find("PetButton").GetComponent<Button>();

            UnityEngine.Debug.Log("Panel and buttons linked to highlighter");
        }
        else
        {
            UnityEngine.Debug.LogWarning("AnimalHighlighter not found on spawned pet");
        }
    }
}
