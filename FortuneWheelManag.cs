using unityEngine;
using UnityRngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnitySocketIO;
using UnitySocketIO.Events;


pubilc class FortuneWheelManage :MonoBehaviour
{
    private bool _isstarted;


    public TMP_Text info_Text;
    public TMP_Text walletAmount_TExt;

    private floaat[] _sectorsAngles;
    private float _finalAngle;
    private float _startAngle = 0;
    private float _currentLerpRotationTime;


    public GameObject IncreaseButton;
    public GameObject ReduceButton;
    public GameObject Circle;

    

    public SocketIOCoontroller io;


    public TMP_Text CoinDeltaText;
    public TMP_Text CurrentCoinsText;
    public TMP_Text InputCoinValue;


    public int TurnCost = 100;
    public int CurrentCoinAmount ;
    public int totalAmount;


     void start ()
    {
        InputCoinValue.text = "10";

        io.On("connect" , (e) =>
        {
            Debug.Log("Game started");
            io.On("spin result", (res) =>
            {
                startCoroutine(SpinResult(res));
            });
        });
        io.Connect();
    }



    void Update ()
    {
		if (_isstarted) 
        {
		
			randomTime = Random.Range (5, 10);
			itemNumber = Random.Range (2, prize.Count);
			float maxAngle = 360 * randomTime + (itemNumber * anglePerItem);
			StartCoroutine (SpinTheWheel (5 * randomTime, maxAngle));
		}
    }
    public void InputCoinValue_changed()
    {
        if(int.Parse(InputCoinValue.text) <=10)
        InputCoinValue.text = "10";
        else if (int.Parse(InputCoinValue.text) >= 100)
        {
            InputCoinValue.text = "100";
        }
    }
    public void Reducebtn_Clicked()
    {
        if(int.Parse(InputCoinValuetext) <=100)
        int.Parse(InputCoinValue.text) -= 10;
        InputCoinValue.text = "int.Parse(InputCoinValue.text)";
    
    }

    public void Increasebtn_Clicked()
    {
        if(int.Parse(InputCoinValue.text) >= 0)
        int.Parse(InputCoinValue.text) += 10;
        InputCoinValue.text = "int.Parse(InputCoinValue.text)";

    }

    	public void SpinBtn_Clicked()
	{
		info_Text.text = "";
		Jsontype JObject = new Jsontype();
		int myTotalAmount = int.Parse(string.IsNullOrEmpty(walletAmount_Text.text) ? "0" : walletAmount_Text.text);
		int betamount = int.Parse(string.IsNullOrEmpty(AmountFiled.text) ? "0" : AmountField.text);
		if(betamount<myTotalAmount)
		{
			JObject.betAmount = betamount;
			io.Emit("bet info", JsonUtility.ToJson(JObject));
		}
		else info_Text.text = "Not enough Funds";
	}


    
	    IEnumerator BetResult(SocketIOEvent socketIOEvent)
    {
        isbetting = true;
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        yield return new WaitForSeconds(1.5f);
        isbetting = false;
        walletAmount_Text.text = res.amount.ToString("");
        if (res.gameResult)
            info_Text.text = "You Win!  You earned " + res.earnAmount.ToString();
        else
            info_Text.text = "You Lose!";
        SetReelText(res.randomNumber);
        Debug.Log(res.amount + "   " + res.earnAmount + "   " + res.gameResult + "  " + res.randomNumber);
    }

	IEnumerator SpinTheWheel (float time, float maxAngle)
	{
		_isstarted = true;
		
		float timer = 0.0f;		
		float startAngle = transform.eulerAngles.z;		
		maxAngle = maxAngle - startAngle;
		
		int animationCurveNumber = Random.Range (0, animationCurves.Count);
		Debug.Log ("Animation Curve No. : " + animationCurveNumber);
		
		while (timer < time) {
		//to calculate rotation
			float angle = maxAngle * animationCurves [animationCurveNumber].Evaluate (timer / time) ;
			transform.eulerAngles = new Vector3 (0.0f, 0.0f, angle + startAngle);
			timer += Time.deltaTime;
			yield return 0;
		}
		
		transform.eulerAngles = new Vector3 (0.0f, 0.0f, maxAngle + startAngle);
		spinning = false;
			
		Debug.Log ("Prize: " + prize [itemNumber]);//use prize[itemNumnber] as per requirement
	}

}