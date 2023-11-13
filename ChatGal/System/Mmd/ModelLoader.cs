using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UniGLTF;
using VRM;
using LipSync;
using UniHumanoid;
using System.IO;
using OggVorbis;
using GLTF.Schema.BVA;
using BVA;

public enum ModelType
{
    Mmd,
    Vrm,
    Bva
}
public class ModelLoader : MonoBehaviour
{
    public TipsSystem tip;
    public Setting setting;
    public BehaviourController behaviour;
    public bool StartScene,isCustom;
    public string CustomHeader;
    public RuntimeAnimatorController SampleAnimatorController;
    public GameObject Root;
    Vector3 defaultPosition = new Vector3(0, 0, 0);
    Quaternion defaultRotation = Quaternion.Euler(0, 0, 0);
    public VmdPlayer vmdPlayer;
    public List<ActionM> ActionList;
    public string origin;
    public string[] mouthName_JP;
    public string[] mouthName_EN;
    public List<CustomBlendShape> CustomBlendShapes;
    public ModelType type;
    public Shader urpLitShader;
    Shader urpMToonShader;
    Shader urpUnlitShader;
    public bool isDefault;
    public SkinnedMeshRenderer BlendShapeTarget;
    // Start is called before the first frame update
    public void Start()
    {
        urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        urpMToonShader = Shader.Find("VRM/URP/MToon");      
        urpUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
    }
    public void ChangeMat(GameObject root)
    {
        var skinnedMeshRenderers = root.GetComponentsInChildren<Renderer>();
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            foreach (var v in skinnedMeshRenderer.sharedMaterials)
            {
                if (v.shader.name.ToLower().Contains("mtoon"))
                {
                    var albedo = v.GetTexture("_MainTex");
                    float cutoff = v.GetFloat("_Cutoff");
                    v.shader = urpLitShader;
                    v.SetTexture("_BaseMap", albedo);
                    v.SetFloat("_Cutoff", cutoff);
                    v.SetOverrideTag("RenderType", "TransparentCutout");

                    v.renderQueue = 2450;

                    // Set Emission  _EMISSION
                    Texture emissionMap = v.GetTexture("_EmissionMap");
                    if (emissionMap != null && !emissionMap.name.Equals("Shader_NoneBlack", StringComparison.CurrentCultureIgnoreCase))
                    {
                        v.EnableKeyword("_EMISSION");
                        //v.EnableKeyword("_ALPHATEST_ON");
                    }
                    else
                    {
                        v.EnableKeyword("_ALPHATEST_ON");
                    }
                }
            }
        }
    }
    async public void LoadModel(string path)
    {
        Transform girl = this.transform;
        if (isDefault)
        {
            girl = this.transform.GetChild(0);
        }
        else
        {
            if (path == "")
            {
                path = Application.streamingAssetsPath + origin;
            }
            if (path.Contains(".p"))
            {
                girl = await PMXModelLoader.LoadPMXModel(path, SampleAnimatorController);
                //ChangeMat(girl.gameObject);
                type = ModelType.Mmd;
            }
            else if (path.Contains(".v"))
            {
                var data = new GlbFileParser(path).Parse();
                var vrm = new VRMData(data);
                var context = new VRMImporterContext(vrm);
                var loaded = context.Load();
                data.Dispose();
                loaded.EnableUpdateWhenOffscreen();
                loaded.ShowMeshes();
                loaded.gameObject.name = loaded.name;
                girl = loaded.gameObject.transform;
                BVAMaterialExtension.ChangeMaterial(girl.gameObject);
                BVASpringBoneExtension.TranslateVRMPhysicToBVAPhysics(girl.gameObject);
                type = ModelType.Vrm;
            }
            else if (path.Contains(".g") || path.Contains(".b"))
            {
                BVASceneManager.Instance.onSceneLoaded += (type, scene) =>
                {
                    girl = scene.mainScene;
                };
                await BVASceneManager.Instance.LoadAvatar(path);
                type = ModelType.Bva;
            }
        }
        girl.gameObject.name = "Girl";
        
        if (girl.gameObject.GetComponent<Animator>())
        {
            girl.gameObject.GetComponent<Animator>().runtimeAnimatorController = SampleAnimatorController;
            girl.gameObject.GetComponent<Animator>().applyRootMotion = false;
            UnityVMDPlayer vmd = girl.gameObject.AddComponent<UnityVMDPlayer>();
            vmd.IsLoop = true;
            vmd.UseParentOfAll = true;
            vmdPlayer = girl.gameObject.AddComponent<VmdPlayer>();
            if (!StartScene) behaviour.model = this;
            girl.parent = Root.transform;
            girl.transform.localPosition = defaultPosition;
            girl.transform.localRotation = defaultRotation;
            if(!isDefault) girl.transform.localScale = new Vector3(1, 1, 1);
            if (StartScene) girl.transform.localRotation = Quaternion.Euler(0, 180, 0);
            if (!StartScene)
            {
                AudioLipSync lip = girl.gameObject.AddComponent<AudioLipSync>();
                lip.enabled = false;
                if (!isDefault)
                {
                    lip.enabled = true;
                    lip.lipSyncMethod = ELipSyncMethod.Runtime;
                    lip.audioSource = GameObject.Find("VITS_Speeker").GetComponent<AudioSource>();
                    lip.recognizerLanguage = ERecognizerLanguage.Japanese;
                    lip.propertyNames = mouthName_JP;
                    lip.propertyNames_EN = mouthName_EN;
                    lip.propertyMinValue = 0;
                    lip.propertyMaxValue = 100;
                    lip.amplitudeThreshold = 0.02f;
                    lip.moveTowardsSpeed = 8;
                }
                EmotionController emotion = girl.gameObject.AddComponent<EmotionController>();
                emotion.preindex = -1;
                emotion.index = -1;
                emotion.speed = 1;
                switch (type)
                {

                    case ModelType.Mmd:
                        if(!isDefault) lip.targetBlendShapeObject = girl.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
                        emotion.Initial();
                        emotion.CustomBlendShapes = CustomBlendShapes;
                        emotion.isCustom = isCustom;
                        emotion.CustomHeader = CustomHeader;
                        if (!isDefault) emotion.targetBlendShapeObject = girl.transform.GetChild(0).gameObject.GetComponent<SkinnedMeshRenderer>();
                        else emotion.targetBlendShapeObject = BlendShapeTarget;
                        emotion.GetBlendShape();
                        emotion.GetCustomBlendShape();
                        break;
                    case ModelType.Vrm:
                        int index = 0;
                        int Count = 0;
                        if (!isDefault) {
                            for (int i = 0; i < girl.childCount; i++)
                            {
                                if (girl.GetChild(i).GetComponent<SkinnedMeshRenderer>())
                                {
                                    SkinnedMeshRenderer mesh = girl.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                                    if (mesh.sharedMesh.blendShapeCount > Count)
                                    {
                                        index = i;
                                        Count = mesh.sharedMesh.blendShapeCount;
                                        emotion.targetBlendShapeObject = mesh;
                                    }
                                }
                            }
                        }
                        else emotion.targetBlendShapeObject = BlendShapeTarget;
                        VRMBlendShapeProxy vrmproxy = girl.GetComponentInChildren<VRMBlendShapeProxy>();
                        if (!isDefault)
                        {
                            lip.targetBlendShapeObject = girl.GetChild(index).GetComponent<SkinnedMeshRenderer>();
                            lip.isVrm = true;
                            lip.vrmproxy = vrmproxy;
                            lip.GetVrmBlendShape();
                        }
                        emotion.CustomBlendShapes = CustomBlendShapes;
                        emotion.isCustom = isCustom;
                        emotion.CustomHeader = CustomHeader;
                        emotion.isVrm = true;
                        emotion.Initial();
                        emotion.vrmproxy = vrmproxy;
                        emotion.GetVrmBlendShape();
                        emotion.GetCustomBlendShape();
                        break;
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(0.1f));
            Play(0);
        }
        else
        {
            setting.PmxSource.value = 0;
            tip.Open("模型異常，自動加載默認模型");
        }
    }
    public void DeleteModel()
    {
        if (GameObject.Find("Girl"))
        {
            Destroy(GameObject.Find("Girl"));
            behaviour.model = null;
        }
    }
    public void Play(int i)
    {
        if (vmdPlayer)
        {
            switch (ActionList[i].type)
            {
                case 0:
                    vmdPlayer.PlayAnimator(i);
                    break;
                case 1:
                    vmdPlayer.PlayVmd(Application.streamingAssetsPath + ActionList[i].name);
                    break;
            }
        }
    }  
    public void SetBool(string name,bool flag)
    {
        if(vmdPlayer) vmdPlayer.SetBool(name, flag);
    }
    public void SetInteger(string name,int i)
    {
        if (vmdPlayer) vmdPlayer.SetInteger(name, i);
    }
    // Update is called once per frame
    void Update()
    {
         
    }
}
[Serializable]
public class ActionM
{
    public string name;
    public int type;
    public float time;
}
