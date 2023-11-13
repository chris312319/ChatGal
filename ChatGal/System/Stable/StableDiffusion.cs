using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System;

public class Payload
{
    public string prompt;
    public string negative_prompt;
    public int steps;
    public int width;
    public int height;
}
public class Callback
{
    public string[] images;
    public string parameters;
    public string info;
}
public class StableDiffusion : MonoBehaviour
{
    // Start is called before the first frame update
    public DrawSystem draw;
    public string url;
    public Sprite source;
    public GameObject Wall360;
    [SerializeField] public Payload payload;
    public int steps;
    public int width;
    public int height;
    public Image image;
    [TextArea]
    public string ex_prompt;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //StartCoroutine(SendRequest());
        }
    }
    public IEnumerator SendRequest(string prompt,string negative,int opcode,Image image = null,GameObject obj = null)
    {
        payload = new Payload();
        payload.prompt = prompt + ex_prompt;
        payload.negative_prompt = negative;
        payload.steps = steps;
        payload.width = width;
        payload.height = height;
        Debug.Log(JsonUtility.ToJson(payload));
        using (UnityWebRequest request = UnityWebRequest.Post(url + "/sdapi/v1/txt2img", "POST")) 
        {
            string _jsonText = JsonUtility.ToJson(payload);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            string message = request.downloadHandler.text;
            if (request.responseCode == 200)
            {
                Callback callback = JsonUtility.FromJson<Callback>(message);
                Texture2D tex = CreateTexture(callback.images[0]);
                switch (opcode)
                {
                    case 0:
                        SetSprite(image, tex, tex.width, tex.height);
                        Create360(tex);
                        break;
                    case 1:
                        draw.SaveBase64(callback.images[0]);
                        Texture2D temp = CreateTexture(callback.images[0]);
                        SetSprite(draw.image, temp, temp.width, temp.height);
                        draw.SetButton(true);
                        break;
                }
            }
            else
            {
                Debug.Log(message);
                switch (opcode)
                {
                    case 0:
                        break;
                    case 1:
                        draw.SetButton(true);
                        break;
                }
            }
            request.Dispose();
        }
    }
    public IEnumerator RequestTexture(System.Uri url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url.AbsoluteUri);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
            yield break;
        }
        if (request.isDone)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
    static public void SetSprite(Image target, Texture2D texture, float width, float height)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, width, height), new Vector2(0.5f, 0.5f), 100.0f);
        target.sprite = sprite;
    }
    static public void SetMaterial(GameObject obj, Material material)
    {
        MeshRenderer[] meshs = obj.GetComponentsInChildren<MeshRenderer>();
        MeshRenderer Pmesh = obj.GetComponent<MeshRenderer>();
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material = material;
        }
        Pmesh.material = material;
     }
    static public Material CreateMaterial(Texture2D texture)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Nature/SpeedTree7"));
        mat.mainTexture = texture;
        return mat;
    }
    static public Material CreateInsideMaterial(Texture2D texture)
    {
        Material mat = new Material(Shader.Find("InsideVisible"));
        mat.mainTexture = texture;
        return mat;
    }
    static public Texture2D CreateTexture(string base64)
    {
        byte[] imageBytes = Convert.FromBase64String(base64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        return tex;
    }
    public void Create360(Texture2D tex)
    {
        Material mat = CreateInsideMaterial(tex);
        mat.SetTextureScale("_MainTex", new Vector2(1, 1));
        Wall360.GetComponent<MeshRenderer>().material = mat;
    }
}
