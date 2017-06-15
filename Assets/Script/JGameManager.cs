using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		InitScene();
		InitAllMirror();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void InitScene()
	{
		GameObject mirror_right = GameObject.Find("root_right");
		Vector3 scale = new Vector3(JCameraAdapter.Instance.worldWidth / 2, JCameraAdapter.Instance.worldHeight, 1);
		mirror_right.transform.localScale = scale;
		mirror_right.transform.position = new Vector3(JCameraAdapter.Instance.worldWidth / 2 / 2, 0, 0);
	}

	private void InitAllMirror()
	{
		GameObject right = GameObject.Find("root_right");
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
		left.name = "root_right";
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
