using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    public int Capacity;
    public GameObject[] Pos;
    public int[] isOn; //0=empty,1=ai,2=player
    public InteractableType type;
    public BehaviourType interactType;
    public float InteractDis;
    public int PlayerisOn = -1;
    public int AIisOn = -1;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        Pos = new GameObject[Capacity];
        isOn = new int[Capacity];
        for(int i = 0; i < Capacity; i++)
        {
            Pos[i] = this.transform.GetChild(i).gameObject;
            InteractPos pos = Pos[i].AddComponent<InteractPos>();
            pos.index = i;
            pos.interactable = this;
        }
    }

    // Update is called once per frame
    void Update()
    {      
        if(Input.GetKeyDown(KeyCode.E) && PlayerisOn != -1)
        {
            player.GetComponent<CharacterController>().enabled = true;
            ResetInteract(PlayerisOn);
        }
        if(PlayerisOn != -1)
        {
            if(type == InteractableType.Furniture)
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = Pos[PlayerisOn].transform.position;
            }
        }
        if(AIisOn != -1)
        {
            if (type == InteractableType.Furniture)
            {
        
            }
        }
    }
    public void Interact(int i)
    {
        isOn[i] = 2;
        PlayerisOn = i;
    }
    public void AIInteract(int i)
    {
        isOn[i] = 1;
        AIisOn = i;
    }
    public void ResetInteract(int i)
    {
        isOn[i] = 0;
        if (PlayerisOn == i) PlayerisOn = -1;
        if (AIisOn == i) AIisOn = -1;
    }
}
public enum InteractableType
{
    None,
    Furniture,
    Item
}
