using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class ActionSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public BehaviourSystem behaviour;
    public EmotionController emotion;
    public MemorySystem memory;
    public NavMeshAgent agent;
    public TipsSystem tip;
    public BackpackSystem backpack;
    public ModelLoader model;

    public List<AvailableItem> ObjectList = new List<AvailableItem>();
    public List<Location> LocationList = new List<Location>();
    public List<Emotion> EmotionList = new List<Emotion>();
    public List<ActionA> ActionList = new List<ActionA>();

    public List<ActionP> actionPList = new List<ActionP>();

    public int LocationIndex;
    public int ActionIndex;
    public int EmotionIndex;
    public int ObjectIndex;
    public bool choice;
    public bool isAction;
    public float minDis;
    public float rotateSpeed;

    public Vector3 targetpos;
    bool isMove,isArriveed;
    public int isMemory;
    public Interactable preInteract;

    void Start()
    {
        ResetIndex();
        SetList();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMove)
        {
            /*Vector3 targetDir = Vector3_h(targetpos, model.gameObject.transform.position) - model.gameObject.transform.position;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotateSpeed * Time.deltaTime, 0.0F);
            model.gameObject.transform.rotation = Quaternion.LookRotation(newDir);*/
            //Debug.Log(agent.pathEndPosition);
            model.gameObject.transform.LookAt(Vector3_h(agent.nextPosition, model.gameObject.transform.position));
            if (Vector3.Distance(model.gameObject.transform.position, targetpos) < minDis) isArriveed = true;
        }
    }
    public void Parser(Content response,int opcode = -1)
    {
        ResetIndex();
        switch (opcode)
        {
            case -1:
                ActionParser(response.action);
                LocationParser(response.target);
                EmotionParser(response.emotion);
                ObjectParser(response.item);
                ChoiceParser(response.choice);
                break;
            case 0:
                ActionParser(response.action);
                LocationParser(response.target);
                EmotionParser(response.emotion);
                break;
        }
    }
    public void ResetIndex()
    {       
        ActionIndex = -1;
        LocationIndex = -1;
        EmotionIndex = -1;
        ObjectIndex = -1;
        choice = false;
    }
    public void LocationParser(string response)
    {
        for (int i = 1; i < LocationList.Count; i++)
        {
            if (response.Contains(LocationList[i].name))
            {
                LocationIndex = i;
                break;
            }
        }
    }
    public void EmotionParser(string response)
    {
        for (int i = 0; i < EmotionList.Count; i++)
        {
            if (response.Contains(EmotionList[i].name))
            {
                EmotionIndex = i;
                break;
            }
        }
    }
    public void ActionParser(string response)
    {
        for(int i = 0; i < ActionList.Count; i++)
        {
            if (response.Contains(ActionList[i].name) && !response.Contains("Think"))
            {
                ActionIndex = i;
                break;
            }
        }
    }
    public void ObjectParser(string response)
    {
        for (int i = 0; i < ObjectList.Count; i++)
        {
            if (response.Contains(ObjectList[i].name) || response.Contains(ObjectList[i].prompt))
            {
                ObjectIndex = i;
                break;
            }
        }
    }
    public void ChoiceParser(bool flag)
    {
        choice = flag;
    }
    public void EmotionAction()
    {
        if (EmotionIndex != -1)
        {
            if (emotion) emotion.SetEmotion(EmotionIndex);
        }
    }
    public void Action(int ismemory = 0)
    {
        if (ismemory == 0 && isMemory == 2) isMemory = 1;
        else isMemory = ismemory;
        if (ObjectIndex != -1 && choice)
        {
            ResetInteract();
            isAction = true;
            ActionIndex = (int)BehaviourType.Give;
            model.Play(ActionIndex);
            backpack.Add(ObjectIndex, 1);
            tip.Open("獲得" + backpack.itemInfos[ObjectIndex].name);
            StopCoroutine(ActionClose());
            StartCoroutine(ActionClose());
        }
        else if (ActionIndex != -1) 
        {
            ResetInteract();
            isAction = true;
            if (LocationIndex == -1) 
            {
                Debug.Log("執行" + ActionIndex);
                model.Play(ActionIndex);
                StopCoroutine(ActionClose());
                StartCoroutine(ActionClose());
            }
            else
            {
                StopCoroutine(ActionClose());
                StartCoroutine(Move());
            }
        }
        else if(LocationIndex != -1)
        {
            ResetInteract();
            isAction = true;
            StopCoroutine(ActionClose());
            StartCoroutine(Move());
        }     
        CurrentState state = new CurrentState();
        state.behaviour = (BehaviourType)(ActionIndex>-1?ActionIndex:0);
        state.location = (LocationType)(LocationIndex>-1?LocationIndex:0);
        state.emotion = (EmotionType)(EmotionIndex>-1?EmotionIndex:0);
        state.item = ObjectIndex > -1 ? ObjectList[ObjectIndex].prompt : "";
        memory.SetState(state);
    }
    public void ResetInteract()
    {
        if (preInteract)
        {
            for(int i = 0; i < preInteract.Capacity; i++)
            {
                if (preInteract.isOn[i] == 1)
                {
                    preInteract.ResetInteract(i);
                }
            }
        }
        preInteract = null;
    }
    public IEnumerator Move()
    {
        isMove = true;
        if(LocationList[LocationIndex].capacity == 1)
        {
            targetpos = Vector3_h(LocationList[LocationIndex].pos[0].transform.position, model.gameObject.transform.position);
            agent.SetDestination(Vector3_y(LocationList[LocationIndex].pos[0].transform.position));
        }
        else
        {
            for (int i = 0; i < LocationList[LocationIndex].capacity; i++)
            {
                if (LocationList[LocationIndex].obj.GetComponent<Interactable>().isOn[i] == 0)
                {
                    targetpos = Vector3_h(LocationList[LocationIndex].pos[i].transform.position, model.gameObject.transform.position);
                    agent.SetDestination(Vector3_y(LocationList[LocationIndex].pos[i].transform.position));
                    break;
                }
            }
        }
        Debug.Log("Set");
        model.Play((int)BehaviourType.Walk);
        yield return new WaitUntil(() => isArriveed);
        agent.ResetPath();
        isArriveed = false;
        isMove = false;
        if (ActionIndex != -1 && ActionIndex != (int)BehaviourType.Walk)
        {
            if(LocationList[LocationIndex].interacttype != BehaviourType.Idle)
            {
                if (LocationList[LocationIndex].interacttype.ToString() == ActionList[ActionIndex].name)
                {
                    preInteract = LocationList[LocationIndex].obj.GetComponent<Interactable>();
                    bool flag = false;
                    for(int i = 0; i < preInteract.Capacity; i++)
                    {
                        if (preInteract.isOn[i] == 0)
                        {
                            preInteract.AIInteract(i);
                            flag = true;
                            break;
                        }
                    }
                    if(!flag)
                    {
                        preInteract = null;
                        ActionIndex = 0;
                        isAction = false;
                    }
                }
            }
        }
        else 
        { 
            ActionIndex = 0;
            isAction = false;
        } 
        model.Play(ActionIndex);
        StartCoroutine(ActionClose());
    }
    public IEnumerator ActionClose()
    {
        yield return new WaitForSeconds(ActionList[ActionIndex].time);
        model.Play((int)BehaviourType.Idle);
        ActionIndex = -1;
        isAction = false;
        if (behaviour.timer > 0 && isMemory == 1) 
        {
            isMemory = 2;
            behaviour.Continue();
        }
        if (behaviour.timer <= 0) 
        {
            behaviour.Stop();
            isMemory = 0;
        }
        if (behaviour.timer < 30) behaviour.timer = 30;
    }
    public void SetList()
    {
        int count = 0;
        count = backpack.itemInfos.Count;
        for(int i = 0; i < count; i++)
        {
            AvailableItem item = new AvailableItem();
            item.prompt = backpack.itemInfos[i].prompt;
            item.name = backpack.itemInfos[i].name;
            item.favorability = backpack.itemInfos[i].favorability;
            ObjectList.Add(item);
        }
        count = model.ActionList.Count;
        for (int i = 0; i < count; i++)
        {
            ActionList.Add(new ActionA() { name = model.ActionList[i].name, time = model.ActionList[i].time });
        }
        ActionList action = FileManager.LoadAction();
        actionPList = action.list;
        for(int i = 0; i < actionPList.Count; i++)
        {
            ActionA temp = new ActionA();
            temp.name = actionPList[i].prompt;
            temp.time = actionPList[i].time;
            ActionList.Add(temp);
            ActionM tempM = new ActionM();
            tempM.name = actionPList[i].path;
            tempM.type = 1;
            tempM.time = actionPList[i].time;
            model.ActionList.Add(tempM);
        }
        count = (int)LocationType.Count;
        for (int i = 0; i < count; i++)
        {
            string Name = Enum.GetName(typeof(LocationType), i);
            Location location = new Location();
            location.name = Name;
            location.obj = GameObject.Find(Name)? GameObject.Find(Name):null;
            if (location.obj) 
            {
                location.capacity = location.obj.GetComponent<Interactable>() ? location.obj.GetComponent<Interactable>().Capacity : 1;
                location.pos = new GameObject[location.capacity];
                for (int j = 0; j < location.capacity; j++)
                {
                    if (i == 0 || i == 1) location.pos[j] = location.obj;
                    else location.pos[j] = location.obj.transform.GetChild(j).gameObject;
                }
                location.type = location.obj.GetComponent<Interactable>() ? location.obj.GetComponent<Interactable>().type : InteractableType.None;
                location.interacttype = location.obj.GetComponent<Interactable>() ? location.obj.GetComponent<Interactable>().interactType : BehaviourType.Idle;
            }
            LocationList.Add(location);
        }
        Location user = new Location();
        user.name = "User";
        user.obj = GameObject.FindWithTag("Player");
        user.capacity = 1;
        user.pos = new GameObject[1];
        user.pos[0] = user.obj;
        user.type = InteractableType.None;
        user.interacttype = BehaviourType.Idle;
        LocationList.Add(user);
        count = (int)EmotionType.Count;
        for (int i = 0; i < count; i++)
        {
            EmotionList.Add(new Emotion() { name = Enum.GetName(typeof(EmotionType), i) });
        }
    }
    public Vector3 Vector3_y(Vector3 pos)
    {
        return new Vector3(pos.x, 0, pos.z);
    }
    public Vector3 Vector3_h(Vector3 target, Vector3 self)
    {
        return new Vector3(target.x, self.y, target.z);
    }
}
[Serializable]
public class AvailableItem{
    public string prompt;
    public string name;
    public int favorability;
}
[Serializable]
public class Location
{
    public string name;
    public int capacity;
    public GameObject obj;
    public GameObject[] pos;
    public InteractableType type;
    public BehaviourType interacttype;
}
[Serializable]
public class Emotion
{
    public string name;
}
[Serializable]
public class ActionA
{
    public string name;
    public float time;
}

