using Project2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public Dropdown targetBvh;
    public Dropdown video;
    public List<GameObject> bvhBodies;
    private int bvhPlayIndex = 0;
    public void TogglePlayer()
    {
        GameSettings.Instance.isSinglePlayer = !GameSettings.Instance.isSinglePlayer;
    }

    public void ToggleGameMode()
    {
        GameSettings.Instance.isEvaluationMode = !GameSettings.Instance.isEvaluationMode;
    }

    public void ChangeVideo()
    {
        switch (video.value)
        {
            case 0:
                GameSettings.Instance.videoPath = "Korean_Solo.mp4";
                break;
            case 1:
                GameSettings.Instance.videoPath = "Inna_Dance.mp4";
                break;
            case 2:
                GameSettings.Instance.videoPath = "Korean_Dual.mp4";
                break;
        }

    }

    public void ChangeBvh()
    {
        bvhBodies[bvhPlayIndex].SetActive(false);
        bvhBodies[bvhPlayIndex].GetComponent<BVHMotionPlayer>().isPlaying = false;
        bvhBodies[targetBvh.value].SetActive(true);
        bvhBodies[targetBvh.value].GetComponent<BVHMotionPlayer>().isPlaying = true;
        bvhBodies[targetBvh.value].GetComponent<BVHMotionPlayer>().currentFrameIndex = 0;
        bvhPlayIndex = targetBvh.value;
        switch (targetBvh.value)
        {
            case 0:
                GameSettings.Instance.targetBvh = "05_01.bvh";
                break;
            case 1:
                GameSettings.Instance.targetBvh = "02_03.bvh";
                break;
            case 2:
                GameSettings.Instance.targetBvh = "02_04.bvh";
                break;
            case 3:
                GameSettings.Instance.targetBvh = "05_03.bvh";
                break;
            case 4:
                GameSettings.Instance.targetBvh = "02_05.bvh";
                break;
            case 5:
                GameSettings.Instance.targetBvh = "02_09.bvh";
                break;
            case 6:
                GameSettings.Instance.targetBvh = "05_10.bvh";
                break;
            case 7:
                GameSettings.Instance.targetBvh = "05_09.bvh";
                break;
            case 8:
                GameSettings.Instance.targetBvh = "05_02.bvh";
                break;
        }


    }

    public void ToggleSource()
    {
        GameSettings.Instance.isVideo = !GameSettings.Instance.isVideo;
    }

    void Start()
    {
        targetBvh.onValueChanged.AddListener(delegate { ChangeBvh(); });
        video.onValueChanged.AddListener(delegate { ChangeVideo(); });
        bvhBodies[0].GetComponent<BVHMotionPlayer>().isPlaying = true;
    }

    public void StartGame()
    {
        if (GameSettings.Instance.isEvaluationMode)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(2);
        }
    }


}
