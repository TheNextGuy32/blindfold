﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Transform cameraTransform;
    [SerializeField]
    //private CharacterController charControl;
    private Vector2 velocity;

	private float scale = 2.5f;
    public float moveSpeed = 100;
	private Vector2 movement;

	[SerializeField]
	private GameObject knife;
    [SerializeField]
    private Image knifeFillImage;
    public float knifeCooldown;
    private float knifeTimer = 0;
    private bool canThrow = true;

    public AudioClip knifeThrowSound;
    private AudioSource audio;

    // Use this for initialization
    void Start ()
    {
        velocity = Vector2.zero;

        audio = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector2 mouseDir = new Vector2(Input.mousePosition.x / Screen.width - 0.5f, Input.mousePosition.y / Screen.height - 0.5f);
        transform.forward = mouseDir;
        
        InputMovement();
        var w = Input.GetKey("w");
        var d = Input.GetKey("d");
        var s = Input.GetKey("s");
        var a = Input.GetKey("a");

        if (!(w && s))
        {
            if (w)
            {
                velocity = new Vector2(velocity.x, 1 * moveSpeed);
            }
            if (s)
            {
                velocity = new Vector2(velocity.x, -1 * moveSpeed);
            }
        }
        if (!w && !s)
        {
            velocity = new Vector2(velocity.x, 0) * moveSpeed;
        }
        if (!(a && d))
        {
            if (a)
            {
                velocity = new Vector2(-1 * moveSpeed, velocity.y);
            }
            if (d)
            {
                velocity = new Vector2(1 * moveSpeed, velocity.y);
            }
        }
        if (!a && !d)
        {
            velocity = new Vector2(0, velocity.y);
        }

        if (canThrow)
        {
            if (Input.GetMouseButton(0))//Left
            {
                canThrow = false;

                //  Throw knife
				GameObject g = GameObject.Instantiate(knife);
				g.GetComponent<KnifeThrow>().Setup(this.gameObject);
                knifeFillImage.color = new Color(1, 0, 0, 1);
                knifeFillImage.fillAmount = 0;
            }
        }
        else
        {
            if (knifeTimer > knifeCooldown)
            {
                knifeTimer = 0;
                canThrow = true;
                knifeFillImage.fillAmount = 1;
                knifeFillImage.color = new Color(0, 1, 0, 1);
            }
            else
            {
                knifeTimer += Time.deltaTime;
                knifeFillImage.fillAmount = knifeTimer / knifeCooldown;
            }
        }



        //Moving player and then camera
        transform.position += (Vector3)(velocity * Time.deltaTime);
        cameraTransform.position = transform.position+new Vector3(0, 0, -30);
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
		
		//following code used to make player character face mouse
		Vector2 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);       //Mouse position
		Vector2 objpos = Camera.main.WorldToViewportPoint(transform.position);        //Object position on screen
		Vector2 relobjpos = new Vector2(objpos.x - 0.5f, objpos.y - 0.5f);            //Set coordinates relative to object's center
		Vector2 relmousepos = new Vector2(mouse.x - 0.5f, mouse.y - 0.5f) - relobjpos;//Mouse cursor relative to object's center
		float angle = Vector2.Angle(Vector2.up, relmousepos);                         //Angle calculation
		
		//if mouse is on the left side of our object
		if (relmousepos.x > 0)
			angle = 360 - angle;
		
		//Uncomment this block to make the player move based on the mouse cursor
		/*float mouseDist = Mathf.Sqrt((relmousepos.x * relmousepos.x) + (relmousepos.y * relmousepos.y)) * 10
		if (mouseDist > 0.7f || v >= 0) 
		{
			movement = angle * movement;
		}
		*/

		transform.rotation = Quaternion.Euler(0, 0, angle);
		//velocity = new Vector2(movement.x, movement.y);
		//GetComponent<Rigidbody2D>().rotation = angle;

	}

    void OnTriggerEnter(Collider other)
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
}
