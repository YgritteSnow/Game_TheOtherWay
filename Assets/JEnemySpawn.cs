using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JEnemySpawn : MonoBehaviour {
	public float m_spawnInterval = 1.0f;
	public float m_minSpawnAmp = 0.1f;
	public float m_maxSpawnAmp = 0.2f;

	private float m_timeDelta = 0.0f;

	[System.Serializable]
	public struct EnemyParam
	{
		public EnemyParam(float w) { width = w; prefab = null; prefab_danger = null; }
		public float width;
		public GameObject prefab;
		public GameObject prefab_danger;
	}
	public EnemyParam[] m_enemyParam = new EnemyParam[3];
	private int[] m_stampWidth = null;

	private float m_maxX = 0.0f;
	// Use this for initialization
	void Start () {
		m_stampWidth = new int[m_enemyParam.Length];
		int spec_len = JAudioAnalysis.Instance().SampleSpectrum.Length;
		m_maxX = Mathf.Log(spec_len);
	}
	
	// Update is called once per frame
	void Update ()
	{
		DebugDraw();

		m_timeDelta += Time.deltaTime;
		if (m_timeDelta < m_spawnInterval)
		{
			return;
		}
		m_timeDelta = 0;

		float[] enemy_max = new float[m_enemyParam.Length];
		int[] enemy_max_idx = new int[m_enemyParam.Length];
		int cur_enemy_idx = 0;
		float[] spec = JAudioAnalysis.Instance().SampleSpectrum;
		
		int allmax_idx = 0;
		for (int idx = 0; idx < spec.Length; ++idx)
		{
			float x = Mathf.Log(idx + 1);
			while(m_enemyParam[cur_enemy_idx].width < x / m_maxX)
			{
				++cur_enemy_idx;
				enemy_max[cur_enemy_idx] = 0;
			}
			if(spec[idx] > spec[allmax_idx])
			{
				allmax_idx = idx;
			}

			if (spec[idx] > enemy_max[cur_enemy_idx])
			{
				enemy_max[cur_enemy_idx] = spec[idx];
				enemy_max_idx[cur_enemy_idx] = idx;
			}
		}
		
		for(int idx = 0; idx < enemy_max.Length; ++idx)
		{
			if(enemy_max[idx] < m_minSpawnAmp)
			{
				continue;
			}

			GameObject enemy = GameObject.Instantiate(enemy_max[idx] < m_maxSpawnAmp ? m_enemyParam[idx].prefab : m_enemyParam[idx].prefab_danger);
			enemy.transform.position = new Vector3(Mathf.Log(enemy_max_idx[idx]+1) / m_maxX * JCameraAdapter.Instance.worldWidth/2, 5, -1);
		}

	}

	private void DebugDraw()
	{
		float[] spec = JAudioAnalysis.Instance().SampleSpectrum;
		int draw_enemy_idx = 0;
		for (int i = 1; i < spec.Length - 1; i++)
		{
			float log_i = Mathf.Log(i + 1);
			Color color = Color.red;
			while (m_enemyParam[draw_enemy_idx].width < log_i / m_maxX)
			{
				++draw_enemy_idx;
				color = Color.green;
			}

			float x = 1.5f;
			Debug.DrawLine(new Vector3(Mathf.Log(i - 1) * x, spec[i - 1] - 5, -2), new Vector3(Mathf.Log(i) * x, spec[i] - 5, -2), color);
		}
	}
}
