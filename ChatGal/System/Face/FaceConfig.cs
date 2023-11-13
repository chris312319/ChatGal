using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioSamplingQuality
{
    /// <summary>
    /// High quality.
    /// </summary>
    High,

    /// <summary>
    /// Very high quality.
    /// </summary>
    VeryHigh,

    /// <summary>
    /// Insane quality.
    /// </summary>
    Maximum
}
public class FaceConfig : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public AudioSamplingQuality SamplingQuality;
    /// <summary>
    /// Audio gain.
    /// </summary>
    [Range(1.0f, 10.0f)]
    public float Gain = 1.0f;
    /// <summary>
    /// Smoothing.
    /// </summary>
    [Range(0.0f, 1.0f)]
    public float Smoothing;
    /// <summary>
    /// Current samples.
    /// </summary>
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
