using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JDataToTexture_cluster : MonoBehaviour
{
	public Texture2D m_texture;

	private Material mat;
	private int width = 128;
	private int height = 128;

	private bool inited = false;
	int[,] m_spectrumColorIdx;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!inited && Time.time > 2)
		{
			inited = true;
			DebugCalCluster();
			DebugDrawCluster();
		}
	}

	void DebugCalCluster()
	{
		WAudioAnalysis.AudioSpectrumDataCache audioSpectrumCache = JAudioAnalysis_offlineSpectrum.Instance().m_audioSpectrumCache;
		int cacheSize = audioSpectrumCache.size;
		int spectrumSize = JAudioAnalysis.spectrumSize;

		m_spectrumColorIdx = new int[cacheSize, spectrumSize];
		for (int spectrum_idx = 0; spectrum_idx < spectrumSize; ++spectrum_idx)
		{
			float[] data_cache = new float[cacheSize];
			for (int idx = 0; idx < cacheSize; ++idx)
			{
				data_cache[idx] = audioSpectrumCache.GetData(idx, spectrum_idx);
			}
			float[] cluster;
			int[] classify;
			WAudioAnalysis.Cluster.Analysis(ref data_cache, 2, out cluster, out classify);
			for (int cache_idx = 0; cache_idx < cacheSize; ++cache_idx)
			{
				m_spectrumColorIdx[cache_idx, spectrum_idx] = classify[cache_idx];
			}
		}
	}

	void DebugDrawCluster()
	{
		WAudioAnalysis.AudioSpectrumDataCache audioSpectrumCache = JAudioAnalysis_offlineSpectrum.Instance().m_audioSpectrumCache;
		int m_spectrumSize = JAudioAnalysis.spectrumSize;

		int last_draw_x = -1;
		float all_pixel_width = Mathf.Log(m_spectrumSize);
		float each_pixel_width = all_pixel_width / width;
		for (int x = 0; x < m_spectrumSize; ++x)
		{
			int draw_x = (int)(Mathf.Log(x + 1) / all_pixel_width * width);
			if (draw_x == last_draw_x)
			{
				continue;
			}
			if (draw_x >= width)
			{
				break;
			}
			for (int y = 0; y < audioSpectrumCache.size && y < height; ++y)
			{
				float amp = audioSpectrumCache.spectrumCache[y, x];
				Color c = m_spectrumColorIdx[y, x] == 0 ? Color.red : (m_spectrumColorIdx[y, x] == 1 ? Color.green : Color.blue);
				for (int step_x = last_draw_x < 0 ? 0 : last_draw_x; step_x <= draw_x; ++step_x)
				{
					m_texture.SetPixel(step_x, y, c);
				}
			}
			last_draw_x = draw_x;
		}
		m_texture.Apply();
	}
}
