using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Controls the UI for displaying pets. It cycles through pet sprites
/// and notifies other scripts when the displayed pet changes.
/// </summary>
public class PetDisplayController : MonoBehaviour
{
    // Assign your pet sprites in the Unity Inspector. 
    // The order must match the pet_type (index 0 = dog, index 1 = cat, etc.)
    public Sprite[] petSprites;

    // Assign the UI Image element that will display the pet
    public Image petDisplayImage;

    // This event will broadcast the current pet's type (index) whenever it changes.
    [System.Serializable]
    public class PetChangedEvent : UnityEvent<int> { }
    public PetChangedEvent OnPetChanged = new PetChangedEvent();

    private int currentPetIndex = 0;

    void Start()
    {
        if (petSprites == null || petSprites.Length == 0)
        {
            Debug.LogError("Pet sprites have not been assigned in the PetDisplayController.");
            return;
        }
        if (petDisplayImage == null)
        {
            Debug.LogError("The Pet Display Image has not been assigned in the PetDisplayController.");
            return;
        }

        // Set the initial pet image and notify any listeners of the starting pet.
        UpdatePetImage();
    }

    public void NextPet()
    {
        currentPetIndex++;
        if (currentPetIndex >= petSprites.Length)
        {
            currentPetIndex = 0; // Wrap around to the start
        }
        UpdatePetImage();
    }

    public void PreviousPet()
    {
        currentPetIndex--;
        if (currentPetIndex < 0)
        {
            currentPetIndex = petSprites.Length - 1; // Wrap around to the end
        }
        UpdatePetImage();
    }

    private void UpdatePetImage()
    {
        // Change the sprite of the Image component
        petDisplayImage.sprite = petSprites[currentPetIndex];

        // Invoke the event, sending the current index as the pet type.
        OnPetChanged.Invoke(currentPetIndex);
    }

    // A helper function for other scripts to get the current pet type on startup.
    public int GetCurrentPetType()
    {
        return currentPetIndex;
    }
}
