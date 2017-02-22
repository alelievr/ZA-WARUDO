using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

	public Transform		head;
	public Transform		bulletShooter;
	public GameObject		bulletPrefab;
	public GameObject		cameraObject;

	[SpaceAttribute]
	public float			walkSpeed = 3;
	public float			runSpeed = 6;
	public float			mouseSpeed = 2;
	public float			jumpHeight = 10;
	public float			gravity = Physics.gravity.y;
	public float			speedSmoothTime = 0.1f;
	[Range(0,1)]
	public float			airControlPercent = .1f;

	CharacterController		cc;
	float					headRotation;
	float					velocityY;
	float					currentSpeed;
	float					speedSmoothVelocity;
	bool					canFire = true;

	// Use this for initialization
	void Start () {
        if (!isLocalPlayer)
		{
			tag = "Enemy";
            return;
		}
			
		cc = GetComponent< CharacterController >();
		Cursor.lockState = CursorLockMode.Locked;
		GameObject cam = GameObject.Instantiate(cameraObject);
		cam.transform.parent = head;
		cam.transform.localPosition = new Vector3(0, 0.1590174f, -0.663f);
		cam.transform.localRotation = Quaternion.identity;
		cam.transform.localScale = Vector3.one;
	}

	float GetModifiedSmoothTime(float smoothTime) {
		if (cc.isGrounded) {
			return smoothTime;
		}

		if (airControlPercent == 0) {
			return float.MaxValue;
		}
		return smoothTime / airControlPercent;
	}


	void Move(Vector3 input, bool running)
	{
		//player and head rotations:
		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSpeed);
		headRotation += Input.GetAxis("Mouse Y") * mouseSpeed;
		headRotation = Mathf.Clamp(headRotation, -75, 90);
		head.eulerAngles = new Vector3(-headRotation, head.eulerAngles.y, head.eulerAngles.z);

		//player Movement:
		float targetSpeed = ((running) ? runSpeed : walkSpeed) * input.magnitude;
		currentSpeed = Mathf.SmoothDamp (currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

		velocityY += Time.deltaTime * gravity;
		Vector3 velocity = transform.TransformDirection(input.normalized) * currentSpeed + Vector3.up * velocityY;

		cc.Move (velocity * Time.deltaTime);
		currentSpeed = new Vector2 (cc.velocity.x, cc.velocity.z).magnitude;

		if (cc.isGrounded) {
			velocityY = 0;
		}
	}

	void Jump()
	{
		if (cc.isGrounded)
		{
			float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);
			velocityY = jumpVelocity;
		}
	}

	IEnumerator	resetCanFire()
	{
		yield return new WaitForSeconds(.500f);
		canFire = true;
	}

	[Command]
	void CmdFire()
	{
		GameObject g = Instantiate(bulletPrefab, bulletShooter.position, bulletShooter.rotation) as GameObject;
		g.GetComponent< Rigidbody >().velocity = bulletShooter.forward * 30;
		Destroy(g, 10);
		NetworkServer.Spawn(g);
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

		Move(input, running);

		if (Input.GetKeyDown(KeyCode.Space))
			Jump();

		if (Input.GetMouseButton(0) && canFire)
		{
			canFire = false;
			CmdFire();
			StartCoroutine(resetCanFire());
		}

		if (Input.GetKeyDown(KeyCode.Escape))
			Cursor.lockState = CursorLockMode.Confined;
	}
}
