using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Project2
{
    public static class PoseEvaluation
    {
        public static float CalculatePCP(int bodyIndex, ref OpenPose.OPDatum datum, BVHParser bvhData, int frameIndex)
        {
            string[] REFERENCE_LIMB_JOINTS = new string[] { "hip", "neck" };
            Vector2? estimateHipPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_LIMB_JOINTS[0]));
            Vector2? estimateRightHipPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_LIMB_JOINTS[1]));
            if (estimateHipPos == null || estimateRightHipPos == null)
            {
                return 0;
            }
            float estimationPoseBoneLength = Vector2.Distance((Vector2)estimateHipPos, (Vector2)estimateRightHipPos);
            float groundTruthBoneLength = Vector2.Distance(bvhData.GetBone(REFERENCE_LIMB_JOINTS[0]).GetWorldPosition(frameIndex), bvhData.GetBone(REFERENCE_LIMB_JOINTS[1]).GetWorldPosition(frameIndex));
            float scaleRatio = estimationPoseBoneLength / groundTruthBoneLength;

            List<Vector2> groundTruthPositions = new List<Vector2>();
            List<Vector2?> estimatePositions = new List<Vector2?>();
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                groundTruthPositions.Add(bvhData.GetBone(MyData.GetJointName(i)).GetWorldPosAfterScale(frameIndex, scaleRatio));
                Vector2? temp = datum.GetPointPosition(bodyIndex, i);
                estimatePositions.Add(temp == null ? temp : new Vector2(-((Vector2)temp).x, -((Vector2)temp).y));
            }
            NormalizePositions(groundTruthPositions, estimatePositions);
            // GameObject.Find("Canvas/Test").GetComponent<MyHumanController2D>().UpdateJointPositions(groundTruthPositions.Select(twoD => (Vector3?)new Vector3(twoD.x, twoD.y, 0)).ToList());
            // GameObject.Find("Canvas/Test2").GetComponent<MyHumanController2D>().UpdateJointPositions(estimatePositions.Select(twoD =>
            // {
            //     if (twoD == null)
            //     {
            //         return null;
            //     }
            //     Vector2 temp = (Vector2)twoD;
            //     return (Vector3?)new Vector3(temp.x, temp.y, 0);
            // }).ToList());

            int correctPartNumber = 0;
            for (int i = 0; i < MyData.PART_JOINT_NAMES.Length; i++)
            {
                Vector2? estimateJointOne = estimatePositions[MyData.GetJointIndex(MyData.PART_JOINT_NAMES[i][0])];
                Vector2? estimateJointTwo = estimatePositions[MyData.GetJointIndex(MyData.PART_JOINT_NAMES[i][1])];
                if (estimateJointOne == null || estimateJointTwo == null)
                {
                    continue;
                }

                Vector2 groundTruthJointOne = groundTruthPositions[MyData.GetJointIndex(MyData.PART_JOINT_NAMES[i][0])];
                Vector2 groundTruthJointTwo = groundTruthPositions[MyData.GetJointIndex(MyData.PART_JOINT_NAMES[i][1])];

                Vector2 estimateLimb = (Vector2)estimateJointOne - (Vector2)estimateJointTwo;
                Vector2 groundTruthLimb = groundTruthJointOne - groundTruthJointTwo;
                if (Vector2.Distance(estimateLimb, groundTruthLimb) <= 0.5 * groundTruthLimb.magnitude)
                {
                    correctPartNumber++;
                }
            }
            return (float)correctPartNumber / MyData.PART_JOINT_NAMES.Length;
        }

        public static float CalculatePCK(int bodyIndex, ref OpenPose.OPDatum datum, BVHParser bvhData, int frameIndex)
        {
            string[] REFERENCE_SPINE_JOINTS = new string[] { "hip", "neck" };
            Vector2? estimateHipPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[0]));
            Vector2? estimateNeckPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[1]));
            if (estimateHipPos == null || estimateNeckPos == null)
            {
                return 0;
            }
            float estimationPoseSpineLength = Vector2.Distance((Vector2)estimateHipPos, (Vector2)estimateNeckPos);
            float groundTruthSpineLength = Vector2.Distance(bvhData.GetBone(REFERENCE_SPINE_JOINTS[0]).GetWorldPosition(frameIndex), bvhData.GetBone(REFERENCE_SPINE_JOINTS[1]).GetWorldPosition(frameIndex));
            float scaleRatio = estimationPoseSpineLength / groundTruthSpineLength;

            List<Vector2> groundTruthPositions = new List<Vector2>();
            List<Vector2?> estimatePositions = new List<Vector2?>();
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                groundTruthPositions.Add(bvhData.GetBone(MyData.GetJointName(i)).GetWorldPosAfterScale(frameIndex, scaleRatio));
                Vector2? temp = datum.GetPointPosition(bodyIndex, i);
                estimatePositions.Add(temp == null ? temp : new Vector2(-((Vector2)temp).x, -((Vector2)temp).y));
            }
            NormalizePositions(groundTruthPositions, estimatePositions);

            int correctJointNumber = 0;
            List<bool> correctJoint = new List<bool>();
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                Vector2 groundTruthJointPos = groundTruthPositions[i];
                Vector2? estimateJointPos = estimatePositions[i];
                if (estimateJointPos == null)
                {
                    correctJoint.Add(false);
                    continue;
                }

                float scaleGroudTruthHeadLength = Vector2.Distance(groundTruthPositions[MyData.GetJointIndex("head")], groundTruthPositions[MyData.GetJointIndex("neck")]);
                if (Vector2.Distance((Vector2)estimateJointPos, groundTruthJointPos) <= 0.5 * scaleGroudTruthHeadLength)
                {
                    correctJoint.Add(true);
                    correctJointNumber++;
                }
                else
                {
                    correctJoint.Add(false);
                }
            }
            if ((float)correctJointNumber / MyData.PoseKeypointsCount > 0.6)
            {
                Debug.Log("debug");
            }

            return (float)correctJointNumber / MyData.PoseKeypointsCount;
        }

        public static float CalculatePDJ(int bodyIndex, ref OpenPose.OPDatum datum, BVHParser bvhData, int frameIndex)
        {
            string[] REFERENCE_SPINE_JOINTS = new string[] { "hip", "neck" };
            Vector2? estimateHipPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[0]));
            Vector2? estimateNeckPos = datum.GetPointPosition(bodyIndex, MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[1]));
            if (estimateHipPos == null || estimateNeckPos == null)
            {
                return 0;
            }
            float estimationPoseSpineLength = Vector2.Distance((Vector2)estimateHipPos, (Vector2)estimateNeckPos);
            float groundTruthSpineLength = Vector2.Distance(bvhData.GetBone(REFERENCE_SPINE_JOINTS[0]).GetWorldPosition(frameIndex), bvhData.GetBone(REFERENCE_SPINE_JOINTS[1]).GetWorldPosition(frameIndex));
            float scaleRatio = estimationPoseSpineLength / groundTruthSpineLength;

            List<Vector2> groundTruthPositions = new List<Vector2>();
            List<Vector2?> estimatePositions = new List<Vector2?>();
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                groundTruthPositions.Add(bvhData.GetBone(MyData.GetJointName(i)).GetWorldPosAfterScale(frameIndex, scaleRatio));
                Vector2? temp = datum.GetPointPosition(bodyIndex, i);
                estimatePositions.Add(temp == null ? temp : new Vector2(-((Vector2)temp).x, -((Vector2)temp).y));
            }
            NormalizePositions(groundTruthPositions, estimatePositions);

            int correctJointNumber = 0;
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                Vector2 groundTruthJointPos = groundTruthPositions[i];
                Vector2? estimateJointPos = estimatePositions[i];
                if (estimateJointPos == null)
                {
                    continue;
                }

                float scaleGroudTruthSpineLength = Vector2.Distance(groundTruthPositions[MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[0])], groundTruthPositions[MyData.GetJointIndex(REFERENCE_SPINE_JOINTS[1])]);
                if (Vector2.Distance((Vector2)estimateJointPos, groundTruthJointPos) <= 0.2 * scaleGroudTruthSpineLength)
                {
                    correctJointNumber++;
                }

            }
            if ((float)correctJointNumber / MyData.PoseKeypointsCount > 0.6)
            {
                Debug.Log("debug");
            }
            return (float)correctJointNumber / MyData.PoseKeypointsCount;
        }

        private static void NormalizePositions(List<Vector2> groundTruthPositions, List<Vector2?> estimatePositions)
        {
            Vector2 originGroundTruthHipPos = groundTruthPositions[MyData.GetJointIndex("hip")];
            for (int i = 0; i < groundTruthPositions.Count; i++)
            {
                groundTruthPositions[i] = groundTruthPositions[i] - originGroundTruthHipPos;
            }
            Vector2 originEstimateHipPos = (Vector2)estimatePositions[MyData.GetJointIndex("hip")];
            for (int i = 0; i < estimatePositions.Count; i++)
            {
                if (estimatePositions[i] != null)
                {
                    estimatePositions[i] = estimatePositions[i] - originEstimateHipPos;
                }
            }
        }
    }
}
