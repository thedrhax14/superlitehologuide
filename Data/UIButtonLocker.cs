using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButtonLocker : MonoBehaviour {
	public UnityEngine.UI.Selectable selectable;
	public int LockLayer;

	public void ChangeLock (int dir) {
		LockLayer += Mathf.Clamp (dir, -1, 1);
		selectable.interactable = LockLayer <= 0;
	}
}