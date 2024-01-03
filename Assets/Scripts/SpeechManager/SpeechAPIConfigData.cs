using Microsoft.CognitiveServices.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SpeechAPIConfigData : ScriptableObject
{
    [Header("Cognitive Services")]
    public string SpeechServiceAPIKey = string.Empty;
    public string SpeechServiceRegion = "westus";
    public string FromLanguage = "en-us";

    [Header("OpenAI")]
    public string OpenAIAPIKey = string.Empty;
    public string OpenAIAPIOrg = string.Empty;

    public SpeechConfig GetSpeechConfig() 
    { 
        return SpeechConfig.FromSubscription(SpeechServiceAPIKey, SpeechServiceRegion);
    }
}
