﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropshadowSizing : MonoBehaviour {
    [SerializeField]
    CustomUnityDropdown dropdown;
    [SerializeField]
    RectTransform item;
    [SerializeField]
    Vector2 offset = new Vector2(5, -7);
    RectTransform rectTransform;

    readonly Vector2 TARGET_ASPECT_RATIO = new Vector2(16, 9);

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 size = rectTransform.sizeDelta;
        size.x = item.rect.width;
        size.y = item.rect.height * dropdown.options.Count;
        rectTransform.sizeDelta = size;

        Vector2 position = item.position;
        Vector2 offset = this.offset;// + new Vector2(size.x, size.y) / 100.0f;
        offset *= ((float)Screen.width / Screen.height) / (TARGET_ASPECT_RATIO.x / TARGET_ASPECT_RATIO.y);
        position += offset;
        rectTransform.position = position;   
    }
}
