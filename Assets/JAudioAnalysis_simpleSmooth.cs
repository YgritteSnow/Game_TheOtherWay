using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JAudioAnalysis_simpleSmooth : MonoBehaviour
{
	#region Singleton
	private static JAudioAnalysis_simpleSmooth m_instance;
	public static JAudioAnalysis_simpleSmooth Instance()
	{
		return m_instance;
	}
	#endregion

	// Analysis 2: 将所有频率在时间上平滑，获取时间上的突变
	public bool[] SpikeSpectrum { get { return m_spikeSpectrum; } }
	private int m_spectrumSmoothCount = 2;
	private float m_spectrumSpikeRatio = 0.95f;
	private bool[] m_spikeSpectrum = new bool[JAudioAnalysis.spectrumSize];
	public WAudioAnalysis.CacheAndSmoothWithCheck[] m_spectrumSmoother = new WAudioAnalysis.CacheAndSmoothWithCheck[JAudioAnalysis.spectrumSize];

	private void Awake()
	{
		m_instance = this;

		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int idx = 0; idx != m_spectrumSmoother.Length; ++idx)
		{
			m_spectrumSmoother[idx] = new WAudioAnalysis.CacheAndSmoothWithCheck(m_spectrumSmoothCount, m_spectrumSpikeRatio);
		}
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		float[] m_spectrum = JAudioAnalysis.Instance().spectrum;
		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int i = 0; i < m_spectrum.Length; i++)
		{
			m_spikeSpectrum[i] = m_spectrumSmoother[i].AddData(m_spectrum[i]);
		}
	}
}
