using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JAudioAnalysis : MonoBehaviour {
	public const int m_ampSmoothCount = 2;
	public const int m_spectrumSmoothCount = 8;

	#region Singleton
	private static JAudioAnalysis m_instance;
	public static JAudioAnalysis Instance()
	{
		return m_instance;
	}
	#endregion

	#region CacheAndSmooth
	private struct CacheAndSmooth
	{
		public CacheAndSmooth(int count)
		{
			result = 0;
			smoothCount = count;
			smoothedAmpList = new float[smoothCount];
			smoothWindow = new float[smoothCount];
			smoothIdx = 0;

			float total = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				float x = (idx + 0.5f) / smoothCount;
				smoothWindow[idx] = x > 0.5f ? 4 * (1 - x) : 4 * x;
				total += smoothWindow[idx];
			}
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				smoothWindow[idx] /= total;
			}
		}
		public float AddData(float data)
		{
			smoothedAmpList[smoothIdx] = data;
			smoothIdx = (smoothIdx + 1) % smoothCount;
			result = 0;
			for (int idx = 0; idx < smoothCount; ++idx)
			{
				result += smoothedAmpList[(smoothIdx + 1 + idx) % smoothCount] * smoothWindow[idx];
			}
			return result;
		}
		public float result;
		private int smoothCount;
		private float[] smoothedAmpList;
		private float[] smoothWindow;
		private int smoothIdx;
	}
	#endregion
	#region Filter
	private struct Filter
	{
		// 仅保留某一段中的最大值点
		private static void FilterWindow_isMax(ref float[] spectrum, int idx, int window_width)
		{
			int idx_start = System.Math.Max(0, idx - window_width);
			int idx_end = System.Math.Min(spectrum.Length, idx + window_width);
			int last_max_idx = idx_start;
			for(int i = idx_start+1; i < idx_end; ++i)
			{
				if(spectrum[i] > spectrum[last_max_idx])
				{
					spectrum[last_max_idx] = 0;
					last_max_idx = i;
				}
				else
				{
					spectrum[i] = 0;
				}
			}
		}
		public static void FilterMax(ref float[] spectrum, int window_width)
		{
			for(int idx = 0; idx != spectrum.Length; ++idx)
			{
				FilterWindow_isMax(ref spectrum, idx, window_width);
			}
		}
	}
	#endregion

	// 曲线数据
	private const int m_spectrumSize = 512;
	private float[] m_spectrum = new float[m_spectrumSize];

	// Analysis 1: 将曲线在时间上平滑
	private CacheAndSmooth m_ampSmoother = new CacheAndSmooth(m_ampSmoothCount);

	// Analysis 1.1: 在频率上的最大值
	public float MaxAmp { get { return m_amp; } }
	private float m_amp = 0.0f;
	private float m_ampMin = 10000;
	private float m_ampMax = -10000;

	// Analysis 1.2: 分析曲线在时间上的突变
	public bool Spike { get { return m_spikeSpectrum; } }
	public bool[] m_spikeSpectrum = new bool[m_ampSmoothCount];

	// Analysis 2: 将曲线在频率上平滑
	public float[] SampleSpectrum { get { return m_sampleSpectrum; } }
	private const int m_sampleStep = 1;
	private float[] m_sampleSpectrum = new float[(int)Mathf.Ceil(m_spectrumSize/m_sampleStep)];
	private CacheAndSmooth[] m_sampleSpectrumSmoother = new CacheAndSmooth[(int)Mathf.Ceil(m_spectrumSize / m_sampleStep)];

	private void Awake()
	{
		GetComponent<AudioSource>().Play();
		m_instance = this;

		// Analysis 2: 初始化平滑用的结构
		for (int idx = 0; idx != m_sampleSpectrumSmoother.Length; ++idx)
		{
			m_sampleSpectrumSmoother[idx] = new CacheAndSmooth(m_spectrumSmoothCount);
		}
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

		// Analysis 2: 将曲线在频率上平滑
		for (int i = 1; i < m_spectrum.Length; i++)
		{
			int sample_idx = i / m_sampleStep;
			m_sampleSpectrum[sample_idx] = m_sampleSpectrumSmoother[sample_idx].AddData(m_spectrum[i]);
		}
	}
}