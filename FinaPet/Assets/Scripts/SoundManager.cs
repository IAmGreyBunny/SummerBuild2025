using UnityEngine;
using UnityEngine.UI;
// Make sure this is attached to an active GameObject in your scene.
public class SoundManager : MonoBehaviour
{
    public Toggle toggle;
    // This Awake function runs when the script instance is being loaded.
    private void Awake()
    {
        Debug.Log("SoundManager script has started!");

        if (AudioListener.volume == 0) toggle.isOn = false;
        else toggle.isOn = true;
    }

    // This is the function the checkbox should call.
    // In your SoundManager.cs script

    public void SetSoundState(bool isSoundOn)
    {
        // This log will tell us IF the function is being called and WHAT value it received.
        Debug.Log($"Checkbox value changed! isSoundOn is: {isSoundOn}");

        if (isSoundOn)
        {
            // If the checkbox is ticked (isSoundOn is true), set volume to 1 (ON)
            AudioListener.volume = 1;
            Debug.Log("AudioListener.volume set to 1. Sounds should be ON.");
        }
        else
        {
            // If the checkbox is unticked (isSoundOn is false), set volume to 0 (MUTED)
            AudioListener.volume = 0;
            Debug.Log("AudioListener.volume set to 0. Sounds should be MUTED.");
        }
    }
}