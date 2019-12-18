﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private GameObject[] menus = null; // 0 = FastChoice (for now)
	[SerializeField] private Text[] promptSpaces = null;
	[SerializeField] private Image fillBar = null;

	ChoiceFreeze cf;

	static GameManager inst;

	public float FillBarAmount
	{
		get { return fillBar.fillAmount; }
		set { fillBar.fillAmount = value; }
	}

	public ChoiceFreeze choiceFreeze
	{
		get { return cf; }
	}


	public static GameManager instance
	{
		get { return inst; }
	}




    // Start is called before the first frame update
    private void Awake()
    {
		if (instance == null || instance == this)
			inst = this;
		else
			Destroy(gameObject);
		cf = GetComponent<ChoiceFreeze>();
    }

	public void DisplayFastChoice(bool enabled)
	{
		menus[0].SetActive(enabled);
	}

	public void DisplayFastChoice(bool enabled, FastCall[] prompts)
	{
		//Resets the prompts on the canvas
		foreach(Text t in promptSpaces)
		{
			t.text = "";
		}

		menus[0].SetActive(enabled);

		for (int i = 0; i < prompts.Length; i++)
		{
			promptSpaces[i].text = (i+1) + ". " + prompts[i].callText;
		}
	}
}
