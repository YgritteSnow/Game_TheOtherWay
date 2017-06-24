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
		Debug.Log(right.transform.parent);
		right.GetComponent<MirrorBehavior>().SetMirror(true);
		foreach(Transform g in right.transform)
		{
			MirrorBehavior m = g.GetComponent<MirrorBehavior>();
			if (m)
			{
				m.SetMirror(true);
			}
		}

		IterInitMirror(right.transform, null);
	}

	private void IterInitMirror(Transform right, Transform left_Parent)
	{
		GameObject right_obj = right.gameObject;
		Transform left = null;
		string left_name = MirrorBehavior.SwitchName(right_obj.name);
		if (!left_Parent)
		{
			GameObject left_obj = GameObject.Find(left_name);
			left = left_obj ? left_obj.transform : null;
		}
		else
		{
			left = left_Parent.Find(left_name);
		}
		if (!left)
		{
			GameObject left_obj = GameObject.Instantiate(right_obj);
			left = left_obj.transform;
			left.parent = left_Parent;

			left_obj.name = left_name;
			ResetMirrorData(left_obj, right_obj, false);
		}
		else
		{
			ResetMirrorData(left.gameObject, right_obj, false);
		}

		for(int idx = 0; idx < right_obj.transform.childCount; ++idx)
		{
			IterInitMirror(right_obj.transform.GetChild(idx), left);
		}
	}

	private void ResetMirrorData(GameObject left, GameObject right, bool is_right)
	{
		Vector3 mirror_map = new Vector3(-1, 1, 1);

		Vector3 pos = right.transform.position;
		pos.Scale(mirror_map);
		left.transform.position = pos;

		Vector3 scale = right.transform.localScale;
		scale.Scale(mirror_map);
		left.transform.localScale = scale;

		left.GetComponent<MirrorBehavior>().SetMirror(is_right);
		foreach (Transform g in left.transform)
		{
			MirrorBehavior m = g.GetComponent<MirrorBehavior>();
			if (m)
			{
				m.SetMirror(is_right);
			}
		}
	}
}
