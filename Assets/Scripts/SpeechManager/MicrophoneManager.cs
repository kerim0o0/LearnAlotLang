using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    private AudioClip microphoneClip;

    private AudioClip playbackClip;

    private bool isRecording = false;

    public void StartRecording()
    {
        if(isRecording)
        {
            Debug.LogError("Already recording!");
            return;
        }

        Debug.Log("Start Recording");

        microphoneClip = Microphone.Start("", false, 512, 16000);
        isRecording = true;
    }

    public void StopRecording()
    {
        Debug.Log("Stop Recording");

        isRecording = false;

        int pos = Microphone.GetPosition("");
        int numSamples = pos * microphoneClip.channels;
        float[] samples = new float[numSamples];
        microphoneClip.GetData(samples, 0);
        byte[] audioData = ConvertAudioClipDataToInt16ByteArray(samples);

        playbackClip = AudioClip.Create("Playback", numSamples, microphoneClip.channels, microphoneClip.frequency, false);
        playbackClip.SetData(samples, 0);

        if (audioData.Length != 0)
        {
            Debug.Log($"Mic pos: {pos}");

            // TODO: Callback
        }

        Microphone.End("");
    }

    public AudioClip GetPlaybackClip()
    {
        return playbackClip;
    }

    public byte[] GetAudioData()
    {
        if(playbackClip == null || playbackClip.length == 0)
        {
            return null;
        }

        int numSamples = playbackClip.samples;
        float[] samples = new float[numSamples];
        playbackClip.GetData(samples, 0);

        return ConvertAudioClipDataToInt16ByteArray(samples);
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public bool IsRecordingValid()
    {
        return (playbackClip != null && playbackClip.length > 0);
    }

    private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
    {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length)
        {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }
        byte[] bytes = dataStream.ToArray();
        dataStream.Dispose();
        return bytes;
    }
}
