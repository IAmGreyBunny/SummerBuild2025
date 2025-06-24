using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetEntryButton : MonoBehaviour
{
    public SceneChangeButtonHandler sceneChange;
    public int petTypeToCheck;
    public Button entryButton;
    public PetManager petManager; // Assign this in the Inspector

    private void OnEnable()
    {
        if (petManager != null)
        {
            petManager.OnPetsUpdated.AddListener(RefreshButton);
        }
        entryButton.interactable = false;
    }

    private void OnDisable()
    {
        if (petManager != null)
        {
            petManager.OnPetsUpdated.RemoveListener(RefreshButton);
        }
    }

    public void RefreshButton()
    {
        // Check if the GameDataManager exists
        if (GameDataManager.Instance == null)
        {
            Debug.LogError("GameDataManager not found!");
            return;
        }

        // --- CRITICAL CHANGE ---
        // Find the pet from the persistent data manager's list.
        PetData pet = GameDataManager.Instance.ownerPets.Find(p => p.pet_type == petTypeToCheck);

        entryButton.interactable = (pet != null);
        entryButton.onClick.RemoveAllListeners();

        if (pet != null)
        {
            entryButton.onClick.AddListener(() =>
            {
                // --- CRITICAL CHANGE ---
                // Set the selected pet on the persistent data manager.
                GameDataManager.Instance.selectedPet = pet;
                //SceneManager.LoadScene("Pet Interaction");
                sceneChange.PerformSceneChange();
            });
        }
    }
}
