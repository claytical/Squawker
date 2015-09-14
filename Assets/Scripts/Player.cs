﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

public class Player : MonoBehaviour {

	public int tan = 0;
	public int style = 0;
	public int attractiveness = 0;
	public int cancerRisk = 0;
	public int actionsLeft = 0;
	public int daysLeft = 0;
	public int dermatologistVisits = 0;
	public TextAsset potentialMessages;
	public bool reset = false;
	public List<Message> inbox;
	public Inbox previewInbox;
	public Profile profile;
	
	public JSONNode json;
	private List<string> messageList;

	// Use this for initialization
	void Start () {

		json = JSON.Parse(potentialMessages.ToString());

		if (reset) {
			PlayerPrefs.DeleteKey("game in progress");
		}
		
		if (!PlayerPrefs.HasKey("game in progress")) {
			//Game in Progress, populate variables
			resetStats();
		}

		string[] storedMessages = Prefs.PlayerPrefsX.GetStringArray("messages", null, 0);
		messageList = storedMessages.OfType<string>().ToList();
		inbox = new List<Message>();
		refreshInbox();
		populateStats();
		updateProfile();

	}

	public int matches(string characterPath) {
		//Get character's attractiveness, between 0 and 100 to set as the initial response time

		int responseTime = json[characterPath]["requirements"]["love"].AsInt;

		//Get the difference between the two characters attractiveness. NPC with 50, player with 20, response time is 30. 
		responseTime -= attractiveness;

		//Add response time if NPC hates player style, reduce response time if the NPC loves the player style

		if (profile.character.wearingGlasses) {
			responseTime += json [characterPath] ["requirements"] ["accessories"] ["glasses"].AsInt;
		}

		if (profile.character.wearingTie) {
			responseTime += json [characterPath] ["requirements"] ["accessories"] ["tie"].AsInt;

		}

		if (profile.character.wearingBand) {
			responseTime += json[characterPath]["requirements"]["accessories"]["band"].AsInt;

		}

		if (profile.character.wearingRibbon) {
			responseTime += json[characterPath]["requirements"]["accessories"]["band"].AsInt;
		}


		if (tan <= json [characterPath] ["requirements"] ["tan"].AsInt) {
			//must meet tan requirement. If they don't, this gets manually pushed to 9999 to be unresponsive.
			responseTime = 9999;
		}


		//If the response time is negative, we just set it to 0 so the NPC responds instantly

		if (responseTime < 0) {
			responseTime = 0;
		}
		Debug.Log("Match will respond in " + responseTime + " days");
		return responseTime;
	}

	public void refreshInbox() {
		inbox.Clear();
		Debug.Log("MESSAGE LIST COUNT: " + messageList.Count);
		for (int i = 0; i < messageList.Count; i++) {
			string[] messageParts = StringArrayFunctions.getMessage(messageList[i]);
			if (int.Parse(messageParts[2]) <= 0) {
				//TODO: Check tan requirements. not sure if this is the best place
				if (tan <= json[messageParts[0]]["requirements"]["tan"]) {
					//Don't add message
				}
				else {
				}
					//new message, add to list
					Message message = new Message();
					message.index = i;
					message.path = messageList[i];
					message.sender = messageParts[0];
					message.passage = messageParts[1];
					message.subject = json[message.sender][message.passage]["subject"];
					message.body = json[message.sender][message.passage]["message"];
					JSONNode responses = json[message.sender][message.passage]["responses"];
					for (int j = 0; j < responses.Count; j++) {
						Response r = new Response(responses[j]["path"], responses[j]["response"], responses[j]["time"].AsInt, i);
						message.responses.Add(r);
					}

					inbox.Add(message);
					Debug.Log("Adding message to inbox");

			}
		}

		if (previewInbox != null) {
			previewInbox.clear();
			updatePreviewBox();
		}
	}

	public void updatePreviewBox() {
		for (int i = 0; i < inbox.Count; i++) {
			previewInbox.addMessage(inbox[i]);
		}
	}

	private void getMessageList() {
	}

	private void saveMessageList() {
		Prefs.PlayerPrefsX.SetStringArray("messages", messageList.ToArray());
	}

	public void addMessage(string path) {
		messageList.Add(path);
		saveMessageList();
		Debug.Log("Adding Message");
	}

	public void removeMessage(int index) {
		messageList.RemoveAt(index);
		saveMessageList();
	}

	public void removeAllMessages() {
		messageList.Clear();
		saveMessageList();
	}
	
	private void saveProgress(int actions, int days) {
		PlayerPrefs.SetInt("actions left", actions);
		PlayerPrefs.SetInt("days left", days);
	}
	
	public void newOffer(string type) {
		JSONNode offers = json [type].AsObject;
		
		Debug.Log ("Amount of Offers: " + offers.Count);
		int selectedOffer = Random.Range (0, offers.Count);
		JSONNode offer = offers [selectedOffer].AsObject;
		Debug.Log ("Picked Offer #" + selectedOffer + ", subject is: " + offer ["subject"]);
		addMessage(offer["path"]);

	}


