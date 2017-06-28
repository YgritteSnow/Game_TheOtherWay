using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JAudioAnalysis : MonoBehaviour {

	#region Singleton
	private static JAudioAnalysis m_instance;
	public static JAudioAnalysis Instance()
	{
		return m_instance;
	}
	#endregion


	// 曲线数据
	[HideInInspector]public const int spectrumSize = 128;
	public float[] spectrum { get { return m_spectrum; } }
	public float ampMin { get { return m_ampMin; } }
	public float ampMax { get { return m_ampMax; } }
	private float[] m_spectrum = new float[spectrumSize];
	private float m_ampMin = 10000;
	private float m_ampMax = -10000;

	// Analysis 2: 将所有频率在时间上平滑，获取时间上的突变
	public bool[] SpikeSpectrum { get{ return m_spikeSpectrum; } }
	private int m_spectrumSmoothCount = 2;
	private float m_spectrumSpikeRatio = 0.95f;
	private bool[] m_spikeSpectrum = new bool[spectrumSize];
	public WAudioAnalysis.CacheAndSmoothWithCheck[] m_spectrumSmoother = new WAudioAnalysis.CacheAndSmoothWithCheck[spectrumSize];

	private void Awake()
	{
		GetComponent<AudioSource>().Play();
		m_instance = this;

		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int idx = 0; idx != m_spectrumSmoother.Length; ++idx)
		{
			m_spectrumSmoother[idx] = new WAudioAnalysis.CacheAndSmoothWithCheck(m_spectrumSmoothCount, m_spectrumSpikeRatio);
		}
	}

	void Update()
	{
		// 获取曲线
		AudioListener.GetSpectrumData(m_spectrum, 0, FFTWindow.Rectangular);
		float[] s = JAudioAnalysis.Instance().spectrum;
		for (int i = 0; i < s.Length; i++)
		{
			m_ampMin = Mathf.Min(m_ampMin, s[i]);
			m_ampMax = Mathf.Max(m_ampMax, s[i]);
		}

		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int i = 0; i < m_spectrum.Length; i++)
		{
			m_spikeSpectrum[i] = m_spectrumSmoother[i].AddData(m_spectrum[i]);
		}

		DebugDrawSpectrumColor();
	}

	private const int m_cacheSize = 200;
	private float[,] m_cachedSpec = new float[m_cacheSize, spectrumSize];
	private int m_curCacheIdx = 0;
	void DebugDrawSpectrumColor()
	{
		for (int idx = 0; idx < spectrumSize; ++idx)
		{
			m_cachedSpec[m_curCacheIdx, idx] = m_spectrum[idx];
			m_curCacheIdx = (m_curCacheIdx + 1) % m_cacheSize;
		}

		float last_draw_x = -1;
		for(int x = 0; x < spectrumSize; ++x)
		{
			if(Mathf.Log(x + 1) - last_draw_x < 1.0f / Screen.width * 2)
			{
				continue;
			}
			
			for (int y = 0; y <= m_cacheSize; ++y)
			{
				int real_y = (m_curCacheIdx + y) % m_cacheSize;
				float amp = m_cachedSpec[real_y, x];
				float ratio = (amp - m_ampMin) / Mathf.Max(1, m_ampMax - m_ampMin) * 5;
				Color c = new Color(ratio, 0, 0, 1);
				Debug.DrawLine(new Vector3(Mathf.Log(x + 1), y * 10.0f / m_cacheSize - 5, -1),
					new Vector3(Mathf.Log(x + 1), (y+1) * 10.0f / m_cacheSize - 5, -1), 
					c);
				last_draw_x = Mathf.Log(x + 1);
			}
		}
	}
}