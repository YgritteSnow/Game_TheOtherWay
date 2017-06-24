using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JAudioAnalysis : MonoBehaviour {
	private static JAudioAnalysis m_instance;
	public static JAudioAnalysis Instance()
	{
		return m_instance;
	}

	// 采样间隔：使用这段间隔内的 最大值 作为本段间隔的值。
	private float m_sampleFreq = 0.2f;
	// 采样数量
	private int m_sampleCount = 0;

	private AudioSource m_source;
	private float m_curAmp = 0.0f;
	private float m_ampMin = 100;
	private float m_ampMax = -100;

	private float m_smoothedAmp = 0.0f;
	private const int m_smoothCount = 6;
	private int m_smoothIdx = 0;
	private float[] m_smoothedAmpList = new float[m_smoothCount];
	private float[] m_smoothWindow = new float[m_smoothCount];
	public float GetCurAmp() { return m_smoothedAmp; }

	private void Awake()
	{
		GetComponent<AudioSource>().Play();

		m_instance = this;
		float total = 0;
		for(int idx = 0; idx < m_smoothCount; ++idx)
		{
			float x = (idx + 0.5f) / m_smoothCount;
			m_smoothWindow[idx] = x > 0.5f ? 4 * (1-x) : 4 * x;
			total += m_smoothWindow[idx];
		}
		for (int idx = 0; idx < m_smoothCount; ++idx)
		{
			m_smoothWindow[idx] /= total;
		}
	}
	
	void Update()
	{
		const int list_size = 128;
		float[] spectrum = new float[list_size];
		AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Triangle);
		float count_all = 0;
		for (int i = 1; i < spectrum.Length; i++)
		{
			count_all += spectrum[i];
			m_ampMin = Mathf.Min(m_ampMin, spectrum[i]);
			m_ampMax = Mathf.Max(m_ampMax, spectrum[i]);
		}
		count_all /= list_size;
		m_curAmp = 1 - Mathf.Clamp((count_all - m_ampMin) / (m_ampMax - m_ampMin), 0, 1);

		m_smoothedAmpList[m_smoothIdx] = m_curAmp;
		m_smoothIdx = (m_smoothIdx + 1) % m_smoothCount;
		m_smoothedAmp = 0;
		for (int idx = 0; idx < m_smoothCount; ++idx)
		{
			m_smoothedAmp += m_smoothedAmpList[idx] * m_smoothWindow[idx];
		}
	}
}
