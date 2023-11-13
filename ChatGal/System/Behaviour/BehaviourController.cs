using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BehaviourController : MonoBehaviour
{
    public ActionSystem action;
    public Moegoe moegoe;
    public ModelLoader model;
    public NavMeshAgent agent;
    public float Distance;
    public float minDis;
    public float rotateSpeed;
    GameObject target;
    Vector3 destination;
    public int state,r;
    public float timer,time;
    public bool isAction;
    public float minX, maxX;
    public float minY, maxY;
    public float minTime, maxTime;
    public int[] ActionList;
    void Start()
    {
        target = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (model)
        {
            if (Vector3.Distance(this.transform.position, target.transform.position) < Distance || action.isAction || moegoe.isSpeak)
            {
                if(state != 0)
                {
                    model.Play(0);
                    timer = 0;
                    agent.ResetPath();
                    Debug.Log("Clear");
                    state = 0;
                    isAction = false;
                }
            }
            else
            {
                timer += Time.deltaTime;
                if (state == 0 && timer > minTime)
                {
                    timer = 0;
                    state = -1;
                    isAction = false;
                }
            }
            if (state == -1)
            {
                state = Random.Range(1, 3);
            }
            switch (state)
            {
                case 0:
                    if (!action.isAction)
                    {
                        if (!isAction)
                        {
                            destination = Vector3.zero;
                            isAction = true;
                        }
                        Vector3 targetDir = Vector3_h(target.transform.position, this.transform.position) - this.transform.position;
                        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotateSpeed * Time.deltaTime, 0.0F);
                        this.transform.rotation = Quaternion.LookRotation(newDir);                      
                    }
                    break;
                case 1:
                    if (!isAction)
                    {
                        time = Random.Range(minTime, maxTime);
                        float x = Random.Range(minX, maxX);
                        float y = Random.Range(minY, maxY);
                        destination = new Vector3(x, 0, y);
                        agent.SetDestination(destination);
                        model.Play(3);
                        isAction = true;
                        
                    }
                    /*Vector3 targetDir_ = Vector3_h(destination, this.transform.position) - this.transform.position;
                    Vector3 newDir_ = Vector3.RotateTowards(transform.forward, targetDir_, rotateSpeed * Time.deltaTime, 0.0F);
                    this.transform.rotation = Quaternion.LookRotation(newDir_);*/
                    this.transform.LookAt(Vector3_h(agent.nextPosition, this.transform.position));
                    if (Vector3.Distance(this.transform.position, Vector3_h(destination, this.transform.position)) < minDis)
                    {
                        agent.SetDestination(this.transform.position);
                        model.Play(0);
                        timer += Time.deltaTime;
                        if(timer > time)
                        {
                            time = 0;
                            timer = 0;
                            model.Play(0);
                            state = -1;
                            isAction = false;
                        }
                    }
                    break;
                case 2:
                    if (!isAction)
                    {
                        r = Random.Range(0, ActionList.Length);
                        model.Play(ActionList[r]);
                        isAction = true;
                    }
                    timer += Time.deltaTime;
                    if (timer > model.ActionList[r].time)
                    {
                        timer = 0;
                        model.Play(0);
                        r = -1;
                        state = -1;
                        isAction = false;
                    }
                    break;
            }
        }
    }
    public Vector3 Vector3_h(Vector3 target, Vector3 self)
    {
        return new Vector3(target.x, self.y, target.z);
    }
    public void SetTarget(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}
