using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoRewardManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;          // Drag your VideoPlayer here in the Inspector
    public GameObject rewardPopupPanel;      // Drag your popup panel UI here

    void Start()
    {
        // Register the event when the video finishes playing
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Show the reward popup
        rewardPopupPanel.SetActive(true);
    }

    public void ClaimReward()
    {
        // Give reward logic goes here
        Debug.Log("Reward claimed!");

        // Hide the panel
        rewardPopupPanel.SetActive(false);
    }
}
