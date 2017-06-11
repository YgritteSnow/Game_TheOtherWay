using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		InitAllMirror();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void InitAllMirror()
	{
		GameObject right = GameObject.Find("mirror_right");
		right.GetComponent<MirrorBehavior>().SetMirror(true);
		foreach(Transform g in right.transform)
		{
			MirrorBehavior m = g.GetComponent<MirrorBehavior>();
			if (m)
			{
				m.SetMirror(true);
			}
		}

		GameObject left = Instantiate(right);
		Vector3 mirror_map = new Vector3(-1, 1, 1);
		left.name = "mirror_right";
		Vector3 pos = left.transform.position;
		pos.Scale(mirror_map);
		left.transform.position = pos;
		Vector3 scale = left.transform.localScale;
		scale.Scale(mirror_map);
		left.transform.localScale = scale;
		left.GetComponent<MirrorBehavior>().SetMirror(false);
		foreach (Transform g in left.transform)
		{
			MirrorBehavior m = g.GetComponent<MirrorBehavior>();
			if (m)
			{
				m.SetMirror(false);
			}
		}
	}
}
