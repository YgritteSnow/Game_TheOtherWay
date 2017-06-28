using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * Analysis 1: 将频率最大值在时间上平滑
 */

public class JAudioAnalysis_maxAmp : MonoBehaviour
{
	#region Singleton
	private static JAudioAnalysis_maxAmp m_instance;
	public static JAudioAnalysis_maxAmp Instance()
	{
		return m_instance;
	}
	#endregion

	public float GetMaxAmp() { return m_amp; }
	private const int m_ampSmoothCount = 2;
	private WAudioAnalysis.CacheAndSmooth m_ampSmoother = new WAudioAnalysis.CacheAndSmooth(m_ampSmoothCount);
	private float m_amp = 0.0f;

	private void Awake()
	{
		m_instance = this;
	}

	// Update is called once per frame
	void Update ()
	{
		// Analysis 1: 计算曲线在不同频率上的最大值
		float count_all = 0;
		float[] s = JAudioAnalysis.Instance().spectrum;
		for (int i = 0; i < s.Length; i++)
		{
			count_all += s[i];
		}
		count_all /= JAudioAnalysis.spectrumSize;
		m_amp = m_ampSmoother.AddData(1 - Mathf.Clamp((count_all - JAudioAnalysis.Instance().ampMin) / (JAudioAnalysis.Instance().ampMax - JAudioAnalysis.Instance().ampMin), 0, 1));
	}
}
