using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;

public class BehaviourSystem : MonoBehaviour
{
    // Start is called before the first frame updatep
    public Setting setting;
    public Moegoe moegoe;
    public GptTurboScript gpt;
    public ChatScript chat;
    public ActionSystem action;
    public PlayableDirector director;
    public string Time;
    public Day day;
    public BehaviourType behaviour;
    public LocationType location;
    public EmotionType emotion;
    public GameObject player,model;
    public float minDis, rotateSpeed;
    public float timer = 10;
    public bool isSend;

    public int preAction, preLocation;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        SetTime();
        if((Vector3.Distance(player.transform.position, model.transform.position) < minDis || (gpt.isWait && !isSend)) && !action.isAction)
        {
            Vector3 targetDir = Vector3_h(player.transform.position, model.transform.position) - model.transform.position;
            Vector3 newDir = Vector3.RotateTowards(model.transform.forward, targetDir, rotateSpeed * UnityEngine.Time.deltaTime, 0.0F);
            model.transform.rotation = Quaternion.LookRotation(newDir);
        }
        if(Vector3.Distance(player.transform.position,model.transform.position) > minDis && !action.isAction && !gpt.isWait && moegoe.isStart && !moegoe.isSpeak && !setting.isOpen)
        {
            timer -= UnityEngine.Time.deltaTime;
        }
        if(timer <= 0 && !isSend && !action.isAction && !gpt.isWait && moegoe.isStart && !moegoe.isSpeak && !setting.isOpen && setting.isBehaviour)
        {
            isSend = true;
            StartCoroutine(gpt.GetPostDataAuto(chat.m_OpenAI_Key));
        }
    }
    public void SetTime()
    {
        Time = System.DateTime.Now + "";
        float hour = System.DateTime.Now.Hour;
        hour += System.DateTime.Now.Minute / 60f + System.DateTime.Now.Second / 3600f;
        director.time = hour;
        if(hour >= 6 && hour < 12)
        {
            day = Day.Morning;
        }
        else if(hour >= 12 && hour < 13)
        {
            day = Day.Noon;
        }
        else if(hour >= 13 && hour < 17)
        {
            day = Day.Afternoon;
        }
        else
        {
            day = Day.Night;
        }
    }
    public void Play()
    {
        preAction = action.ActionIndex;
        preLocation = action.LocationIndex;
        action.Action(2);
    }
    public void Pause()
    {
        
    }
    public void Continue()
    {
        action.ActionIndex = preAction;
        action.LocationIndex = preLocation;
        action.Action(2);
    }
    public void Stop()
    {
        preAction = -1;
        preLocation = -1;
    }
    public Vector3 Vector3_h(Vector3 target, Vector3 self)
    {
        return new Vector3(target.x, self.y, target.z);
    }

}
public enum Day 
{
    Morning,
    Noon,
    Afternoon,
    Night,
    Count
}
public enum BehaviourType
{
    Idle,
    Think,
    //Jump,
    Run,
    Walk,
    Give,
    Sit,
    Count
}
public enum LocationType
{
    Root,
    Door,
    Sofa,
    ComputerChair,
    DiningChair,
    GuestRoom,
    GuestBed,
    BedRoom,
    Bed,
    Television,
    BathRoom,
    Toilet,
    BathTub,
    Kitchen,
    Count
}
public enum EmotionType
{
    Neutral,
    Angry,
    Fun,
    Joy,
    Sorrow,
    Surprised,
    Count
}
public enum EmotionType_JP
{
    ない,
    怒り,
    じと,
    笑い,
    困る,
    あわわ,
    Count
}

