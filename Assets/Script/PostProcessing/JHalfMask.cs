using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JHalfMask : MonoBehaviour {
	public Material m_maskMat;
	public Texture2D m_maskTex;
	public Material m_blurMat;
	public float m_blur_step_x;
	public float m_blur_step_y;

	// Use this for initialization
	void Start () {
		if (!m_maskTex || !m_blurMat)
		{
			Debug.LogError("MaskTexture not found!!!");
		}

		m_maskMat.SetTexture("_Mask", m_maskTex);
		m_blur_step_x = 1.0f / Screen.width;
		m_blur_step_y = 1.0f / Screen.height;
		m_blurMat.SetFloat("_StepX", m_blur_step_x);
		m_blurMat.SetFloat("_StepY", m_blur_step_y);

		// -- For Debug Only
		Dictionary<float, float> mask_map = new Dictionary<float, float>();
		mask_map.Add(0.5f, 0.0f);
		mask_map.Add(1.0f, 1.0f);
		SetMaskTextureByArray(mask_map);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetMaskTextureByArray(Dictionary<float, float> mask_map)
	{
		SortedDictionary<float, float> tmp = new SortedDictionary<float, float>();
		foreach(KeyValuePair<float, float> v in mask_map)
		{
			tmp.Add(v.Key, v.Value);
		}
		int last_pixel = 0;
		float width = m_maskTex.width;
		Color iter_color = Color.white;
		Color[] colors = new Color[m_maskTex.width];
		foreach(KeyValuePair<float, float> v in tmp)
		{
			for(; last_pixel <= (v.Key * width); ++last_pixel)
			{
				if(last_pixel >= m_maskTex.width)
				{
					break;
				}
				colors[last_pixel] = new Color(v.Value, 0, 0, 0);
			}
		}
		m_maskTex.SetPixels(colors);
		m_maskTex.Apply();
		m_maskMat.SetTexture("_Mask", m_maskTex);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (m_maskMat && m_blurMat)
		{
			RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			rt.filterMode = FilterMode.Point;
			rt.wrapMode = TextureWrapMode.Clamp;
			Graphics.Blit(source, rt, m_blurMat, 0);

			m_maskMat.SetTexture("_OriginTex", rt);
			Graphics.Blit(source, destination, m_maskMat, 0);
			RenderTexture.ReleaseTemporary(rt);
		}
		else
		{
			Debug.Log("no mat");
			Graphics.Blit(source, destination);
		}
	}
}
