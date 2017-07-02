using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 离线处理音频的频谱
 */

public class JAudioAnalysis_offlineSpectrum : MonoBehaviour
{
	#region Singleton
	private static JAudioAnalysis_offlineSpectrum m_instance;
	public static JAudioAnalysis_offlineSpectrum Instance()
	{
		return m_instance;
	}
	#endregion

	public WAudioAnalysis.AudioSpectrumDataCache m_audioSpectrumCache;
	private float m_startCacheSpectrumTime;

	// Use this for initialization
	void Awake ()
	{
		m_instance = this;

		m_audioSpectrumCache = new WAudioAnalysis.AudioSpectrumDataCache(GetComponent<AudioSource>().clip.length, 0.01f, JAudioAnalysis.spectrumSize);
		m_startCacheSpectrumTime = Time.fixedTime;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float[] s = JAudioAnalysis.Instance().spectrum;
		m_audioSpectrumCache.AddData(s, Time.fixedTime - m_startCacheSpectrumTime);

		if (Time.time < 11.0f)
		{
			return;
		}

		DrawByLine();
	}

	void DrawByLine()
	{
		float last_draw_x = -1;
		for (int x = 0; x < JAudioAnalysis.spectrumSize; ++x)
		{
			if (Mathf.Log(x + 1) - last_draw_x < 1.0f / Screen.width * 2)
			{
				continue;
			}

			for (int y = 0; y < m_audioSpectrumCache.size; ++y)
			{
				float amp = m_audioSpectrumCache.spectrumCache[y, x];
				float ratio = (amp - JAudioAnalysis.Instance().ampMin) / Mathf.Max(1, JAudioAnalysis.Instance().ampMax - JAudioAnalysis.Instance().ampMin) * 5;
				Color c = new Color(ratio, 0, 0, 1);
				Debug.DrawLine(new Vector3(Mathf.Log(x + 1), y * 10.0f / m_audioSpectrumCache.size - 5, -1),
					new Vector3(Mathf.Log(x + 1), (y + 1) * 10.0f / m_audioSpectrumCache.size - 5, -1),
					c);
				last_draw_x = Mathf.Log(x + 1);
			}
		}
	}
}
