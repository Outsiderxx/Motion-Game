﻿using System;
using System.Collections.Generic;
using UnityEngine;

// This class uses no Unity data types and should be completely safe to use in another thread
public class BVHParser
{
    public int frames = 0;
    public float frameTime = 1000f / 60f;
    public BVHBone root;
    private List<BVHBone> boneList;

    static private char[] charMap = null;
    private float[][] channels;
    private string bvhText;
    private int pos = 0;

    public List<BVHBone> allBones
    {
        get
        {
            return this.boneList;
        }
    }

    public class BVHBone
    {
        public string name;
        public BVHBone parent;
        public List<BVHBone> children;
        public float offsetX, offsetY, offsetZ;
        public float endSiteOffsetX = 0, endSiteOffsetY = 0, endSiteOffsetZ = 0;
        public int[] channelOrder;
        public int channelNumber;
        public Quaternion[] quaternions;
        public BVHChannel[] channels;

        private BVHParser bp;

        public bool isEndEffector
        {
            get
            {
                return this.children.Count == 0;
            }
        }

        public bool isRoot
        {
            get
            {
                return this.parent == null;
            }
        }

        // 0 = Xpos, 1 = Ypos, 2 = Zpos, 3 = Xrot, 4 = Yrot, 5 = Zrot
        public struct BVHChannel
        {
            public bool enabled;
            public float[] values;
        }

        public BVHBone(BVHParser parser, bool rootBone)
        {
            bp = parser;
            bp.boneList.Add(this);
            channels = new BVHChannel[6];
            channelOrder = new int[6] { 0, 1, 2, 5, 3, 4 };
            children = new List<BVHBone>();

            bp.skip();
            if (rootBone)
            {
                bp.assureExpect("ROOT");
            }
            else
            {
                bp.assureExpect("JOINT");
            }
            bp.assure("joint name", bp.getString(out name));
            bp.skip();
            bp.assureExpect("{");
            bp.skip();
            bp.assureExpect("OFFSET");
            bp.skip();
            bp.assure("offset X", bp.getFloat(out offsetX));
            bp.skip();
            bp.assure("offset Y", bp.getFloat(out offsetY));
            bp.skip();
            bp.assure("offset Z", bp.getFloat(out offsetZ));
            bp.skip();
            bp.assureExpect("CHANNELS");

            bp.skip();
            bp.assure("channel number", bp.getInt(out channelNumber));
            bp.assure("valid channel number", channelNumber >= 1 && channelNumber <= 6);

            for (int i = 0; i < channelNumber; i++)
            {
                bp.skip();
                int channelId;
                bp.assure("channel ID", bp.getChannel(out channelId));
                channelOrder[i] = channelId;
                channels[channelId].enabled = true;
            }

            char peek = ' ';
            do
            {
                float ignored;
                bp.skip();
                bp.assure("child joint", bp.peek(out peek));
                switch (peek)
                {
                    case 'J':
                        BVHBone child = new BVHBone(bp, false);
                        children.Add(child);
                        child.parent = this;
                        break;
                    case 'E':
                        bp.assureExpect("End Site");
                        bp.skip();
                        bp.assureExpect("{");
                        bp.skip();
                        bp.assureExpect("OFFSET");
                        bp.skip();
                        bp.assure("end site offset X", bp.getFloat(out ignored));
                        this.endSiteOffsetX = ignored;
                        bp.skip();
                        bp.assure("end site offset Y", bp.getFloat(out ignored));
                        this.endSiteOffsetY = ignored;
                        bp.skip();
                        bp.assure("end site offset Z", bp.getFloat(out ignored));
                        this.endSiteOffsetZ = ignored;
                        bp.skip();
                        bp.assureExpect("}");
                        break;
                    case '}':
                        bp.assureExpect("}");
                        break;
                    default:
                        bp.assure("child joint", false);
                        break;
                }
            } while (peek != '}');
        }

