using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceController : MonoBehaviour
{
    // Start is called before the first frame update
    public FaceConfig faceconfig;
    public ModelLoader model;
    public SkinnedMeshRenderer skin;
    public AudioSource voice;
    public int[] mouthIndex;

    public AudioSamplingQuality SamplingQuality;
    private float[] Samples;
    [Range(1.0f, 10.0f)]
    public float Gain = 1.0f;
    public float LastRms { get; set; }
    private float VelocityBuffer;
    [Range(0.0f, 1.0f)]
    public float Smoothing;
    void Start()
    {

        mouthIndex = new int[model.mouthName_JP.Length];
        skin = GameObject.Find("Girl").GetComponentInChildren<SkinnedMeshRenderer>();
        voice = GameObject.Find("VITS_Speeker").GetComponent<AudioSource>();
        Mesh mesh = skin.sharedMesh;
        for(int i = 0; i < model.mouthName_JP.Length; i++)
        {
            mouthIndex[i] = mesh.GetBlendShapeIndex(model.mouthName_JP[i]);
        }
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        Speak();
    }
    public void Init()
    {
        SamplingQuality = faceconfig.SamplingQuality;
        Gain = faceconfig.Gain;
        Smoothing = faceconfig.Smoothing;
        switch (SamplingQuality)
        {
            case (AudioSamplingQuality.VeryHigh):
                {
                    Samples = new float[256];
                    break;
                }
            case (AudioSamplingQuality.Maximum):
                {
                    Samples = new float[512];
                    break;
                }
            default:
                {
                    Samples = new float[256];
                    break;
                }
        }
    }
    public void Speak()
    {
        var total = 0f;
        voice.GetOutputData(Samples, 0);
        for (var i = 0; i < Samples.Length; ++i)
        {
            var sample = Samples[i];
            total += (sample * sample);
        }
        var rms = Mathf.Sqrt(total / Samples.Length) * Gain;
        // Clamp root mean square.
        rms = Mathf.Clamp(rms, 0.0f, 1.0f);
        // Smooth rms.
        rms = Mathf.SmoothDamp(LastRms, rms, ref VelocityBuffer, Smoothing * 0.1f);
        skin.SetBlendShapeWeight(mouthIndex[0], rms * 100);
        LastRms = rms;
    }
}
