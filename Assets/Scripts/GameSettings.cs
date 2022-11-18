using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }
    public bool isSinglePlayer; //single or dual
    public bool isEvaluationMode; // evaluation or analysis
    public string targetBvh;
    public string videoPath;
    public bool isVideo;

    public void Reset()
    {
        isSinglePlayer = true;
        isEvaluationMode = true;
        isVideo = true;
        targetBvh = "05_01.bvh";
        videoPath = "Korean_Solo.mp4";
    }
    void Awake()
    {
        if (Instance == null)
        {
            isSinglePlayer = true;
            isEvaluationMode = true;
            isVideo = true;
            targetBvh = "05_01.bvh";
            videoPath = "Korean_Solo.mp4";
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
