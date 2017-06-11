using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MirrorBehavior : MonoBehaviour
{
	public bool m_curMirror;
	public void SetMirror(bool right)
	{
		if(m_curMirror != right)
		{
			m_curMirror = right;
		}
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
