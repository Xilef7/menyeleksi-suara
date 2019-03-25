using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TitleUI : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Text>().text = Application.productName;
    }
}