        public Vector3 GetWorldPosition(int frameIndex)
        {
            if (frameIndex >= quaternions.Length)
            {
                throw new Exception("Frame index overflow");
            }
            if (parent == null)
            {
                return new Vector3(offsetX, offsetY, offsetZ) + new Vector3(channels[0].values[frameIndex], channels[1].values[frameIndex], channels[2].values[frameIndex]);
            }
            return parent.GetWorldPosition(frameIndex) + parent.GetGlobalQuaternion(frameIndex) * new Vector3(offsetX, offsetY, offsetZ);
        }

        public Vector3 GetWorldPosAfterScale(int frameIndex, float scale)
        {
            if (frameIndex >= quaternions.Length)
            {
                throw new Exception("Frame index overflow");
            }
            if (parent == null)
            {
                return new Vector3(offsetX, offsetY, offsetZ) * scale + new Vector3(channels[0].values[frameIndex], channels[1].values[frameIndex], channels[2].values[frameIndex]);
            }
            return parent.GetWorldPosAfterScale(frameIndex, scale) + parent.GetGlobalQuaternion(frameIndex) * (new Vector3(offsetX, offsetY, offsetZ) * scale);
        }

        private Quaternion GetGlobalQuaternion(int frameIndex)
        {
            if (parent == null)
            {
                return this.quaternions[frameIndex];
            }
            return parent.GetGlobalQuaternion(frameIndex) * this.quaternions[frameIndex];
        }
    }

    public BVHBone GetBone(string boneName)
    {
        return this.allBones.Find((bone) => bone.name == boneName);
    }

    public static Vector3 FindTotalOffset(BVHBone bone)
    {
        Vector3 result = Vector3.zero;
        if (bone.parent != null)
        {
            result += BVHParser.FindTotalOffset(bone.parent);
        }
        result += new Vector3(bone.offsetX, bone.offsetY, bone.offsetZ);
        if (bone.isEndEffector)
        {
            result += new Vector3(bone.endSiteOffsetX, bone.endSiteOffsetY, bone.endSiteOffsetZ);
        }
        return result;
    }

    private bool peek(out char c)
    {
        c = ' ';
        if (pos >= bvhText.Length)
        {
            return false;
        }
        c = bvhText[pos];
        return true;
    }

    private bool expect(string text)
    {
        foreach (char c in text)
        {
            if (pos >= bvhText.Length || (c != bvhText[pos] && bvhText[pos] < 256 && c != charMap[bvhText[pos]]))
            {
                return false;
            }
            pos++;
        }
        return true;
    }

    private bool getString(out string text)
    {
        text = "";
        while (pos < bvhText.Length && bvhText[pos] != '\n' && bvhText[pos] != '\r')
        {
            text += bvhText[pos++];
        }
        text = text.Trim();

        return (text.Length != 0);
    }

    private bool getChannel(out int channel)
    {
        channel = -1;
        if (pos + 1 >= bvhText.Length)
        {
            return false;
        }
        switch (bvhText[pos])
        {
            case 'x':
            case 'X':
                channel = 0;
                break;
            case 'y':
            case 'Y':
                channel = 1;
                break;
            case 'z':
            case 'Z':
                channel = 2;
                break;
            default:
                return false;
        }
        pos++;
        switch (bvhText[pos])
        {
            case 'p':
            case 'P':
                pos++;
                return expect("osition");
            case 'r':
            case 'R':
                pos++;
                channel += 3;
                return expect("otation");
            default:
                return false;
        }
    }

    private bool getInt(out int v)
    {
        bool negate = false;
        bool digitFound = false;
        v = 0;

        // Read sign
        if (pos < bvhText.Length && bvhText[pos] == '-')
        {
            negate = true;
            pos++;
        }
        else if (pos < bvhText.Length && bvhText[pos] == '+')
        {
            pos++;
        }

        // Read digits
        while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9')
        {
            v = v * 10 + (int)(bvhText[pos++] - '0');
            digitFound = true;
        }

        // Finalize
        if (negate)
        {
            v *= -1;
        }
        if (!digitFound)
        {
            v = -1;
        }
        return digitFound;
    }

