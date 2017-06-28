using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JDebugBoard : MonoBehaviour {
	public Texture2D m_texture;
	private int width = 128;
	private int height = 128;

	private int cur_draw_height = 0;
	private Material mat;

	// Use this for initialization
	void Start ()
	{
		mat = GetComponent<MeshRenderer>().material;
		m_texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
		SetTexture();
	}

	private void Update()
	{
		FormatTexture();
		SetTextureOffset();
	}

	void FormatTexture()
	{
		int m_spectrumSize = JAudioAnalysis.m_spectrumSize;
		float m_ampMin = JAudioAnalysis.Instance().m_ampMin;
		float m_ampMax = JAudioAnalysis.Instance().m_ampMax;


		Color[] color_buf = new Color[width];

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
			if(draw_x >= width)
			{
				break;
			}

			float amp = JAudioAnalysis.Instance().m_spectrum[x];
			float ratio = (amp - m_ampMin) / Mathf.Max(1, m_ampMax - m_ampMin) * 5;
			for (int step_x = last_draw_x < 0 ? 0 : last_draw_x; step_x <= draw_x; ++step_x)
			{
				color_buf[step_x] = new Color(ratio, 0, 0, ratio);
			}

			last_draw_x = draw_x;
		}
		m_texture.SetPixels(0, cur_draw_height, width, 1, color_buf);
		m_texture.Apply();
		cur_draw_height = (cur_draw_height + 1) % height;
	}
	void SetTextureOffset()
	{
		mat.SetTextureOffset("_MainTex", new Vector2(0, (float)cur_draw_height / height));
	}

	void FormatWholeTexture()
	{
		WAudioAnalysis.AudioSpectrumDataCache m_audioSpectrumCache = JAudioAnalysis.Instance().m_audioSpectrumCache;
		int m_spectrumSize = JAudioAnalysis.m_spectrumSize;
		float m_ampMin = JAudioAnalysis.Instance().m_ampMin;
		float m_ampMax = JAudioAnalysis.Instance().m_ampMax;

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

	void SetTexture() {
		mat.SetTexture("_MainTex", m_texture);
	}
}
