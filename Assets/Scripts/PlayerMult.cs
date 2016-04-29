﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMult : MonoBehaviour
{
    private Transform cameraTransform;
    public Vector3 velocity;

	private float scale = 2.5f;
	private Vector2 movement;
	[SerializeField]
	private PhotonView playerPV;
	[SerializeField]
	private GameObject knife;
    private Image knifeFillImage;
    public float knifeCooldown;
    private float knifeTimer = 0;
    private bool canThrow = true;

    public AudioClip knifeThrowSound;
    private AudioSource audio;

	//Sync
	private NetworkSync ns;

    // Use this for initialization
    void Start ()
    {
        velocity = Vector3.zero;
		if (playerPV.isMine) {
			cameraTransform = Camera.main.transform;
			knifeFillImage = GameObject.FindGameObjectWithTag ("Fill").GetComponent<Image> ();
			knifeFillImage.color = new Color (1, 0, 0, 1);
			knifeFillImage.fillAmount = 0;
			canThrow = false;
			knifeTimer = 0;
		}
		ns = GetComponent<NetworkSync> ();
        audio = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (playerPV.isMine) {
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
			Vector3 mouseDir = new Vector3 (Input.mousePosition.x / Screen.width - 0.5f, Input.mousePosition.y / Screen.height - 0.5f, 0);
			transform.forward = mouseDir;
	        
			InputMovement ();
	
			if (canThrow) {
				if (Input.GetMouseButton (0)) {//Left
					canThrow = false;
	
					//  Throw knife
					GameObject g = (GameObject)PhotonNetwork.Instantiate (knife.name, transform.position + Vector3.forward, transform.rotation, 0);
					g.GetComponent<KnifeThrow> ().Setup (this.gameObject);
					knifeFillImage.color = new Color (1, 0, 0, 1);
					knifeFillImage.fillAmount = 0;
				}
			} else {
				if (knifeTimer > knifeCooldown) {
					knifeTimer = 0;
					canThrow = true;
					knifeFillImage.fillAmount = 1;
					knifeFillImage.color = new Color (0, 1, 0, 1);
				} else {
					knifeTimer += Time.deltaTime;
					knifeFillImage.fillAmount = knifeTimer / knifeCooldown;
				}
			}

			//Keeping player at a z-pos of 0
	
			//Moving player and then camera
			GetComponent<Rigidbody> ().MovePosition (GetComponent<Rigidbody> ().position + (velocity * Time.deltaTime));
			transform.position = GetComponent<Rigidbody> ().position;
			cameraTransform.position = transform.position + new Vector3 (0, 0, -20);
		} 
		else 
		{
			SyncedMovement ();
		}
	}

	private void InputMovement()
	{

		//used to get input for direction
		float v = Input.GetAxis("Vertical");
		float h = Input.GetAxis("Horizontal");

		v *= scale;
		h *= scale;
 
		//store Movement
		movement = new Vector2 (h, v);

		// Distance from camera to object. In case it becomes !0.
		float camDis = cameraTransform.position.y - this.transform.position.y;

		// Get the mouse position in world space. Using camDis for the Z axis.
		Vector3 mouse = Camera.main.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, camDis));

		float AngleRad = Mathf.Atan2 (mouse.y - transform.position.y, mouse.x - transform.position.x);
		float angle = (180 / Mathf.PI) * AngleRad - 90;

		//if mouse is on the left side of our object
		if (Input.mousePosition.x < 0)
			angle = 360 - angle;
		
		//Uncomment this block to make the player move based on the mouse cursor
		/*float mouseDist = Mathf.Sqrt((relmousepos.x * relmousepos.x) + (relmousepos.y * relmousepos.y)) * 10
		if (mouseDist > 0.7f || v >= 0) 
		{
			movement = angle * movement;
		}
		*/

		transform.rotation = Quaternion.Euler(0, 0, angle);
		velocity = new Vector3(movement.x, movement.y, 0);
		//GetComponent<Rigidbody2D>().rotation = angle;

	}

	private void SyncedMovement()
	{
		ns.syncTime += Time.deltaTime;
		GetComponent<Rigidbody>().position = Vector3.Lerp(ns.syncStartPosition, ns.syncEndPosition, ns.syncTime / ns.syncDelay);
		transform.position = GetComponent<Rigidbody> ().position;
	}
	void OnTriggerEnter2D(Collider2D other)
    {
        switch(other.gameObject.tag)
        {
            case "Solid Object":
                
                velocity *= -0.8f;
                
                break;
            
            case "Collectable":
                //objectsCollected++;
                Destroy(other.gameObject);
                break;
        }
    }
	public void Respawn()
	{
		transform.position = Vector3.zero;
		GetComponent<Rigidbody> ().position = transform.position;
		knifeFillImage.color = new Color (1, 0, 0, 1);
		knifeFillImage.fillAmount = 0;
		canThrow = false;
		knifeTimer = 0;
	}
}