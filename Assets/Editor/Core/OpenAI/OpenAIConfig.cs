using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class OpenAIConfig : ScriptableObject
{
    public string APIKey;
    [Header("Only check this if you have v4 API Access")]
    public bool UseGPT4 = false;
}
