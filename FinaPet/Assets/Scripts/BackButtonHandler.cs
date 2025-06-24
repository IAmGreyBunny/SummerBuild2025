using UnityEngine;

// A simple script to bridge the gap between a UI Button and the SceneController singleton.
public class BackButtonHandler : MonoBehaviour
{
    /// <summary>
    /// This public method can be easily linked to a button's OnClick event.
    /// </summary>
    public void OnBackButtonClick()
    {
        Debug.Log("Back button clicked");

        // Check if the instance exists to prevent errors
        if (SceneController.Instance != null)
        {
            // Use the public static 'Instance' to call the GoBack method.
            SceneController.Instance.GoBack();
        }
        else
        {
            Debug.LogError("SceneController.Instance not found. Make sure your SceneController is in the initial scene.");
        }
    }
}