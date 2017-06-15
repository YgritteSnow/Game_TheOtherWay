using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPlayer : MirrorBehavior
{
	public float speed = 1.0f;
	private float m_movingLeft;
	public Material[] m_materials;

	public Transform m_clampParent;

	public float m_clampLeft = 0;
	public float m_clampRight = 0;

	private void Start()
	{
		ResetClamp();
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
		string new_parentName = SwitchName(m_curMirror, m_clampParent.name);
		m_clampParent = GameObject.Find(new_parentName).transform;

		int mat_idx = SwitchIndex(m_curMirror);
		GetComponent<MeshRenderer>().material = m_materials[mat_idx];

		ResetClamp();
	}

	private void ResetClamp()
	{
		Vector3 bound_lb = m_clampParent.localToWorldMatrix.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0));
		Vector3 bound_rt = m_clampParent.localToWorldMatrix.MultiplyPoint(new Vector3(0.5f, 0.5f, 0));
		Vector3 bound_lb_player = transform.localToWorldMatrix.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0));
		Vector3 bound_rt_player = transform.localToWorldMatrix.MultiplyPoint(new Vector3(0.5f, 0.5f, 0));
		float player_width = Mathf.Abs(bound_lb_player.x - bound_rt_player.x);
		m_clampLeft = Mathf.Min(bound_lb.x, bound_rt.x) + player_width / 2;
		m_clampRight = Mathf.Max(bound_lb.x, bound_rt.x) - player_width / 2;

		Vector3 bound_local = new Vector3(m_clampLeft, 0, 0);
		m_clampLeft = m_clampParent.worldToLocalMatrix.MultiplyPoint(bound_local).x;
		bound_local.x = m_clampRight;
		m_clampRight = m_clampParent.worldToLocalMatrix.MultiplyPoint(bound_local).x;
		if(m_clampLeft > m_clampRight)
		{
			float tmp = m_clampRight;
			m_clampRight = m_clampLeft;
			m_clampLeft = tmp;
		}
	}
}
