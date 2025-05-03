using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayUI : MonoBehaviour
{
    public void OnExitClicked()
    {
        this.gameObject.SetActive(false);
    }
}
