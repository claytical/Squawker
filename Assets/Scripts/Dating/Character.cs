﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;

public class Character : MonoBehaviour {

	public Image backgroundHair;
	public Image face;
	public Image hairLine;
	public Image eyes;
	public Image foregroundHair;
	public Image iris;
	public Image nose;
	public Image mouth;
	public Image brows;
	private Sprite[] backgroundHairStyles;
	private Sprite[] faceStyles;
	private Sprite[] hairlineStyles;
	private Sprite[] eyeStyles;
	private Sprite[] hairStyles;
	private Sprite[] irisStyles;
	private Sprite[] noseStyles;
	private Sprite[] mouthStyles;
	private Sprite[] eyeBrowStyles;
	private int selectedBackgroundHair;
	private int selectedFace;
	private int selectedHairLine;
	private int selectedEyes;
	private int selectedEyeBrow;
	private int selectedForegroundHair;
	private int selectedIris;
	private int selectedNose;
	private int selectedMouth;
	private int selectedHairColor;
	private int baseSkinTone;
	private int tanTone;
	private bool hasLongHair = false;
	private bool hasShortHair = false;
	public string name;
	private string characterAssignment;
	private Player player;

//	private Image 
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		if (name == "me") {
			characterAssignment = name;
			assign ();
		} 

	}


	public void assign(string character = null) {
		if (character != null) {
			characterAssignment = character;
		}
		Debug.Log("My Character Assignment is " + characterAssignment);
		if (PlayerPrefs.HasKey (characterAssignment)) {
			Debug.Log("Has Character Assignment for " + characterAssignment);
			loadCharacter();
			loadStyles ();
			assignStyles ();
			
		} else {
			Debug.Log("Generating Character Assignment");
			
			randomlyGenerate();
		}

	}

	public string getCharacterAssignment() {
		return characterAssignment;
	}

	public void randomlyGenerate() {
		randomlyPickColors ();

		loadStyles ();
		generateAvatar ();
		assignStyles ();

	}

	public void saveCharacter() {
		int[] prefs = new int[12] {baseSkinTone, tanTone, selectedFace,selectedHairColor,selectedEyeBrow,selectedBackgroundHair, selectedForegroundHair, selectedHairLine, selectedEyes, selectedIris, selectedNose, selectedMouth};
		Prefs.PlayerPrefsX.SetIntArray(characterAssignment + "_avatar", prefs);
		bool[] options = new bool[]{ hasLongHair, hasShortHair };
		Prefs.PlayerPrefsX.SetBoolArray (characterAssignment + "_options", options);
		PlayerPrefs.SetString (characterAssignment, name);
	}

	public void loadCharacter() {
		Debug.Log ("Loading " + characterAssignment);
		name = PlayerPrefs.GetString (characterAssignment);
		Debug.Log("Has name of " + name);
		int[] prefs = Prefs.PlayerPrefsX.GetIntArray (characterAssignment + "_avatar");
		bool[] options = Prefs.PlayerPrefsX.GetBoolArray (characterAssignment + "_options");
		hasLongHair = options [0];
		hasShortHair = options [1];
		baseSkinTone = prefs [0];
		tanTone = prefs [1];
		selectedFace = prefs [2];
		selectedHairColor = prefs [3];
		selectedEyeBrow = prefs [4];
		selectedBackgroundHair = prefs [5];
		selectedForegroundHair = prefs [6];
		selectedHairLine = prefs [7];
		selectedEyes = prefs [8];
		selectedIris = prefs [9];
		selectedNose = prefs [10];
		selectedMouth = prefs [11];
	}

	public void assignCharacter(string type) {
		string [] dates = Prefs.PlayerPrefsX.GetStringArray(type);
		//TODO: Check if there are no more potential characters
		Debug.Log("There are currently " + dates.Length + " characters left");
		List<string> list = new List<string>(dates);
//		int numNames = player.json["names"]["men"].Count;
//		name = player.json ["names"] [type] [Random.Range (0, numNames)];
		int selectedCharacter = Random.Range(0, list.Count);
		characterAssignment = list[selectedCharacter];
		list.RemoveAt(selectedCharacter);
		Prefs.PlayerPrefsX.SetStringArray(type,list.ToArray());
		PlayerPrefs.SetString(characterAssignment, name);
		Debug.Log("NAME IS " + name);

	}

	public void assignName(string type) {
		int numNames = player.json["names"]["men"].Count;
		name = player.json ["names"] [type] [Random.Range (0, numNames)];
	}


	public Sprite getHairStyle() {
		return backgroundHairStyles [Random.Range (0, backgroundHairStyles.Length)];
	}

	public void randomlyPickColors() {

		if (Random.Range (0, 2) == 1) {
			hasShortHair = true;
		}

		if (Random.Range (0, 2) == 1) {
			hasLongHair = true;
		}

		selectedHairColor = Random.Range (0, 8);
		baseSkinTone = Random.Range (0, 8);
		tanTone = 0;
		PlayerPrefs.SetInt("selected hair color", selectedHairColor);
		PlayerPrefs.SetInt ("selected skin tone", baseSkinTone);
		PlayerPrefs.SetInt ("tan tone", tanTone);
	}


	public void loadStyles() {
			//TODO: When skin tone and color exports arrive, place each in a subfolder, store skin tone in playerprefs
			//face and skintone affected assets will ultimately be: Avatar/Face/<basetone>/<shade>/
			//other assets will be: Avatar/iris/<color>
			faceStyles = Resources.LoadAll<Sprite> ("Avatar/Face/" + baseSkinTone + "/" + tanTone);
			hairlineStyles = Resources.LoadAll<Sprite> ("Avatar/Hair/" + selectedHairColor + "/Line");
			backgroundHairStyles = Resources.LoadAll<Sprite> ("Avatar/Hair/" + selectedHairColor + "/Long");
			hairStyles = Resources.LoadAll<Sprite> ("Avatar/Hair/" + selectedHairColor + "/Short");
			eyeBrowStyles = Resources.LoadAll<Sprite> ("Avatar/Hair/" + selectedHairColor + "/Brows");
			eyeStyles = Resources.LoadAll<Sprite> ("Avatar/Eyes");
			irisStyles = Resources.LoadAll<Sprite> ("Avatar/Iris");
			noseStyles = Resources.LoadAll<Sprite> ("Avatar/Nose");
			mouthStyles = Resources.LoadAll<Sprite> ("Avatar/Mouth");

	}

	public void assignStyles() {

		if (hasShortHair) {
			foregroundHair.sprite = hairStyles [selectedForegroundHair];
		} else {
			foregroundHair.enabled = false;
		}

		if (hasLongHair) {
			backgroundHair.sprite = backgroundHairStyles[selectedBackgroundHair];

		} else {
			backgroundHair.enabled = false;
		}

		face.sprite = faceStyles[selectedFace];
		hairLine.sprite = hairlineStyles [selectedHairLine];
		brows.sprite = eyeBrowStyles [selectedEyeBrow];
		eyes.sprite = eyeStyles [selectedEyes];
		iris.sprite = irisStyles [selectedIris];
		nose.sprite = noseStyles [selectedNose];
		mouth.sprite = mouthStyles [selectedMouth];
		                         
	}

	public void generateAvatar() {
			selectedBackgroundHair = Random.Range (0, backgroundHairStyles.Length);
			selectedFace = Random.Range (0, faceStyles.Length);
			selectedHairLine = Random.Range (0, hairlineStyles.Length);
			selectedEyes = Random.Range (0, eyeStyles.Length);
			selectedForegroundHair = Random.Range (0, hairStyles.Length);
			selectedIris = Random.Range (0, irisStyles.Length);
			selectedNose = Random.Range (0, noseStyles.Length);
			selectedMouth = Random.Range (0, mouthStyles.Length);
	}
	// Update is called once per frame
	void Update () {
	
	}
}