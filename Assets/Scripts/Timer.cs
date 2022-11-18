using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public int seconds;
    public Text timerText;
    public bool isTimeOut;
    public bool isCounting;

    public void StartCountDown()
    {
        seconds = 5;
        timerText.text = "5";
        isTimeOut = false;
        isCounting = true;
        StartCoroutine(Countdown());
    }

    public void EndCountDown()
    {
        timerText.text = "";
        isTimeOut=false;
        isCounting=false; 
        StopCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        while (seconds > 0)
        {
            yield return new WaitForSeconds(1);
            seconds--;
            timerText.text = seconds.ToString();
        }
        isTimeOut = true;
    }
}
