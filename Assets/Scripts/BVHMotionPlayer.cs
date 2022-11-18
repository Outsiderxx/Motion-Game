using System;
using System.IO;
using UnityEngine;

namespace Project2
{

    public class BVHMotionPlayer : MonoBehaviour
    {
        public bool isRepeat = false;
        public float speed = 1;
        [SerializeField] private string motionName;

        private BVHBodyDrawer drawer;
        private BVHParser _bvhData;
        private int _currentFrameIndex;
        private float currentTime = 0;
        public bool isPlaying = false;

        public System.Action isEnd;

        public BVHParser bvhData
        {
            get
            {
                return this._bvhData;
            }
        }

        public int currentFrameIndex
        {
            get
            {
                return this._currentFrameIndex;
            }
            set
            {
                if (value == this._currentFrameIndex)
                {
                    return;
                }
                if (value >= this._bvhData.frames)
                {
                    if (this.isEnd != null)
                    {
                        this.isEnd.Invoke();
                    }
                    if (!isRepeat)
                    {
                        return;
                    }
                    this._currentFrameIndex = 0;
                    this.currentTime = 0;
                }
                else
                {
                    this._currentFrameIndex = value;
                }
                if (value == 0)
                {
                    this.currentTime = 0;
                }
                this.OnFrameIndexUpdate();
            }
        }

        private void Awake()
        {
            this.drawer = this.GetComponent<BVHBodyDrawer>();
            string bvhString = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "BVHFiles/" + motionName));
            _bvhData = new BVHParser(bvhString);
        }

        public void LoadBvh(string str)
        {

            string bvhString = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "BVHFiles/" + str));
            _bvhData = new BVHParser(bvhString);
        }

        // Update is called once per frame
        private void Update()
        {
            if (this._bvhData == null)
            {
                return;
            }
            if (isPlaying)
            {
                this.currentTime += Time.deltaTime * speed;
                this.currentFrameIndex = (int)(this.currentTime / this._bvhData.frameTime);
            }
            else
            {
                this.currentTime = 0;
                this.currentFrameIndex = 0;
            }
        }

        private void OnFrameIndexUpdate()
        {
            this.drawer.Draw(this._bvhData, this._currentFrameIndex);
        }
    }
}
