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

        // In your IndivPetSpawner.cs Start() method...

        AnimalHighlighter highlighter = petInstance.GetComponent<AnimalHighlighter>();
        if (highlighter != null)
        {
            highlighter.interactionPanel = panelInstance;
            highlighter.canvas = canvas;

            // --- CORRECTED BUTTON FINDING LOGIC ---

            // Find the Feed Button using its full path
            Transform feedButtonTransform = panelInstance.transform.Find("Button Container/FeedButton");
            if (feedButtonTransform != null)
            {
                highlighter.feedButton = feedButtonTransform.GetComponent<Button>();
                UnityEngine.Debug.Log("Successfully found and assigned FeedButton.");
            }
            else
            {
                UnityEngine.Debug.LogError("ERROR: Could not find 'FeedButton' at path 'Button Container/FeedButton'!");
            }

            // Find the Pet Button using its full path
            Transform petButtonTransform = panelInstance.transform.Find("Button Container/PetButton");
            if (petButtonTransform != null)
            {
                highlighter.petButton = petButtonTransform.GetComponent<Button>();
                UnityEngine.Debug.Log("Successfully found and assigned PetButton.");
            }
            else
            {
                UnityEngine.Debug.LogError("ERROR: Could not find 'PetButton' at path 'Button Container/PetButton'!");
            }

            // Do the same for your Close Button
            // Assuming it's also inside "Button Container" and named "CloseButton"
            Transform closeButtonTransform = panelInstance.transform.Find("CloseButton");
            if (closeButtonTransform != null)
            {
                Button closeButton = closeButtonTransform.GetComponent<Button>();
                closeButton.onClick.AddListener(() => panelInstance.SetActive(false));
                UnityEngine.Debug.Log("Close button linked.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("Could not find 'CloseButton' at path 'CloseButton'!");
            }
        }
    }
}