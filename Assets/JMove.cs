using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class JMove : MonoBehaviour {
	private float m_speed = 5.0f;
	private float m_heightMin = -7;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position -= (m_speed * Time.deltaTime) * Vector3.up;
		if(transform.position.y <= m_heightMin)
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}
