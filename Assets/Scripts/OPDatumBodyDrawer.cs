using OpenPose;
using System.Collections.Generic;
using UnityEngine;

namespace Project2
{
    public class OPDatumBodyDrawer : MonoBehaviour
    {
        private MyHumanController2D body;
        public OpenPose.OPDatum datum;
        public int bodyIndex;

        public float detectPercentage
        {
            get
            {
                try
                {
                    if (this.bodyIndex >= datum.poseKeypoints.GetSize()[0])
                    {
                        return 0;
                    }
                    int detectJointNumber = 0;
                    for (int i = 0; i < MyData.PoseKeypointsCount; i++)
                    {
                        if (datum.poseKeypoints.Get(this.bodyIndex, i, 2) > 0)
                        {
                            detectJointNumber++;
                        }
                    }
                    return (float)detectJointNumber / MyData.PoseKeypointsCount;
                }
                catch (System.Exception)
                {
                    return 0;
                }

            }
        }

        private void Awake()
        {
            body = GetComponent<MyHumanController2D>();
        }

        public void Draw(ref OPDatum datum, int bodyIndex, float scoreThres = 0)
        {
            this.datum = datum;
            this.bodyIndex = bodyIndex;
            List<Vector3?> positions = new List<Vector3?>();
            if (datum.poseKeypoints != null && bodyIndex < datum.poseKeypoints.GetSize(0))
            {
                // Pose
                for (int part = 0; part < MyData.PoseKeypointsCount; part++)
                {
                    // Joints overflow
                    if (part >= datum.poseKeypoints.GetSize(1))
                    {
                        positions.Add(null);
                    }
                    else if (datum.poseKeypoints.Get(bodyIndex, part, 2) <= scoreThres)
                    {
                        positions.Add(null);
                    }
                    else
                    {
                        positions.Add(new Vector3(datum.poseKeypoints.Get(bodyIndex, part, 0), datum.poseKeypoints.Get(bodyIndex, part, 1), 0f));
                    }
                }
            }
            body.UpdateJointPositions(positions);
        }
    }
}
