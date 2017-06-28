using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class JDataToTexture_dynamic : MonoBehaviour
{
	public Texture2D m_texture;

	private Material mat;
	private int width = 128;
	private int height = 128;

	private int cur_draw_height = 0;
	private int draw_step = 1;

	// Use this for initialization
	void Start ()
	{
		mat = GetComponent<MeshRenderer>().material;
		m_texture = new Texture2D(width, height, TextureFormat.ARGB32, true);
		mat.SetTexture("_MainTex", m_texture);
	}
	
	// Update is called once per frame
	void Update () {
		FormTexture();
		RefreshTextureOffset();
	}

	void FormTexture()
	{
		int m_spectrumSize = JAudioAnalysis.spectrumSize;
		float m_ampMin = JAudioAnalysis.Instance().ampMin;
		float m_ampMax = JAudioAnalysis.Instance().ampMax;


		Color[] color_buf = new Color[width * draw_step];

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

			float amp = JAudioAnalysis.Instance().spectrum[x];
			float ratio = (amp - m_ampMin) / Mathf.Max(1, m_ampMax - m_ampMin) * 5;
			for (int step_y = 0; step_y < draw_step; ++step_y)
			{
				for (int step_x = last_draw_x < 0 ? 0 : last_draw_x; step_x <= draw_x; ++step_x)
				{
					color_buf[step_x + (step_y * width)] = new Color(ratio, 0, 0, 1);
				}
			}

			last_draw_x = draw_x;
		}
		m_texture.SetPixels(0, cur_draw_height, width, draw_step, color_buf);
		m_texture.Apply();
		cur_draw_height = (cur_draw_height + draw_step) % height;
	}

	void RefreshTextureOffset()
	{
		mat.SetTextureOffset("_MainTex", new Vector2(0, (float)cur_draw_height / height));
	}
}
