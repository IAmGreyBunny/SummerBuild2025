using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // You can call this method from a UI button, another script, or an event.
    public void MuteAllSound()
    {
        AudioListener.volume = 0;
        Debug.Log("All sounds muted.");
    }

    public void UnmuteAllSound()
    {
        AudioListener.volume = 1; // 1 is full volume
        Debug.Log("All sounds unmuted.");
    }

    // Optional: A method to toggle mute state
    public void ToggleMuteSound()
    {
        if (AudioListener.volume == 0)
        {
            UnmuteAllSound();
        }
        else
        {
            MuteAllSound();
        }
    }

    // Example of how you might use it (e.g., press 'M' to mute/unmute)
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    ToggleMuteSound();
        //}
    }
}