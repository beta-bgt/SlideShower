using System;
using UdonSharp;
using UnityEngine;

public class PhotoStand : UdonSharpBehaviour
{

    [SerializeField, Tooltip("横表示か縦表示か")]
    public bool isHorizontal;

    [NonSerialized]
    public Renderer renderer;

    [NonSerialized]
    public bool tex1ToTex2 = true;

    [NonSerialized]
    public int counter = 0;

    void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
    }
}