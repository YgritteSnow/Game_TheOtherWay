using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPlayer : MirrorBehavior
{
	public float speed = 1.0f;
	private float m_movingLeft;
	public Material[] m_materials;

	public float m_clampLeft = -1.0f;
	public float m_clampRight = 1f;

	private void Start()
	{
		Debug.Log("m_clamp:" + m_clampLeft + ", " + m_clampRight);
	}

	// Update is called once per frame
	void Update()
	{
		m_movingLeft = Input.GetAxisRaw("Horizontal") * speed;
	}

	private void LateUpdate()
	{
		transform.Translate(speed * m_movingLeft, 0, 0);
		if(transform.localPosition.x < m_clampLeft || transform.localPosition.x > m_clampRight)
		{
			Vector3 pos = transform.localPosition;
			transform.localPosition = new Vector3(Mathf.Clamp(pos.x, m_clampLeft, m_clampRight), pos.y, pos.z);
		}
	}

	public override void OnMirror()
	{
		int mat_idx = m_curMirror ? 0 : 1;
		GetComponent<MeshRenderer>().material = m_materials[mat_idx];
	}
}
