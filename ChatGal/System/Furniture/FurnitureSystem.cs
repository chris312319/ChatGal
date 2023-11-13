
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FurnitureSystem : MonoBehaviour
{
    // Start is called before the first frame update
    public DrawSystem draw;
    public int Amount;
    public List<Furniture> Furnitures = new List<Furniture>();
    public GameObject[] FurnitureObjs;
    void Start()
    {
        for(int i = 0; i < Amount; i++)
        {
            string Name = Enum.GetName(typeof(LocationType), i);
            if(GameObject.Find(Name)) FurnitureObjs[i] = GameObject.Find(Name);           
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Action()
    {
        for (int i = 0; i < Amount; i++)
        {
            if(Float2Vector3(Furnitures[i].Position) != Vector3.zero) FurnitureObjs[i].transform.position = Float2Vector3(Furnitures[i].Position);
            FurnitureObjs[i].transform.eulerAngles = Float2Vector3(Furnitures[i].Rotation);
            draw.Base64[i] = Furnitures[i].Base64;
            if (FurnitureObjs[i].GetComponent<DrawObject>()) 
            {
                FurnitureObjs[i].GetComponent<DrawObject>().Initial();
                Debug.Log("Action " + i);
                FurnitureObjs[i].GetComponent<DrawObject>().Action();
            }
        }
    }
    public void SavePos()
    {
        for(int i = 0; i < Amount; i++)
        {
            Furnitures[i].Position = Vector32Float(FurnitureObjs[i].transform.position);
            Furnitures[i].Rotation = Vector32Float(FurnitureObjs[i].transform.eulerAngles);
        }
    }
    public Vector3 Float2Vector3(float[] list)
    {
        Vector3 vector3 = new Vector3();
        if(list.Length == 3)
        {
            vector3.x = list[0];
            vector3.y = list[1];
            vector3.z = list[2];
        }
        return vector3;
    }
    public float[] Vector32Float(Vector3 vector3)
    {
        float[] f = new float[3];
        f[0] = vector3.x;
        f[1] = vector3.y;
        f[2] = vector3.z;
        return f;
    }
}
