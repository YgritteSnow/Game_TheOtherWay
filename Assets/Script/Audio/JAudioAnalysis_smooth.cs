using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Analysis 3: 将曲线在频率上分段后，每段放在一起取均值，然后在时间上平滑
 */

public class JAudioAnalysis_smooth : MonoBehaviour
{
	#region Singleton
	private static JAudioAnalysis_smooth m_instance;
	public static JAudioAnalysis_smooth Instance()
	{
		return m_instance;
	}
	#endregion

	public float[] SampleSpectrum { get { return m_sampleSpectrum; } }
	private const int m_sampleStep = 1;
	private const int m_sampleSpectrumSmoothCount = 8;
	private float[] m_sampleSpectrum = new float[(int)Mathf.Ceil(JAudioAnalysis.spectrumSize / m_sampleStep)];
	private WAudioAnalysis.CacheAndSmooth[] m_sampleSpectrumSmoother = new WAudioAnalysis.CacheAndSmooth[(int)Mathf.Ceil(JAudioAnalysis.spectrumSize / m_sampleStep)];

	void Awake ()
	{
		m_instance = this;

		for (int idx = 0; idx != m_sampleSpectrumSmoother.Length; ++idx)
		{
			m_sampleSpectrumSmoother[idx] = new WAudioAnalysis.CacheAndSmooth(m_sampleSpectrumSmoothCount);
		}
	}
	
	void Update ()
	{
		float[] s = JAudioAnalysis.Instance().spectrum;
		for (int i = 0; i < s.Length; i++)
		{
			int sample_idx = i / m_sampleStep;
			m_sampleSpectrum[sample_idx] = m_sampleSpectrumSmoother[sample_idx].AddData(s[i]);
		}
	}
}
