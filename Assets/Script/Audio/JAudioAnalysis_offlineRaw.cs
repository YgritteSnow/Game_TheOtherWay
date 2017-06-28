using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 离线处理音频的原始波形
 */

public class JAudioAnalysis_offlineRaw : MonoBehaviour
{
	public WAudioAnalysis.AudioRawDataCache m_audioRawCache;

	// Use this for initialization
	void Awake ()
	{
		m_audioRawCache = new WAudioAnalysis.AudioRawDataCache(GetComponent<AudioSource>().clip);
	}
	
	// Update is called once per frame
	void Update () {
		for(int x = 1; x < m_audioRawCache.rawData.Length; ++x)
		{
			float zoom = 0.00001f;
			Debug.DrawLine(new Vector3((x - 1) * zoom, m_audioRawCache.rawData[x-1], 1), new Vector3(x * zoom, m_audioRawCache.rawData[x], 1), Color.red);
		}
	}
}
