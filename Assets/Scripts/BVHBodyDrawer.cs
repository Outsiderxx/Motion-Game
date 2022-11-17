using System.Collections.Generic;
using UnityEngine;

namespace Project2
{
    public class BVHBodyDrawer : MonoBehaviour
    {
        private MyHumanController2D body;

        private void Awake()
        {
            body = GetComponent<MyHumanController2D>();
        }

        public void Draw(BVHParser bvhData, int frameIndex)
        {
            List<Vector3?> positions = new List<Vector3?>();
            for (int i = 0; i < MyData.PoseKeypointsCount; i++)
            {
                BVHParser.BVHBone bone = bvhData.GetBone(boneName: MyData.GetJointName(i));
                if (bone == null)
                {
                    Debug.LogError("Can't find bone:" + MyData.GetJointName(i));
                    positions.Add(null);
                }
                else
                {
                    Vector3 worldPos = bone.GetWorldPosAfterScale(frameIndex, 1.5f);
                    // worldPos.z = 0;
                    positions.Add(worldPos);
                }
            }
            body.UpdateJointPositions(positions);
        }
    }
}
