// OpenPose Unity Plugin v1.0.0alpha-1.5.0
using UnityEngine;
using UnityEngine.UI;

namespace Project2
{
    /*
     * Visualize face/head circle according to situations (for better looking):
     * If face is disabled, draw simple circle on nose
     * If face is enabled && keypoints detected >= 20, draw circle according to the kepoints rect
     * If face is enabled && keypoints detected < 20, draw circle according to the FaceRectangle output
     */
    [RequireComponent(typeof(Image))]
    public class MyRenderHeadCircle : MonoBehaviour
    {
        public float headSize = 50;
        // Face center joint (nose)
        [SerializeField] RectTransform faceCenter;

        private RectTransform rectTransform { get { return transform as RectTransform; } }
        private Image image { get { return GetComponent<Image>(); } }

        // Update is called once per frame
        void Update()
        {
            image.enabled = faceCenter.gameObject.activeInHierarchy;
            rectTransform.sizeDelta = Vector2.one * headSize;
            rectTransform.localPosition = faceCenter.localPosition;
        }
    }
}
