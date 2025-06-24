using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetEntryButton : MonoBehaviour
{
    public int petTypeToCheck;
    public Button entryButton;
    public PetManager petManager; // Assign this in the Inspector

    private void Start()
    {
        PetData pet = petManager.GetPetByType(petTypeToCheck);

        if (pet != null)
        {
            entryButton.interactable = true;
            entryButton.onClick.AddListener(() =>
            {
                PetSession.selectedPet = pet;
                SceneManager.LoadScene("Pet Interaction");
            });
        }
        else
        {
            entryButton.interactable = false;
        }
    }
    public void RefreshButton()
    {
        PetData pet = petManager.GetPetByType(petTypeToCheck);
        entryButton.interactable = (pet != null);

        entryButton.onClick.RemoveAllListeners();
        if (pet != null)
        {
            entryButton.onClick.AddListener(() =>
            {
                PetSession.selectedPet = pet;
                SceneManager.LoadScene("Pet Interaction");
            });
        }
    }

}
