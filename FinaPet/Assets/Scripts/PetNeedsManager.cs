using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

/// <summary>
/// Manages the pet's hunger UI slider and handles the "run away" logic.
/// When hunger reaches zero, it shows a popup and provides a method to
/// delete the pet from the database before returning to the previous scene.
/// </summary>
public class PetNeedsManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI Slider that displays the pet's current hunger level.")]
    public Slider hungerSlider;
    [Tooltip("The popup panel that appears when the pet runs away.")]
    public GameObject petRanAwayPopup;

    private bool petHasRunAway = false;

    void Start()
    {
        // Ensure the runaway popup is hidden on start.
        if (petRanAwayPopup != null)
        {
            petRanAwayPopup.SetActive(false);
        }

        // Initialize the hunger slider's value from the persistent data manager.
        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialHunger = GameDataManager.Instance.selectedPet.hunger;

            // Set slider properties.
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = initialHunger;

            Debug.Log($"PetNeedsManager: Hunger slider initialized with value: {initialHunger}");

            // Immediately check if the pet should have already run away.
            CheckHungerStatus();
        }
        else
        {
            // Fallback for testing or if data is missing.
            Debug.LogWarning("PetNeedsManager: No pet data found. Initializing hunger to a default of 100.");
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = 100;
        }
    }

    /// <summary>
    /// Checks the current hunger value and triggers the runaway sequence if it's zero.
    /// </summary>
    private void CheckHungerStatus()
    {
        if (petHasRunAway) return;

        if (hungerSlider.value <= 0)
        {
            HandlePetRunAway();
        }
    }

    /// <summary>
    /// The sequence of events when a pet's hunger reaches zero.
    /// This now only handles the visual part: disabling the pet and showing the popup.
    /// </summary>
    private void HandlePetRunAway()
    {
        petHasRunAway = true;
        Debug.Log("Pet has run away due to hunger! Showing runaway popup.");

        // Find and disable the pet's GameObject.
        PetDetails petObject = FindObjectOfType<PetDetails>();
        if (petObject != null)
        {
            petObject.gameObject.SetActive(false);
        }

        // Show the "Pet Ran Away" popup.
        if (petRanAwayPopup != null)
        {
            petRanAwayPopup.SetActive(true);
        }
    }

    /// <summary>
    /// This is the new public method that your "Back" button on the popup should call.
    /// It starts the process of deleting the pet from the database and then returning.
    /// </summary>
    public void InitiateDeleteAndReturn()
    {
        StartCoroutine(Co_DeletePetAndReturn());
    }

    /// <summary>
    /// This Coroutine first sends a request to delete the pet from the database,
    /// waits for the server to respond, and then loads the previous scene.
    /// </summary>
    private IEnumerator Co_DeletePetAndReturn()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("Cannot delete pet: No pet data found in GameDataManager.");
            // Still go back to the previous scene even if we can't delete.
            SceneManager.LoadScene("My Pets");
            yield break;
        }

        int petIdToDelete = GameDataManager.Instance.selectedPet.pet_id;
        var requestData = new DeletePetRequest { pet_id = petIdToDelete };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        Debug.Log("Sending delete request to server: " + jsonRequestBody);

        string fullUrl = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath() + "/delete_pet.php";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Wait for the request to complete
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Delete Pet API Call Failed! Error: {request.error}");
            }
            else
            {
                Debug.Log("Delete Pet Response: " + request.downloadHandler.text);
            }
        }

        // After the web request is done, load the scene.
        SceneManager.LoadScene("My Pet");
    }

    // --- Public Methods for other scripts (like FeedManager) to use ---

    public int GetCurrentHunger()
    {
        return (int)hungerSlider.value;
    }

    public void SetHunger(int newHunger)
    {
        if (petHasRunAway) return;
        hungerSlider.value = Mathf.Clamp(newHunger, hungerSlider.minValue, hungerSlider.maxValue);
        CheckHungerStatus(); // Check if this change caused the pet to run away.
    }

    public bool IsHungerFull()
    {
        return hungerSlider.value >= hungerSlider.maxValue;
    }

    // A small helper class to structure the JSON request for deletion.
    [System.Serializable]
    private class DeletePetRequest
    {
        public int pet_id;
    }
}
