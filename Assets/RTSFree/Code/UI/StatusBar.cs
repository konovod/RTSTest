using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar : MonoBehaviour
{
	public GameObject HealthBar;
	public GameObject EnergyBar;
	public GameObject SelectView;

	public void Select(bool isSelected)
	{
		if (SelectView != null)
		{
			SelectView.SetActive(isSelected);
		}
	}

	public void SetHealth(float argValue)
	{
		if (HealthBar != null)
		{
			SetBar(HealthBar, argValue);
		}
	}

	public void SetEnergy(float argValue)
	{
		if (EnergyBar != null)
		{
			SetBar(EnergyBar, argValue);
		}
	}

	private void SetBar(GameObject argBar, float argValue)
	{
		if (argBar != null)
		{
			Vector3 s = argBar.transform.localScale;
			float sz = (0.25f / 100f) * argValue;
			argBar.transform.localScale = new Vector3(s.x, s.y, sz);
			Vector3 p = argBar.transform.localPosition;
			argBar.transform.localPosition = new Vector3(p.x, p.y, (0.25f - sz) / 4f);
		}
	}

}
