using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{
    [SerializeField]
    private SpeechAPIConfigData speechAPIConfigData;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private MicrophoneManager microphoneManager;

    private const int synthesisSampleRate = 24000;

    public enum Voice
    {
        Sara,
        Brandon
    }

    public static Dictionary<Voice, string> VoiceNames = new Dictionary<Voice, string>()
    {
        { Voice.Sara,    "en-US-SaraNeural"     },
        { Voice.Brandon, "en-US-BrandonNeural"  }
    };

    public IEnumerator Say(string text, Voice voice = Voice.Brandon)
    {
        var speechConfig = speechAPIConfigData.GetSpeechConfig();
        speechConfig.SpeechSynthesisLanguage = speechAPIConfigData.FromLanguage;
        speechConfig.SpeechSynthesisVoiceName = VoiceNames[voice];
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

        AudioClip clip;
        {
            Debug.Log($"Synthesizing: {text}");

            var subtask = SpeechSynthesisAsync(speechConfig, text);
            yield return new WaitUntil(() => subtask.IsCompleted);

            var subsamples = subtask.Result.Item1;

            clip = AudioClip.Create($"playback", subsamples.Length, 1, synthesisSampleRate, false);
            clip.SetData(subsamples, 0);
        }

        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);
    }

    public IEnumerator Recognize(Action<string> onRecognize, Action<string> onError, byte[] audioData)
    {
        var speechConfig = speechAPIConfigData.GetSpeechConfig();
        speechConfig.SpeechRecognitionLanguage = speechAPIConfigData.FromLanguage;

        Debug.Log("Recognizing...");

        if (audioData != null)
        {
            var task = RecognizeSpeechAsync(speechConfig, audioData);
            yield return new WaitUntil(() => task.IsCompleted);

            var result = task.Result;

            if (task.Result == null)
            {
                Debug.Log("Aborting");
                yield break;
            }

            if (result.Reason == ResultReason.Canceled)
            {
                var cancellationDetail = CancellationDetails.FromResult(result);
                onError(cancellationDetail.ToString());
            }
            else if (result.Reason == ResultReason.RecognizedSpeech)
            {
                onRecognize(result.Text);
            }
            else
            {
                onError(result.Reason.ToString());
            }
        }
        else
        {
            Debug.Log("AudioData was null");
        }
    }

    private static async Task<SpeechRecognitionResult> RecognizeSpeechAsync(SpeechConfig speechConfig, byte[] audioData)
    {
        var stopTask = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        using (var audioInputStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1))) // This need be set based on the format of the given audio data
        using (var audioConfig = AudioConfig.FromStreamInput(audioInputStream))
        using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig))
        {
            audioInputStream.Write(audioData);
            audioInputStream.Write(new byte[0]); // send a zero-size chunk to signal the end of stream

            var result = await speechRecognizer.RecognizeOnceAsync();

            stopTask.TrySetResult(0);

            return result;
        }
    }

    private static async Task<(float[], List<SpeechSynthesisWordBoundaryEventArgs>)> SpeechSynthesisAsync(SpeechConfig speechConfig, string referenceText)
    {
        var stopTask = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        using (var synthesizer = new SpeechSynthesizer(speechConfig, null))
        {
            var boundaries = new List<SpeechSynthesisWordBoundaryEventArgs>();

            synthesizer.WordBoundary += (s, e) =>
            {
                // The unit of e.AudioOffset is tick (1 tick = 100 nanoseconds), divide by 10,000 to convert to milliseconds.
                //Debug.Log($"{e.Text}, {e.BoundaryType}");
                //Debug.Log($"{e.AudioOffset},{e.Duration}");

                if (e.BoundaryType == SpeechSynthesisBoundaryType.Word)
                {
                    boundaries.Add(e);
                }
            };

            using (var result = await synthesizer.StartSpeakingTextAsync(referenceText))
            using (var audioDataStream = AudioDataStream.FromResult(result))
            {
                Debug.Log(result.Reason.ToString());

                var maxChunkSize = synthesisSampleRate * 600;
                var audioChunkBytes = new byte[maxChunkSize * 2];
                var readBytes = audioDataStream.ReadData(audioChunkBytes);

                var audioChunk = new float[readBytes / 2];

                for (int i = 0; i < audioChunk.Length; ++i)
                {
                    if (i < readBytes / 2)
                    {
                        audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                    }
                    else
                    {
                        audioChunk[i] = 0.0f;
                    }
                }

                stopTask.TrySetResult(0);

                return (audioChunk, boundaries);
            }
        }
    }
}
