using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JCamera : MonoBehaviour {
	public Transform m_player;
	private Vector3 m_offset;

	// Use this for initialization
	void Start () {
		m_offset = transform.position - m_player.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	private void LateUpdate()
	{
		transform.position = m_offset + m_player.position;
	}
}