    // Accuracy looks okay
    private bool getFloat(out float v)
    {
        bool negate = false;
        bool digitFound = false;
        int i = 0;
        v = 0f;

        // Read sign
        if (pos < bvhText.Length && bvhText[pos] == '-')
        {
            negate = true;
            pos++;
        }
        else if (pos < bvhText.Length && bvhText[pos] == '+')
        {
            pos++;
        }

        // Read digits before decimal point
        while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9')
        {
            v = v * 10 + (float)(bvhText[pos++] - '0');
            digitFound = true;
        }

        // Read decimal point
        if (pos < bvhText.Length && (bvhText[pos] == '.' || bvhText[pos] == ','))
        {
            pos++;

            // Read digits after decimal
            float fac = 0.1f;
            while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9' && i < 128)
            {
                v += fac * (float)(bvhText[pos++] - '0');
                fac *= 0.1f;
                digitFound = true;
            }
        }

        // Read exponential sign
        if (pos < bvhText.Length && bvhText[pos] == 'e')
        {
            pos++;

            bool exponentialNegative = false;
            if (pos < bvhText.Length && bvhText[pos] == '-')
            {
                exponentialNegative = true;
                pos++;
            }

            // Read digits after exponential sign
            float u = 0;
            while (pos < bvhText.Length && bvhText[pos] >= '0' && bvhText[pos] <= '9' && i < 128)
            {
                u = u * 10 + (bvhText[pos++] - '0');
                digitFound = true;
            }

            if (exponentialNegative)
            {
                u *= -1f;
            }
            v *= Mathf.Pow(10, u);
        }


        // Finalize
        if (negate)
        {
            v *= -1f;
        }
        if (!digitFound)
        {
            v = float.NaN;
        }
        return digitFound;
    }

    private void skip()
    {
        while (pos < bvhText.Length && (bvhText[pos] == ' ' || bvhText[pos] == '\t' || bvhText[pos] == '\n' || bvhText[pos] == '\r'))
        {
            pos++;
        }
    }

    private void skipInLine()
    {
        while (pos < bvhText.Length && (bvhText[pos] == ' ' || bvhText[pos] == '\t'))
        {
            pos++;
        }
    }

    private void newline()
    {
        bool foundNewline = false;
        skipInLine();
        while (pos < bvhText.Length && (bvhText[pos] == '\n' || bvhText[pos] == '\r'))
        {
            foundNewline = true;
            pos++;
        }
        assure("newline", foundNewline);
    }

    private void assure(string what, bool result)
    {
        if (!result)
        {
            string errorRegion = "";
            for (int i = Math.Max(0, pos - 15); i < Math.Min(bvhText.Length, pos + 15); i++)
            {
                if (i == pos - 1)
                {
                    errorRegion += ">>>";
                }
                errorRegion += bvhText[i];
                if (i == pos + 1)
                {
                    errorRegion += "<<<";
                }
            }
            throw new ArgumentException("Failed to parse BVH data at position " + pos + ". Expected " + what + " around here: " + errorRegion);
        }
    }

    private void assureExpect(string text)
    {
        assure(text, expect(text));
    }

    /*private void tryCustomFloats(string[] floats) {
        float total = 0f;
        foreach (string f in floats) {
            pos = 0;
            bvhText = f;
            float v;
            getFloat(out v);
            total += v;
        }
        Debug.Log("Custom: " + total);
    }

    private void tryStandardFloats(string[] floats) {
        IFormatProvider fp = CultureInfo.InvariantCulture;
        float total = 0f;
        foreach (string f in floats) {
            float v = float.Parse(f, fp);
            total += v;
        }
        Debug.Log("Standard: " + total);
    }

    private void tryCustomInts(string[] ints) {
        int total = 0;
        foreach (string i in ints) {
            pos = 0;
            bvhText = i;
            int v;
            getInt(out v);
            total += v;
        }
        Debug.Log("Custom: " + total);
    }

    private void tryStandardInts(string[] ints) {
        IFormatProvider fp = CultureInfo.InvariantCulture;
        int total = 0;
        foreach (string i in ints) {
            int v = int.Parse(i, fp);
            total += v;
        }
        Debug.Log("Standard: " + total);
    }

    public void benchmark () {
        string[] floats = new string[105018];
        string[] ints = new string[105018];
        for (int i = 0; i < floats.Length; i++) {
            floats[i] = UnityEngine.Random.Range(-180f, 180f).ToString();
        }
        for (int i = 0; i < ints.Length; i++) {
            ints[i] = ((int)Mathf.Round(UnityEngine.Random.Range(-180f, 18000f))).ToString();
        }
        tryCustomFloats(floats);
        tryStandardFloats(floats);
        tryCustomInts(ints);
        tryStandardInts(ints);
    }*/

