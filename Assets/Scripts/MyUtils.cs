using UnityEngine;

namespace Project2
{
    public static class MyUtils
    {
        public static Vector2? GetPointPosition(this OpenPose.OPDatum datum, int bodyIndex, int jointIndex)
        {
            if (datum.poseKeypoints.Get(bodyIndex, jointIndex, 2) < 0.5)
            {
                return null;
            }
            return new Vector2(datum.poseKeypoints.Get(bodyIndex, jointIndex, 0), datum.poseKeypoints.Get(bodyIndex, jointIndex, 1));
        }
    }
}
