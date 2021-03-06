﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CardboardAudioSource))]
public class PortalOpen : MonoBehaviour {
	public AudioClip portalOpen;
	public AudioClip portalSoundLoop;
	public bool autoOpen = false;
	public float portalPositionX;
	public float portalPositionY;
	public float portalPositionZ;
	public ParticleSystem portalEffect;
	private float waitBeforeOpen;
	private Vector3 portalOriginalPosition;
	private CardboardAudioSource portalSoundSource;
	private bool isOpenPortal = false;
	private bool isPortalSoundLoopPlaying = false;
	public static bool isExperienceComplete = false;
	private bool startCountdown = false;
	private float playerOriginalPosition;
	private GameObject thePlayer;
	private Dictionary<string, GameObject> gameObjectsInHandReference;
	private Dictionary<string, GameObject> pickableGameObjectsReference;
	public LayerMask layerMask;
	private bool hasSouveneirClipBeenPlayed = false;

	// Use this for initialization
	void Start () {
		waitBeforeOpen = 1.0f;
		thePlayer = GameObject.Find ("CardboardMain/FPSController");
		playerOriginalPosition = thePlayer.transform.position.z;
		portalOriginalPosition = this.transform.localPosition;
		portalSoundSource = this.gameObject.GetComponent<CardboardAudioSource> ();
		portalSoundSource.clip = portalOpen;
		portalSoundSource.loop = false;

		GameObject[] pickableGameObjects = GameObject.FindGameObjectsWithTag ("Pickable");
		pickableGameObjectsReference = new Dictionary<string, GameObject> ();
		foreach (GameObject gameObject in pickableGameObjects) {
			pickableGameObjectsReference.Add(gameObject.name, gameObject);
			if (isExperienceComplete) {
				gameObject.SetActive (true);
			} else {
				gameObject.SetActive (false);
			}
		}

		if (SceneManager.GetActiveScene ().name == "Lab") {
			GameObject[] gameObjectsInHand = GameObject.FindGameObjectsWithTag ("Picked");
			gameObjectsInHandReference = new Dictionary<string, GameObject> ();
			foreach (GameObject gameObject in gameObjectsInHand) {
				gameObjectsInHandReference.Add(gameObject.name, gameObject);
				gameObject.SetActive(false);
			}
		}

		if (isExperienceComplete) {
			autoOpen = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if ((Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Vertical") > 0) && !startCountdown) {
			startCountdown = true;
		}

		if (isExperienceComplete && !hasSouveneirClipBeenPlayed) {
			CardboardAudioSource playerAudioSource = thePlayer.GetComponent<CardboardAudioSource> ();
			playerAudioSource.Play();
			hasSouveneirClipBeenPlayed = true;
		}

		if (Input.GetButtonDown ("Fire1")) {
			if (isExperienceComplete) {
				RaycastHit hitInfo;
				if (Physics.Raycast (Cardboard.SDK.GetComponentInChildren<CardboardHead> ().Gaze, out hitInfo, Mathf.Infinity, layerMask)) {
					if (hitInfo.transform.gameObject.tag == "Pickable") {
						GameObject pickableObject = hitInfo.transform.gameObject;
						pickableObject.SetActive(false);
						GameObject objectPickedInHand = gameObjectsInHandReference[pickableObject.name + "inHand"];
						objectPickedInHand.SetActive(true);
					}
				}
			}
		}
		if (autoOpen && startCountdown) {
			waitBeforeOpen -= Time.deltaTime;
			if (waitBeforeOpen <= 0.0f) {
				openPortal ();
			}
		}

		if (isOpenPortal) {
			this.transform.localPosition = Vector3.Lerp (this.transform.localPosition, 
				new Vector3 (portalPositionX, portalPositionY, portalPositionZ), Time.deltaTime * 1.5f);
			if (!portalSoundSource.isPlaying && !isPortalSoundLoopPlaying) {
				portalSoundSource.clip = portalSoundLoop;
				portalSoundSource.loop = true;
				portalSoundSource.Play ();
				isPortalSoundLoopPlaying = true;
			}
		}
	}

	public void openPortal() {
		autoOpen = false;
		isOpenPortal = true;
		portalEffect.Play ();
		portalSoundSource.PlayDelayed (2.7f);
	}
}
