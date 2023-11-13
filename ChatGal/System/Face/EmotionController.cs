using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRM;
using BVA;

public class EmotionController : MonoBehaviour
{
    // Start is called before the first frame update
    public ActionSystem action;
    public int count;
    public List<int> EmotionIndex = new List<int>();
    public List<string> EmotionList = new List<string>();
    public List<bool> EmotionActive = new List<bool>();
    public VRM.BlendShapeKey[] keys;
    public VRMBlendShapeProxy vrmproxy;
    public SkinnedMeshRenderer targetBlendShapeObject;

    public int preindex = -1;
    public int index = -1;
    public float timer;
    public float speed = 1f;
    public bool isVrm,isCustom;
    public string CustomHeader;
    public List<CustomBlendShape> CustomBlendShapes;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(index != -1)
        {
            if (isCustom)
            {
                if (timer <= 1)
                {
                    timer += Time.deltaTime * speed;
                    for (int i = 0; i < CustomBlendShapes[index].BlendShapes.Count; i++)
                    {
                        if (CustomBlendShapes[index].BlendShapes[i].BlendShapeIndex != -1)
                        {
                            BlendShapeKey key = CustomBlendShapes[index].BlendShapes[i];
                            targetBlendShapeObject.SetBlendShapeWeight(key.BlendShapeIndex, 100f * ((key.maxoffset - key.minOffset) * timer + key.minOffset));
                        }
                    }
                }
            }
            else if (isVrm)
            {
                    if (EmotionActive[index])
                    {
                        if (timer <= 1)
                        {
                            timer += Time.deltaTime * speed;
                            vrmproxy.ImmediatelySetValue(keys[index], timer);
                        }
                    }
            }
            else
            {
                if(EmotionIndex[index] != -1)
                {
                    if (timer <= 1)
                    {
                        timer += Time.deltaTime * speed;
                        targetBlendShapeObject.SetBlendShapeWeight(EmotionIndex[index], timer * 100f);
                    }
                }
            }
        }
    }
    public void Initial()
    {
        action = GameObject.Find("GPT-Turbo").GetComponent<ActionSystem>();
        action.emotion = this;
        count = (int)EmotionType.Count;
        for (int i = 0; i < count; i++)
        {
            if(isVrm)EmotionList.Add(Enum.GetName(typeof(EmotionType), i));
            else EmotionList.Add(Enum.GetName(typeof(EmotionType_JP), i));
            EmotionIndex.Add(-1);
            EmotionActive.Add(false);
        }
        keys = new VRM.BlendShapeKey[count];
    }
    public void SetCustomBendShape(int index,float value)
    {
        for (int i = 0; i < CustomBlendShapes[index].BlendShapes.Count; i++)
        {
            if (CustomBlendShapes[index].BlendShapes[i].BlendShapeIndex != -1) 
            {
              BlendShapeKey key = CustomBlendShapes[index].BlendShapes[i];
              targetBlendShapeObject.SetBlendShapeWeight(key.BlendShapeIndex, timer * 100f * ((key.maxoffset - key.minOffset) * value + key.minOffset));
            } 
        }
    }
    public void GetCustomBlendShape()
    {
        for(int i = 0; i < CustomBlendShapes.Count; i++)
        {
            for(int j = 0; j < CustomBlendShapes[i].BlendShapes.Count; j++)
            {
                for (int k = 0; k < targetBlendShapeObject.sharedMesh.blendShapeCount; k++)
                {
                    string name = targetBlendShapeObject.sharedMesh.GetBlendShapeName(k);
                    if(name.Contains(CustomHeader + CustomBlendShapes[i].BlendShapes[j].BlendShapeName))
                    {
                        CustomBlendShapes[i].BlendShapes[j].BlendShapeIndex = k;
                    }
                }
            }
        }
    }
    public void GetVrmBlendShape()
    {
        for (int i = 0; i < vrmproxy.BlendShapeAvatar.Clips.Count; i++)
        {
            Debug.Log(vrmproxy.BlendShapeAvatar.Clips[i].BlendShapeName);
            for (int j = 0; j < EmotionList.Count; ++j)
            {
                Debug.Log(EmotionList[j]);
                if (vrmproxy.BlendShapeAvatar.Clips[i].BlendShapeName == EmotionList[j])
                {
                    keys[j] = VRM.BlendShapeKey.CreateFromClip(vrmproxy.BlendShapeAvatar.Clips[i]);
                    EmotionActive[j] = true;
                }
            }
        }
    }
    public void GetBlendShape()
    {
        for(int i = 0; i < targetBlendShapeObject.sharedMesh.blendShapeCount; i++)
        {
            string name = targetBlendShapeObject.sharedMesh.GetBlendShapeName(i);
            for(int j = 0; j < EmotionList.Count; ++j)
            {
                if(name.Contains(EmotionList[j]))
                {
                    EmotionIndex[j] = i;
                }
            }
        }
    }
    public void SetEmotion(int input)
    {
        if(preindex != -1) {
            if (isCustom)
            {
                for(int i = 0; i < CustomBlendShapes[preindex].BlendShapes.Count; i++)
                {
                    if(CustomBlendShapes[preindex].BlendShapes[i].BlendShapeIndex != -1) targetBlendShapeObject.SetBlendShapeWeight(CustomBlendShapes[preindex].BlendShapes[i].BlendShapeIndex, 0);
                }
            }
            if (EmotionActive[preindex])
            {
                vrmproxy.ImmediatelySetValue(keys[preindex], 0);
            }
            if(EmotionIndex[preindex] != -1)
            {
                targetBlendShapeObject.SetBlendShapeWeight(EmotionIndex[preindex],0);
            }
        }
        index = input;
        preindex = index;
        timer = 0;
    }
}
[System.Serializable]
public class CustomBlendShape
{
    public List<BlendShapeKey> BlendShapes = new List<BlendShapeKey>();
}
[System.Serializable]
public class BlendShapeKey
{
    public string BlendShapeName;
    public int BlendShapeIndex = -1;
    public float minOffset = 0;
    public float maxoffset = 1;
}
