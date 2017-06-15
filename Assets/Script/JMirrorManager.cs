using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MirrorBehavior : MonoBehaviour
{
	#region Switch between left mirror and right mirror
	public static string SwitchName(bool mirror, string originName)
	{
		return mirror ? originName.Replace("left", "right") : originName.Replace("right", "left");
	}
	public static string SwitchName(string originName)
	{
		if (originName.Contains("left"))
		{
			return originName.Replace("left", "right");
		}else
		{
			return originName.Replace("right", "left");
		}
	}
	public static int SwitchIndex(bool mirror)
	{
		return mirror ? 0 : 1;
	}
	#endregion

	#region Find Mirror Sibling
	public static bool IsRootObj(string name)
	{
		return name == "root_left" || name == "root_right";
	}
	public static GameObject GetMirrorObj(GameObject obj)
	{
		Stack<string> name_stack = new Stack<string>();
		while (obj && !IsRootObj(obj.name))
		{
			name_stack.Push(obj.name);
			obj = obj.transform.parent.gameObject;
		}
		if (!obj)
			return null;

		GameObject parentObj = GameObject.Find(SwitchName(obj.name));
		while (name_stack.Count != 0)
		{
			parentObj = parentObj.transform.Find(name_stack.Pop()).gameObject;
		}
		return parentObj;
	}
	#endregion

	//--
	public bool m_curMirror;
	public void SetMirror(bool right)
	{
		m_curMirror = right;
		gameObject.name = SwitchName(m_curMirror, gameObject.name);
		OnMirror();
	}
	public void ToggleMirror()
	{
		m_curMirror = !m_curMirror;
		OnMirror();
	}

	public abstract void OnMirror();
}

public class JMirrorManager : MonoBehaviour
{

	public MirrorBehavior[] m_mirrors;
	private int m_count = 0;
	private int m_capacity = 4;

	public void RegisterMirror(MirrorBehavior m)
	{
		foreach (MirrorBehavior mb in m_mirrors)
		{
			if (mb == m)
			{
				return;
			}
		}

		if (m_count == m_capacity)
		{
			m_capacity <<= 2;
			MirrorBehavior[] new_mirrors = new MirrorBehavior[m_capacity];
			m_mirrors.CopyTo(new_mirrors, 0);
			m_mirrors = new_mirrors;
		}
		m_mirrors[m_count] = m;
		m_count++;
	}

	public void UnRegisterMirror(MirrorBehavior m)
	{
		for (int i = 0; i < m_count; i++)
		{
			if (m_mirrors[i] == m)
			{
				if (i == m_count - 1)
				{
					m_mirrors[i] = null;
				}
				else
				{
					m_mirrors[i] = m_mirrors[m_count - 1];
					m_mirrors[m_count - 1] = null;
				}
				m_count--;
				return;
			}
		}
	}

	// Use this for initialization
	void Awake()
	{
		m_instance = this;
		m_mirrors = new MirrorBehavior[m_capacity];
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			for (int i = 0; i < m_count; ++i)
			{
				m_mirrors[i].ToggleMirror();
			}
		}
	}

	static private JMirrorManager m_instance;
	static public JMirrorManager Instance { get { return m_instance; } }

}
