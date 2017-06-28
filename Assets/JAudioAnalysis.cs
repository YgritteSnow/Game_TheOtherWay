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
	public const int m_spectrumSize = 1024;
	public float[] m_spectrum = new float[m_spectrumSize];

	// Analysis 1: 将频率最大值在时间上平滑
	public float GetMaxAmp() { return m_amp; }
	private const int m_ampSmoothCount = 2;
	private WAudioAnalysis.CacheAndSmooth m_ampSmoother = new WAudioAnalysis.CacheAndSmooth(m_ampSmoothCount);
	private float m_amp = 0.0f;
	public float m_ampMin = 10000;
	public float m_ampMax = -10000;

	// Analysis 2: 将所有频率在时间上平滑，获取时间上的突变
	public bool[] SpikeSpectrum { get{ return m_spikeSpectrum; } }
	private int m_spectrumSmoothCount = 2;
	private float m_spectrumSpikeRatio = 0.95f;
	private bool[] m_spikeSpectrum = new bool[m_spectrumSize];
	public WAudioAnalysis.CacheAndSmoothWithCheck[] m_spectrumSmoother = new WAudioAnalysis.CacheAndSmoothWithCheck[m_spectrumSize];

	// Analysis 3: 将曲线在频率上分段后，每段放在一起取均值，然后在时间上平滑
	public float[] SampleSpectrum { get { return m_sampleSpectrum; } }
	private const int m_sampleStep = 1;
	private const int m_sampleSpectrumSmoothCount = 8;
	private float[] m_sampleSpectrum = new float[(int)Mathf.Ceil(m_spectrumSize/m_sampleStep)];
	private WAudioAnalysis.CacheAndSmooth[] m_sampleSpectrumSmoother = new WAudioAnalysis.CacheAndSmooth[(int)Mathf.Ceil(m_spectrumSize / m_sampleStep)];

	// Analysis 4: 使用聚类来对音频进行离线处理
	public WAudioAnalysis.AudioRawDataCache m_audioRawCache;
	public WAudioAnalysis.AudioSpectrumDataCache m_audioSpectrumCache;
	private float m_startCacheSpectrumTime;

	private void Awake()
	{
		GetComponent<AudioSource>().Play();
		m_instance = this;

		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int idx = 0; idx != m_spectrumSmoother.Length; ++idx)
		{
			m_spectrumSmoother[idx] = new WAudioAnalysis.CacheAndSmoothWithCheck(m_spectrumSmoothCount, m_spectrumSpikeRatio);
		}
		// Analysis 3: 初始化下采样用的结构
		for (int idx = 0; idx != m_sampleSpectrumSmoother.Length; ++idx)
		{
			m_sampleSpectrumSmoother[idx] = new WAudioAnalysis.CacheAndSmooth(m_sampleSpectrumSmoothCount);
		}
		// Analysis 4: 对音频进行离线处理
		m_audioRawCache = new WAudioAnalysis.AudioRawDataCache(GetComponent<AudioSource>().clip);
		m_audioSpectrumCache = new WAudioAnalysis.AudioSpectrumDataCache(GetComponent<AudioSource>().clip.length, 0.01f, m_spectrumSize);
		m_startCacheSpectrumTime = Time.fixedTime;
	}

	void Update()
	{
		// 获取曲线
		AudioListener.GetSpectrumData(m_spectrum, 0, FFTWindow.Rectangular);

		// Analysis 1: 计算曲线在不同频率上的最大值
		float count_all = 0;
		for (int i = 1; i < m_spectrum.Length; i++)
		{
			count_all += m_spectrum[i];
			m_ampMin = Mathf.Min(m_ampMin, m_spectrum[i]);
			m_ampMax = Mathf.Max(m_ampMax, m_spectrum[i]);
		}
		count_all /= m_spectrumSize;
		m_amp = m_ampSmoother.AddData(1 - Mathf.Clamp((count_all - m_ampMin) / (m_ampMax - m_ampMin), 0, 1));

		// Analysis 2: 将所有频率在时间上平滑，分析曲线在时间上的突变
		for (int i = 1; i < m_spectrum.Length; i++)
		{
			m_spikeSpectrum[i] = m_spectrumSmoother[i].AddData(m_spectrum[i]);
		}

		// Analysis 3: 将曲线在频率上下采样
		for (int i = 1; i < m_spectrum.Length; i++)
		{
			int sample_idx = i / m_sampleStep;
			m_sampleSpectrum[sample_idx] = m_sampleSpectrumSmoother[sample_idx].AddData(m_spectrum[i]);
		}

		//DebugDrawSpectrumColor();
		//DebugDrawAudioRawData();
		CacheSpectrumData();
		//DebugDrawAudioSpectrumDataWithLine();
	}

	void CacheSpectrumData()
	{
		m_audioSpectrumCache.AddData(m_spectrum, Time.fixedTime - m_startCacheSpectrumTime);
	}

	private const int m_cacheSize = 200;
	private float[,] m_cachedSpec = new float[m_cacheSize, m_spectrumSize];
	private int m_curCacheIdx = 0;
	void DebugDrawSpectrumColor()
	{
		for (int idx = 0; idx < m_spectrumSize; ++idx)
		{
			m_cachedSpec[m_curCacheIdx, idx] = m_spectrum[idx];
			m_curCacheIdx = (m_curCacheIdx + 1) % m_cacheSize;
		}

		float last_draw_x = -1;
		for(int x = 0; x < m_spectrumSize; ++x)
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

	void DebugDrawAudioRawData()
	{
		for(int x = 1; x < m_audioRawCache.rawData.Length; ++x)
		{
			float zoom = 0.00001f;
			Debug.DrawLine(new Vector3((x - 1) * zoom, m_audioRawCache.rawData[x-1], 1), new Vector3(x * zoom, m_audioRawCache.rawData[x], 1), Color.red);
		}
	}

	void DebugDrawAudioSpectrumDataWithLine()
	{
		Debug.Log("time=" + Time.time
			+ ", fixedTime=" + Time.fixedTime
			+ ", fixedUnscaledTime=" + Time.fixedUnscaledTime
			+ ", realtimeSinceStartup=" + Time.realtimeSinceStartup
			+ ", unscaledDeltaTime=" + Time.unscaledDeltaTime
			+ ", timeSinceLevelLoad=" + Time.timeSinceLevelLoad
			+ ", size=" + m_audioSpectrumCache.size
			);
		if(Time.time < 11.0f)
		{
			return;
		}
		float last_draw_x = -1;
		for (int x = 0; x < m_spectrumSize; ++x)
		{
			if (Mathf.Log(x + 1) - last_draw_x < 1.0f / Screen.width * 2)
			{
				continue;
			}

			for (int y = 0; y < m_audioSpectrumCache.size; ++y)
			{
				float amp = m_audioSpectrumCache.spectrumCache[y, x];
				float ratio = (amp - m_ampMin) / Mathf.Max(1, m_ampMax - m_ampMin) * 5;
				Color c = new Color(ratio, 0, 0, 1);
				Debug.DrawLine(new Vector3(Mathf.Log(x + 1), y * 10.0f / m_audioSpectrumCache.size - 5, -1),
					new Vector3(Mathf.Log(x + 1), (y + 1) * 10.0f / m_audioSpectrumCache.size - 5, -1),
					c);
				last_draw_x = Mathf.Log(x + 1);
			}
		}
	}
}