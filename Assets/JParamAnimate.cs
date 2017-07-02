using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JParamAnimate : MonoBehaviour
{
	public EventDelegateFloat paramFunc = null;
	public float m_magnify = 3.0f;
	public Material m_mat;

	// Use this for initialization
	void Start () {
		//m_mat = GetComponent<>
	}
	
	// Update is called once per frame
	void Update () {
		if (paramFunc==null || !m_mat)
			return;

		//float param = paramFunc.Execute();
		float param = JAudioAnalysis_maxAmp.Instance().GetMaxAmp();
		m_mat.SetTextureOffset("_MainTex", new Vector2(0, param * m_magnify));
	}
}
