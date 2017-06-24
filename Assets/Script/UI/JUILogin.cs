using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JUILogin : MonoBehaviour {

	public Button m_btnEnter;
	// Use this for initialization
	void Start () {
		m_btnEnter = GetComponent<Button>();
		m_btnEnter.onClick.AddListener(onEnter);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void onEnter()
	{
		SceneManager.LoadScene("level");
	}
}
