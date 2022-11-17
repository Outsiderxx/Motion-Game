using UnityEngine;
using UnityEngine.UI;

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

        private OPDatumBodyDrawer[] opDatumBodyDrawers
        {
            get
            {
                return this.humanContainer.GetComponentsInChildren<OPDatumBodyDrawer>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (bVHMotionPlayer == null || opDatumBodyDrawers.Length == 0)
            {
                return;
            }
            foreach (var opDatumBodyDrawer in opDatumBodyDrawers)
            {
                if (opDatumBodyDrawer.detectPercentage > 0)
                {
                    float PCP = PoseEvaluation.CalculatePCP(opDatumBodyDrawer.bodyIndex, ref opDatumBodyDrawer.datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                    float PCK = PoseEvaluation.CalculatePCK(opDatumBodyDrawer.bodyIndex, ref opDatumBodyDrawer.datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                    float PDJ = PoseEvaluation.CalculatePDJ(opDatumBodyDrawer.bodyIndex, ref opDatumBodyDrawer.datum, this.bVHMotionPlayer.bvhData, this.bVHMotionPlayer.currentFrameIndex);
                    PCPText.text = "PCP: " + PCP.ToString("F2");
                    PCKText.text = "PCK: " + PCK.ToString("F2");
                    PDJText.text = "PDJ: " + PDJ.ToString("F2");
                    ScoreText.text = "Score: " + ((int)((PCP * 0.3 + PDJ * 0.7) * 100)).ToString();
                }
            }
        }
    }
}


