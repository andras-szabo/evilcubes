using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponController))]
public class WeaponTester : MonoBehaviour 
{
	private WeaponController _wpn;	
	private WeaponController Wpn
	{
		get
		{
			return _wpn ?? (_wpn = GetComponent<WeaponController>());
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Wpn.HandleTriggerPull();
		}
		else if (Input.GetMouseButton(0))
		{
			Wpn.HandleTriggerHeld();
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Wpn.HandleTriggerLetGo();
		}
	}
}
