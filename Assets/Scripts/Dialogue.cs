using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string title;

    [TextArea(5,15)]
    public string[] sentences;
}
