using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System.Text;

public class PetNeedsManager : MonoBehaviour
{
    [Header("Slider Reference")]
    public Slider hungerSlider;

    [Header("UI Popups")]
    public GameObject petRanAwayPopup;

    [Header("Hunger Settings")]
    public float decreaseIntervalInSeconds = 30f;
    public float decreaseAmount = 10f;
    public float feedAmount = 20f;

    private float nextDecreaseTime;
    private bool petHasRunAway = false;

    void Start()
    {
        if (petRanAwayPopup != null) { petRanAwayPopup.SetActive(false); }

        if (GameDataManager.Instance != null && GameDataManager.Instance.selectedPet != null)
        {
            int initialHunger = GameDataManager.Instance.selectedPet.hunger;
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = initialHunger;
        }
        else
        {
            hungerSlider.value = 100;
        }
        nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        CheckHungerStatus();
    }

    void Update()
    {
        if (petHasRunAway) return;
        if (Time.time >= nextDecreaseTime)
        {
            DecreaseHunger();
            nextDecreaseTime = Time.time + decreaseIntervalInSeconds;
        }
    }

    void DecreaseHunger()
    {
        hungerSlider.value = Mathf.Max(hungerSlider.minValue, hungerSlider.value - decreaseAmount);
        CheckHungerStatus();
    }

    private void CheckHungerStatus()
    {
        if (petHasRunAway) return;
        if (hungerSlider.value <= 0)
        {
            HandlePetRunAway();
        }
    }

    public void FeedPet()
    {
        if (petHasRunAway) return;
        hungerSlider.value = Mathf.Min(hungerSlider.maxValue, hungerSlider.value + feedAmount);
    }

    private void HandlePetRunAway()
    {
        petHasRunAway = true;
        Debug.Log("Pet has run away due to hunger!");

        // --- CHANGE ---
        // The deletion logic is NO LONGER called from here.
        // We just show the popup.

        PetDetails petObject = FindObjectOfType<PetDetails>();
        if (petObject != null)
        {
            petObject.gameObject.SetActive(false);
        }

        if (petRanAwayPopup != null)
        {
            petRanAwayPopup.SetActive(true);
        }
    }

    /// <summary>
    /// This is the new public method that your "Back" button should call.
    /// It starts the process of deleting the pet and then returning to the previous scene.
    /// </summary>
    public void InitiateDeleteAndReturn()
    {
        StartCoroutine(Co_DeleteAndReturn());
    }

    /// <summary>
    /// This Coroutine first sends a request to delete the pet, waits for the server
    /// to respond, and then loads the previous scene.
    /// </summary>
    private IEnumerator Co_DeleteAndReturn()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.selectedPet == null)
        {
            Debug.LogError("Cannot delete pet: No pet data found.");
            // Still go back to the previous scene even if we can't delete.
            SceneManager.LoadScene("MyPet");
            yield break;
        }

        int petIdToDelete = GameDataManager.Instance.selectedPet.pet_id;
        var requestData = new DeletePetRequest { pet_id = petIdToDelete };
        string jsonRequestBody = JsonUtility.ToJson(requestData);
        Debug.Log("Sending delete request: " + jsonRequestBody);

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

    // A small class to structure the JSON request for deletion
    [System.Serializable]
    private class DeletePetRequest
    {
        public int pet_id;
    }
}
