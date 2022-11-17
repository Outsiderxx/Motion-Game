namespace Project2
{
    public static class MyData
    {
        public static readonly int PoseKeypointsCount = 15;
        public static readonly string[][] PART_JOINT_NAMES = new string[][] {
            new string[] {"head", "neck"},
            new string[] {"neck", "rShldr"},
            new string[] {"rShldr", "rForeArm"},
            new string[] {"rForeArm", "rHand"},
            new string[] {"neck", "lShldr"},
            new string[] {"lShldr", "lForeArm"},
            new string[] {"lForeArm", "lHand"},
            new string[] {"neck", "hip"},
            new string[] {"hip", "rButtock"},
            new string[] {"rButtock", "rShin"},
            new string[] {"rShin", "rFoot"},
            new string[] {"hip", "lButtock"},
            new string[] {"lButtock", "lShin"},
            new string[] {"lShin", "lFoot"},
        };
        private static readonly string[] JOINT_NAME = new string[] {
            "head", /* Nose */
            "neck", /* Neck */
            "rShldr", /* RShoulder */
            "rForeArm", /* RElbow */
            "rHand", /* RWrist */
            "lShldr", /* LShoulder */
            "lForeArm", /* LElbow */
            "lHand", /* LWrist */
            "hip", /* MidHip */
            "rButtock", /* RHip */
            "rShin", /* RKnee */
            "rFoot", /* RAnkle */
            "lButtock", /* LHip */
            "lShin", /* LKnee */
            "lFoot", /* LAnkle */
        };

        public static int GetJointIndex(string jointName)
        {
            int index = -1;
            for (int i = 0; i < JOINT_NAME.Length; i++)
            {
                if (JOINT_NAME[i] == jointName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        public static string GetJointName(int jointIndex)
        {
            return JOINT_NAME[jointIndex];
        }
    }
}


