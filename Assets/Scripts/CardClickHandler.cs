using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 
// CardClickHandler - Helper class for subscribing to Card click events.
//                    Manually differentiates between click and drag because OnMouseDown
//					   and OnMouseDrag had funny interactions with each other
public class CardClickHandler : MonoBehaviour {
	public System.Action<Card> onClick = null;
	public System.Action<Card> onDrag = null;
	public System.Action<Card> onUp = null;

	float maxTimeForClick = 0.3f;

	bool isDown = false;
	bool needsToTriggerMouseUp = false;
	float mouseDownStart = 0.0f;	
	
	void OnMouseDown() {
		isDown = true;
		mouseDownStart = Time.time;
		if (onDrag != null) {
			onDrag(gameObject.GetComponent<Card>());
		} 
	}

	void OnMouseUp() {
		if (isDown) {
			if ((Time.time - mouseDownStart) <= maxTimeForClick) {
				needsToTriggerMouseUp = true;
				if (onClick != null) {
					onClick(gameObject.GetComponent<Card>());
				}
			} else {
				if (onUp != null) {
					onUp(gameObject.GetComponent<Card>());
				}
			}
		}
	}

	void Update() {
		if (needsToTriggerMouseUp) {
			if (onUp != null) {
				onUp(gameObject.GetComponent<Card>());
			}
			needsToTriggerMouseUp = false;
		}
	}
}