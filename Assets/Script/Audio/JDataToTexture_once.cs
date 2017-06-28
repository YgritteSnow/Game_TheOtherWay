using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class JDataToTexture_once : MonoBehaviour
{
	public Texture2D m_texture;

	private Material mat;
	private int width = 128;
	private int height = 128;

	// Use this for initialization
	void Start ()
	{
		mat = GetComponent<MeshRenderer>().material;
		m_texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
		mat.SetTexture("_MainTex", m_texture);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void FormatWholeTexture()
	{
		WAudioAnalysis.AudioSpectrumDataCache m_audioSpectrumCache = JAudioAnalysis_offlineSpectrum.Instance().m_audioSpectrumCache;
		int m_spectrumSize = JAudioAnalysis.spectrumSize;
		float m_ampMin = JAudioAnalysis.Instance().ampMin;
		float m_ampMax = JAudioAnalysis.Instance().ampMax;

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
			for (int y = 0; y < m_audioSpectrumCache.size && y < height; ++y)
			{
				float amp = m_audioSpectrumCache.spectrumCache[y, x];
				float ratio = (amp - m_ampMin) / Mathf.Max(1, m_ampMax - m_ampMin) * 5;
				Color c = new Color(ratio, 0, 0, 1);
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
