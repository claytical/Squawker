﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TanningSalonAssistant : MonoBehaviour {

	public Text dialogue;
	public Text response;
	public Player player;
	public GameObject waiver;
	public GameObject tanControls;
	public GameObject dialogControls;
	public Animator meter;
	public Animator bed;
	public GameObject continueButton;
	public GameObject stopTanButton;

	private int conversationIndex = 0;
	private bool signedWaiver = false;
	// Use this for initialization
	void Start () {
	}

	public void respond() {
		dialogue.text = player.getTanningSalonAssistantMessage (conversationIndex);
		response.text = player.getTanningSalonAssistantResponse (conversationIndex);
		conversationIndex++;
		if (conversationIndex == 3) {
			gameObject.SetActive(false);
			waiver.SetActive(true);
			continueButton.SetActive (false);
		}
		if (conversationIndex == 4) {
			if (signedWaiver) {
				dialogControls.SetActive(false);
				Debug.Log("Activating Salon");
				tanControls.SetActive(true);
				bed.SetTrigger("close");
				meter.SetTrigger ("start");
				stopTanButton.SetActive (true);
				Debug.Log ("Should have set tan button active");
			}
			else {
				player.loadSceneNumber (1);
			}
		}
		if (conversationIndex == 5) {
			player.loadSceneNumber (1);
		}
	}

	public void accept() {
		//TODO: Animate transition between waiver
		waiver.SetActive (false);
		gameObject.SetActive (true);
		signedWaiver = true;
		continueButton.SetActive (true);
		bed.SetTrigger("open");
	}

	public void decline() {
		//TODO: Should log analytics for declining offer
		//TODO: Animate the transition
		dialogue.text = "Changed your mind? That's okay. Maybe next time!";
		response.text = "Ok!";
		waiver.SetActive (false);
	}

}
