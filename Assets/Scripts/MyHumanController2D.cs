// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Project2
{
    public class MyHumanController2D : MonoBehaviour
    {
        [SerializeField] RectTransform PoseParent;

        private List<RectTransform> poseJoints = new List<RectTransform>();

        public bool IsDisplayCompletely
        {
            get
            {
                bool isDisplayCompletely = true;
                foreach (Transform child in transform)
                {
                    if (!child.gameObject.activeInHierarchy)
                    {
                        isDisplayCompletely = false;
                    }
                }
                return isDisplayCompletely;
            }
        }

        // Use this for initialization
        void Start()
        {
            InitJoints();
        }

        public void SetColor(Color color)
        {
            foreach (var lineRenderer in this.transform.GetComponentsInChildren<LineRenderer>())
            {
                lineRenderer.startColor = color;
                lineRenderer.endColor = color;
            }
        }

        public Vector3[] GetJointPositions()
        {
            return this.poseJoints.Select(joint => joint.position).ToArray();
        }

        public void UpdateJointPositions(List<Vector3?> positions)
        {
            if (positions == null)
            {
                PoseParent.gameObject.SetActive(false);
            }
            else
            {
                PoseParent.gameObject.SetActive(true);
                // Pose
                for (int part = 0; part < poseJoints.Count; part++)
                {
                    // Joints overflow
                    if (part >= positions.Count)
                    {
                        poseJoints[part].gameObject.SetActive(false);
                        continue;
                    }
                    // Compare score
                    if (positions[part] == null)
                    {
                        poseJoints[part].gameObject.SetActive(false);
                    }
                    else
                    {
                        poseJoints[part].gameObject.SetActive(true);
                        poseJoints[part].localPosition = (Vector3)positions[part];
                    }
                }
            }
        }

        private void InitJoints()
        {
            // Pose
            if (PoseParent)
            {
                Debug.Assert(PoseParent.childCount == MyData.PoseKeypointsCount, "Pose joint count not match");
                for (int i = 0; i < MyData.PoseKeypointsCount; i++)
                {
                    poseJoints.Add(PoseParent.GetChild(i) as RectTransform);
                }
            }
        }
    }
}