	public string getDermatologistMessage(int index) {
		JSONNode dermatologistMessage = json ["doctor"]["conversation"][index]["doctor"];		
		return dermatologistMessage;
	}

	public string getDermatologistResponse(int index) {
		JSONNode dermatologistResponse = json ["doctor"]["conversation"][index]["patient"];		
		return dermatologistResponse;
	}


	private void newDay() {

		//ADD TANNING OFFER
//		newOffer("tanning");

		for(int i = 0; i < messageList.Count; i++) {
				//iterate through inbox, reduce wait time for each message
				string[] message = StringArrayFunctions.getMessage (messageList[i]);
				int currentDuration = int.Parse(message[2]);
				
				if(currentDuration > 0) {
				//TODO: Check tan requirements here and if not met, add 1?
					currentDuration-=1;
					Debug.Log("Decreasing Duration");
				}
				else {
					Debug.Log("Leaving Duration Alone : " + currentDuration);
				}
			//TODO: Look at parts of message
				messageList[i] = message[0] + "/" + message[1] + "/" + currentDuration.ToString();
			}
			saveMessageList();
		}


	public bool takeAction() {
		if (actionsLeft > 1) {
			actionsLeft--;
			saveProgress(actionsLeft, daysLeft);
			return true;
		}
		else {
			daysLeft--;
			newDay();
			refreshInbox();
			if (daysLeft < 0) {
				//GAME OVER
				Debug.Log("Days have run out");
				return false;
			}
			else {
				actionsLeft = 3;
				saveProgress(actionsLeft, daysLeft);
				return true;
			}
		}

	}

	public void setTan(int amount) {
		if (tan < amount) {
			//tan increased, increase risk by amount tanned
			int riskIncrease = amount - tan;
			cancerRisk += riskIncrease;
			PlayerPrefs.SetInt("cancer risk", cancerRisk);
		}
		tan = amount;

		PlayerPrefs.SetInt("tan", tan);
	}

	public void setAttractiveness(int amount) {
		//TODO: Decide if attractiveness is additive or just personal best in the love game
		attractiveness = amount;		
		PlayerPrefs.SetInt("attractiveness", amount);

	}

	private void resetStats() {
//		PlayerPrefs.DeleteKey ("messages");
		PlayerPrefs.DeleteAll();
		//Debug.Log (json.ToString());
		/*
		JSONNode offers = json ["characters"].AsObject;
		
		Debug.Log ("Amount of Offers: " + offers.Count);
		int selectedOffer = Random.Range (0, offers.Count);
		JSONNode offer = offers [selectedOffer].AsObject;



*/
		List<string> list = new List<string>();

		for (int i = 0; i < json["characters"].Count; i++) {
			list.Add(json["characters"][i]);
		}

		Prefs.PlayerPrefsX.SetStringArray("men", list.ToArray());
		Debug.Log("Saved " + list.Count + " characters");
		PlayerPrefs.SetInt("game in progress", 1);
		PlayerPrefs.SetInt("tan", Random.Range(0,10));
		PlayerPrefs.SetInt("attractiveness", Random.Range(0,10));
		PlayerPrefs.SetInt("style", Random.Range(0,10));
		PlayerPrefs.SetInt("cancer risk", 0);
		PlayerPrefs.SetInt("dermatologist visits", 0);
		PlayerPrefs.SetInt("actions left", 3);
		PlayerPrefs.SetInt("days left", 30);

	}
	
	private void populateStats() {
		tan = PlayerPrefs.GetInt("tan", 0);
		attractiveness = PlayerPrefs.GetInt("attractiveness", 0);
	
		profile.heart.CrossFadeAlpha (Remap (attractiveness, 0, 100, 0, 1), 3,true);
		//TODO: Style becomes an avatar choice with accessories
		style = PlayerPrefs.GetInt("style", 0);
		actionsLeft = PlayerPrefs.GetInt("actions left", 0);
		daysLeft = PlayerPrefs.GetInt("days left", 0);
		cancerRisk = PlayerPrefs.GetInt("cancer risk", 0);
		dermatologistVisits = PlayerPrefs.GetInt("dermatologist visits", 0);
	}

	public void updateProfile() {
		if (profile) {
			profile.actionsLeft.text = actionsLeft.ToString();
			profile.daysLeft.text = daysLeft.ToString("0 days left to get a date");
			profile.messageCount.text = inbox.Count.ToString();
			
		}
	}

	public string fetchMaleName() {
		int numNames = json["names"]["men"].Count;
		return json["names"]["men"][Random.Range(0,numNames)];
	}

	public string fetchMotto() {
		int numMottos = json["mottos"].Count;
		return json["mottos"][Random.Range (0, numMottos)];
	}

	public void returnHome(){
		Application.LoadLevel (0);
	}


	public float Remap (this float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
		
}
