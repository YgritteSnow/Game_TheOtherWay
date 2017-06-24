using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JCameraAdapter : MonoBehaviour {
	[HideInInspector]public float worldWidth;
	[HideInInspector]public float worldHeight;
	[HideInInspector]public float pixelWidth;
	[HideInInspector]public float pixelHeight;
	private static JCameraAdapter m_instance;
	public static JCameraAdapter Instance { get{ return m_instance; } }

	private void Awake()
	{
		m_instance = this;
		Camera cam = gameObject.GetComponent<Camera>();
		Rect tmp = cam.pixelRect;
		pixelWidth = Screen.width;
		pixelHeight = Screen.height;
		worldHeight = cam.orthographicSize * 2;
		worldWidth = pixelWidth / pixelHeight * worldHeight;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

	}
}