    private void parse(bool overrideFrameTime, float time)
    {
        // Prepare character table
        if (charMap == null)
        {
            charMap = new char[256];
            for (int i = 0; i < 256; i++)
            {
                if (i >= 'a' && i <= 'z')
                {
                    charMap[i] = (char)(i - 'a' + 'A');
                }
                else if (i == '\t' || i == '\n' || i == '\r')
                {
                    charMap[i] = ' ';
                }
                else
                {
                    charMap[i] = (char)i;
                }
            }
        }

        // Parse skeleton
        skip();
        assureExpect("HIERARCHY");

        boneList = new List<BVHBone>();
        root = new BVHBone(this, true);

        // Parse meta data
        skip();
        assureExpect("MOTION");
        skip();
        assureExpect("FRAMES:");
        skip();
        assure("frame number", getInt(out frames));
        skip();
        assureExpect("FRAME TIME:");
        skip();
        assure("frame time", getFloat(out frameTime));

        if (overrideFrameTime)
        {
            frameTime = time;
        }

        // Prepare channels
        int totalChannels = 0;
        foreach (BVHBone bone in boneList)
        {
            totalChannels += bone.channelNumber;
        }
        int channel = 0;
        channels = new float[totalChannels][];
        foreach (BVHBone bone in boneList)
        {
            for (int i = 0; i < bone.channelNumber; i++)
            {
                channels[channel] = new float[frames];
                bone.channels[bone.channelOrder[i]].values = channels[channel++];
            }
        }

        // Parse frames
        for (int i = 0; i < frames; i++)
        {
            newline();
            for (channel = 0; channel < totalChannels; channel++)
            {
                skipInLine();
                assure("channel value", getFloat(out channels[channel][i]));
            }
        }

        this.handle();
    }

    private void handle()
    {
        for (int i = 0; i < this.frames; i++)
        {
            this.root.channels[0].values[i] *= -1;
        }
        foreach (var jointData in this.allBones)
        {
            jointData.offsetX *= -1;
            jointData.endSiteOffsetX *= -1;
            for (int i = 0; i < this.frames; i++)
            {
                jointData.channels[4].values[i] *= -1;
                jointData.channels[5].values[i] *= -1;
            }
        }


        foreach (BVHParser.BVHBone bone in this.allBones)
        {
            bone.quaternions = new Quaternion[this.frames];
            for (int i = 0; i < this.frames; i++)
            {
                for (int j = 3; j < 6; j++)
                {
                    int channelID = bone.channelOrder[j];
                    if (channelID == 3)
                    {
                        bone.quaternions[i] *= Quaternion.AngleAxis(bone.channels[3].values[i], Vector3.right);
                    }
                    else if (channelID == 4)
                    {
                        bone.quaternions[i] *= Quaternion.AngleAxis(bone.channels[4].values[i], Vector3.up);
                    }
                    else
                    {
                        bone.quaternions[i] = Quaternion.AngleAxis(bone.channels[5].values[i], Vector3.forward);
                    }
                }
            }
        }
    }

    public BVHParser(string bvhText)
    {
        this.bvhText = bvhText;

        parse(false, 0f);
    }

    public BVHParser(string bvhText, float time)
    {
        this.bvhText = bvhText;

        parse(true, time);
    }
}
