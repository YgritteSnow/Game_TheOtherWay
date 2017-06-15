using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JBackground : MirrorBehavior
{
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnMirror()
	{
		Color c = m_curMirror ? new Color(0.5f, 0.7f, 0.5f) : new Color(0.5f, 0.5f, 0.7f);
		Material left_mat = GetComponent<MeshRenderer>().material;
		left_mat.SetColor("_Color", c);
	}
}
