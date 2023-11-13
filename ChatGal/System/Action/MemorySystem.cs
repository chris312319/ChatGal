using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Key
{
    public string Obj;
    public string Event;
    public string Location;
}
public class MemorySystem : MonoBehaviour
{
    // Start is called before the first frame update
    [TextArea]
    public string BehaviourSet;
    public BehaviourSystem behaviour;
    public List<Key> KeyList;
    public int Favorability;
    public TMP_Text favorability;
    public string currentTime;
    public BehaviourType currentBehaviour;
    public LocationType currentLocation;
    public EmotionType currentEmotion;

    public List<Memory> MemoryList = new List<Memory>();

    void Start()
    {
        favorability.text = Favorability + "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetFavorability(int i)
    {
        if(i != -1)
        {
            Favorability = i;
            favorability.text = Favorability + "";
        }
    }
    public void SetState(CurrentState state)
    {
        bool flag = false;
        if (currentBehaviour != state.behaviour || (currentLocation != state.location && state.location != LocationType.Root) || state.item != "")
        {
            flag = true;
        }
        Memory memory = new Memory();
        memory.Time = currentTime = behaviour.Time;
        memory.Behaviour = currentBehaviour = state.behaviour;
        if (state.location == LocationType.Root) memory.Location = currentLocation; 
        else memory.Location = currentLocation = state.location;
        memory.Emotion = currentEmotion = state.emotion;
        memory.Item = state.item;
        if (flag) MemoryList.Add(memory);
    }
    public CurrentState GetState()
    {
        CurrentState state = new CurrentState();
        state.behaviour = currentBehaviour;
        state.location = currentLocation;
        state.emotion = currentEmotion;
        return state;
    }
}
public class CurrentState
{
    public BehaviourType behaviour;
    public LocationType location;
    public EmotionType emotion;
    public string item;
}
[Serializable]
public class Memory
{
    public string Time;
    public BehaviourType Behaviour;
    public LocationType Location;
    public EmotionType Emotion;
    public string Item;
}
