using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawObject : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject player;
    FurnitureSystem furniture;
    DrawSystem drawsystem;
    GptTurboScript gpt;
    public int index;
    Material Pmat;
    List<Material> materials = new List<Material>(0);
    public float InteractDis;
    bool isMouse = false;
    void Start()
    {

    }
    public void Initial()
    {
        player = GameObject.FindWithTag("Player");
        drawsystem = GameObject.Find("DrawSystem").GetComponent<DrawSystem>();
        gpt = GameObject.Find("GPT-Turbo").GetComponent<GptTurboScript>();
        furniture = GameObject.Find("FurnitureSystem").GetComponent<FurnitureSystem>();
        if (this.GetComponent<MeshRenderer>()) Pmat = this.GetComponent<MeshRenderer>().material;
        MeshRenderer[] meshs = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshs)
        {
            materials.Add(mesh.material);
        }
    }
    // Update is called once per frame
    void Update()
    {
        Active();
    }
    public void Active(bool isChild = false)
    {
        if (gpt.isDraw && !EventSystem.current.IsPointerOverGameObject() && Vector3.Distance(this.gameObject.transform.position,player.transform.position) < InteractDis && Input.GetKeyDown(KeyCode.R))
        {
            if(isMouse || isChild)
            {
                drawsystem.Open(this.gameObject, index);
            }
        }
    }
    public void Action()
    {
        string base64 = furniture.Furnitures[index].Base64;
        if (base64 != null && base64.Length > 100) 
        {
            Texture2D tex = StableDiffusion.CreateTexture(base64);
            Material mat = StableDiffusion.CreateMaterial(tex);
            StableDiffusion.SetMaterial(this.gameObject, mat);
        }
    }
    public void ResetMaterial()
    {
        if(Pmat) this.GetComponent<MeshRenderer>().material = Pmat;
        MeshRenderer[] meshs = this.GetComponentsInChildren<MeshRenderer>();
        for(int i = 0; i < meshs.Length; i++)
        {
            meshs[i].material = materials[i];
        }
    }
    public void OnMouseEnter()
    {
        isMouse = true;
    }
    public void OnMouseExit()
    {
        isMouse = false;
    }
}
