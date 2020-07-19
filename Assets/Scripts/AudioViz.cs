using UnityEngine;
using System;
using System.Collections;
using Logic;
using DG.Tweening;

public class AudioViz
{
    public Texture2D AudioTexture { get; } = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);

    public const int TexWidth = 512;
    public const int TexHeight = 2;

    private readonly float[] _spectrumDataTemp = new float[TexWidth];
    private readonly float[] _spectrumData = new float[TexWidth];
    private readonly float[] _maxSpectrumData = new float[TexWidth];
    private readonly float[] _outputData = new float[TexWidth];

    public AudioViz()
    {
        AudioTexture.filterMode = FilterMode.Point;
    }

    public void SetSpectrumData(AudioSource audioSource,float lerpSpeed)
    {
        if (audioSource == null)
            return;

        // Obtain the samples from the frequency bands of the attached AudioSource  
        audioSource.GetSpectrumData(_spectrumDataTemp, 0, FFTWindow.BlackmanHarris);
        audioSource.GetOutputData(_outputData, 0);

        var data = new Color32[TexWidth * TexHeight];
        for (int i = 0; i < TexWidth; i++)
        {
            if (lerpSpeed>0.0f && _spectrumDataTemp[i]<_spectrumData[i])
            {
                _spectrumData[i] = Mathf.Lerp(_spectrumData[i],_spectrumDataTemp[i], lerpSpeed);
            }
            else
            {
                _spectrumData[i] = _spectrumDataTemp[i];
            }
            _maxSpectrumData[i] = Math.Max(_maxSpectrumData[i], _spectrumData[i]);

            // first row:
            var s = _spectrumData[i]/_maxSpectrumData[i];
            data[i] = new Color(s, s, s, s);

            // second row:
            var o = _outputData[i] * 255.0f;
            data[i + TexWidth] = new Color(o, o, o, o);
        }

        AudioTexture.SetPixels32(data);
        AudioTexture.Apply();
    }
}