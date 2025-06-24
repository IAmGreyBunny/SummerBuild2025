using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetEntryButton : MonoBehaviour
{
    // --- UI STATE MANAGEMENT ---
    // Assign your "Go to Pet" button/object in the Inspector.
    public GameObject ownedStateObject;

    // Assign your "You have not unlocked this pet!" panel/object in the Inspector.
    public GameObject unownedStateObject;
    // -------------------------

    // The actual button component that lives on the ownedStateObject
    public Button entryButton;

    // Scene and Data management references
    public SceneChangeButtonHandler sceneChange;
    public PetDisplayController displayController;
    public PetManager petManager;

    private int petTypeToCheck;

    private void OnEnable()
    {
        // --- Listeners remain the same ---
        if (displayController != null)
        {
            displayController.OnPetChanged.AddListener(OnCurrentPetChanged);
        }
        else
        {
            Debug.LogError("CRITICAL: DisplayController is not assigned on the PetEntryButton!");
        }

        if (petManager != null)
        {
            petManager.OnPetsUpdated.AddListener(RefreshButton);
        }
        else
        {
            Debug.LogError("CRITICAL: PetManager is not assigned on the PetEntryButton!");
        }

        // Initial refresh
        RefreshButton();
    }

    private void OnDisable()
    {
        // --- Listeners remain the same ---
        if (displayController != null)
        {
            displayController.OnPetChanged.RemoveListener(OnCurrentPetChanged);
        }
        if (petManager != null)
        {
            petManager.OnPetsUpdated.RemoveListener(RefreshButton);
        }
    }

    private void OnCurrentPetChanged(int newPetType)
    {
        petTypeToCheck = newPetType;
        RefreshButton();
    }

    /// <summary>
    /// This master method now updates the UI visuals based on pet ownership.
    /// </summary>
    public void RefreshButton()
    {
        if (GameDataManager.Instance == null)
        {
            Debug.LogWarning("RefreshButton called, but GameDataManager not ready yet.");
            return;
        }

        if (displayController != null)
        {
            petTypeToCheck = displayController.GetCurrentPetType();
        }

        PetData pet = GameDataManager.Instance.ownerPets.Find(p => p.pet_type == petTypeToCheck);

        // --- NEW LOGIC TO SWAP UI ---
        bool playerOwnsPet = (pet != null);

        // Show the correct UI state
        if (ownedStateObject != null) ownedStateObject.SetActive(playerOwnsPet);
        if (unownedStateObject != null) unownedStateObject.SetActive(!playerOwnsPet);
        // -----------------------------

        // Clear previous listeners to avoid bugs
        entryButton.onClick.RemoveAllListeners();

        // If the player owns the pet, set up the button's click event
        if (playerOwnsPet)
        {
            entryButton.onClick.AddListener(() =>
            {
                GameDataManager.Instance.selectedPet = pet;
                sceneChange.PerformSceneChange();
            });
        }
    }
}
