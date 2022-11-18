using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Project2
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BVHMotionPlayer bVHMotionPlayer;
        [SerializeField] private Transform humanContainer;
        [SerializeField] private Text ScoreText;
        [SerializeField] private Text PCPText;
        [SerializeField] private Text PCKText;
        [SerializeField] private Text PDJText;

        [SerializeField] private Text ScoreText2;
        [SerializeField] private Text PCPText2;
        [SerializeField] private Text PCKText2;
        [SerializeField] private Text PDJText2;

        public Text suggestion;
        private float score1 = 0;
        private float score2 = 0;
        public bool isGameStart;
        public Timer timer;
        private int lastBvhFrame = 0;

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

        private void StartGame()
        {
            Application.targetFrameRate = 30;
            bVHMotionPlayer.currentFrameIndex = 0;
            timer.StartCountDown();
        }

        void Start()
        {
            bVHMotionPlayer.isEnd += () =>
            {
                Debug.Log("End!!!!");
                isGameStart = false;
                bVHMotionPlayer.isPlaying = false;
            };
            bVHMotionPlayer.LoadBvh(GameSettings.Instance.targetBvh);
        }
        // Update is called once per frame
        void Update()
        {
            if (bVHMotionPlayer == null || opDatumBodyDrawers.Length == 0)
            {
                suggestion.text = "Player Not Found";
                return;
            }
            suggestion.text = "";

            if (!isGameStart)
            {
                //Check all players are found to start game     
                bool isFoundPlayers = true;
                for (int i = 0; i < opDatumBodyDrawers.Length; i++)
                {
                    if (opDatumBodyDrawers[i].detectPercentage != 1)
                    {
                        isFoundPlayers = false;
                        break;
                    }
                }
                int maxPeople = 1;
                if (!GameSettings.Instance.isSinglePlayer)
                {
                    maxPeople = 2;
                }
                if (isFoundPlayers && !timer.isCounting && maxPeople == opDatumBodyDrawers.Length)
                {
                    StartGame();
                }

                //Start game
                if (timer.isTimeOut)
                {
                    score1 = 0;
                    score2 = 0;
                    isGameStart = true;
                    bVHMotionPlayer.isPlaying = true;
                    timer.EndCountDown();
                }


            }
            //Game loop
            else
            {
                for (int i = 0; i < opDatumBodyDrawers.Length; i++)
                {

                    if (opDatumBodyDrawers[i].detectPercentage > 0 && i == 0)
                    {
                        float PCP = PoseEvaluation.CalculatePCP(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        float PCK = PoseEvaluation.CalculatePCK(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        float PDJ = PoseEvaluation.CalculatePDJ(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        PCPText.text = "PCP: " + PCP.ToString("F2");
                        PCKText.text = "PCK: " + PCK.ToString("F2");
                        PDJText.text = "PDJ: " + PDJ.ToString("F2");
                        score1 += (PCP * 0.3f + PDJ * 0.7f) * 100f / bVHMotionPlayer.bvhData.frames;
                        ScoreText.text = "Score: " + score1.ToString("F2");

                    }

                    if (opDatumBodyDrawers[i].detectPercentage > 0 && i == 1)
                    {
                        float PCP = PoseEvaluation.CalculatePCP(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        float PCK = PoseEvaluation.CalculatePCK(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        float PDJ = PoseEvaluation.CalculatePDJ(opDatumBodyDrawers[i].bodyIndex, ref opDatumBodyDrawers[i].datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                        PCPText2.text = "PCP: " + PCP.ToString("F2");
                        PCKText2.text = "PCK: " + PCK.ToString("F2");
                        PDJText2.text = "PDJ: " + PDJ.ToString("F2");
                        score2 += (PCP * 0.3f + PDJ * 0.7f) * 100f / bVHMotionPlayer.bvhData.frames;
                        ScoreText2.text = "Score: " + score2.ToString("F2");
                    }
                }

                //Check joint detection
                bool isFoundPlayers = true;
                for (int i = 0; i < opDatumBodyDrawers.Length; i++)
                {
                    if (opDatumBodyDrawers[i].detectPercentage != 1)
                    {
                        isFoundPlayers = false;
                        break;
                    }
                }
                if (!isFoundPlayers)
                {
                    suggestion.text = "Lost Joint";
                }
            }
        }
    }
}


