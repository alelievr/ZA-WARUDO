﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	void OnCollisionEnter(Collision c)
	{
		enabled = false;
		Destroy(gameObject);
	}

}
