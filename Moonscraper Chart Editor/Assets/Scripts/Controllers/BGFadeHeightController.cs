﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class BGFadeHeightController : MonoBehaviour {
    [SerializeField]
    Transform objectMaxPosition;
    [SerializeField]
    float offset;
    Renderer ren;

	// Use this for initialization
	void Start () {
        ren = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	public void AdjustHeight () {
        Vector3 worldFadePos = objectMaxPosition.position;
        worldFadePos.y -= offset;
        float screenHeight = Camera.main.WorldToScreenPoint(worldFadePos).y;

        ren.sharedMaterial.SetFloat("_HeightPosition", screenHeight / Screen.height);
	}
}
