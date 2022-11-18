using Project2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class AnalysisManager : MonoBehaviour
{
    [SerializeField] private List<BVHMotionPlayer> bVHMotionPlayer;
    [SerializeField] private Transform humanContainer;
    public Text analysisText;
    private int count = 0;
    private List<float> scores;
    public List<GameObject> motionScores;
    public Timer timer;

    private void Start()
    {
        scores = new List<float>();
        for (int i = 0; i < 5; i++)
        {
            scores.Add(0);
        }
    }

    private OPDatumBodyDrawer[] opDatumBodyDrawers
    {
        get
        {
            return this.humanContainer.GetComponentsInChildren<OPDatumBodyDrawer>();
        }
    }

    public void BackToMenu()
    {
        GameSettings.Instance.Reset();
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if (bVHMotionPlayer == null || opDatumBodyDrawers.Length == 0)
        {
            return;
        }
        if (timer.isTimeOut)
        {
            timer.EndCountDown();
            for (int i = 0; i < 5; i++)
            {
                this.bVHMotionPlayer[i].currentFrameIndex = 0;
            }
        }
        if (!timer.isCounting)
        {

            for (int i = 0; i < 5; i++)
            {
                if (opDatumBodyDrawers[0].detectPercentage > 0)
                {
                    float PCP = PoseEvaluation.CalculatePCP(opDatumBodyDrawers[0].bodyIndex, ref opDatumBodyDrawers[0].datum, this.bVHMotionPlayer[i].bvhData, this.bVHMotionPlayer[i].currentFrameIndex);
                    float PCK = PoseEvaluation.CalculatePCK(opDatumBodyDrawers[0].bodyIndex, ref opDatumBodyDrawers[0].datum, this.bVHMotionPlayer[i].bvhData, this.bVHMotionPlayer[i].currentFrameIndex);
                    float PDJ = PoseEvaluation.CalculatePDJ(opDatumBodyDrawers[0].bodyIndex, ref opDatumBodyDrawers[0].datum, this.bVHMotionPlayer[i].bvhData, this.bVHMotionPlayer[i].currentFrameIndex);
                    float score = (PCP * 0.3f + PDJ * 0.7f) * 100 / 400f;
                    scores[i] += score;
                }
            }
            count++;
            if (count > 400)
            {
                float max = -1;
                int index = 0; ;
                for (int i = 0; i < 5; i++)
                {
                    if (max < scores[i])
                    {
                        max = scores[i];
                        index = i;
                    }
                    motionScores[i].GetComponent<Text>().text = scores[i].ToString("F2");
                    scores[i] = 0;
                }
                count = 0;
                switch (index)
                {
                    case 0:
                        analysisText.text = "Run";
                        break;
                    case 1:
                        analysisText.text = "Kick";
                        break;
                    case 2:
                        analysisText.text = "Punch";
                        break;
                    case 3:
                        analysisText.text = "Swing";
                        break;
                    case 4:
                        analysisText.text = "Dance";
                        break;
                }
                timer.StartCountDown();
            }
        }
    }
}
