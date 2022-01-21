using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using SimpleJSON;

public class UIManager : MonoBehaviour
{
    private bool _isStarted;

	public Text walletAmount_Text;
	public Text CoinsDeltaText; 		// Pop-up text with wasted or rewarded coins amount
	public Text InputCoinValue ;        // public InputField InputCoinValue;
	public TMP_Text Errortext;


	private float[] _sectorsAngles;
    private float _finalAngle;
    private float _startAngle = 0;
    private float _currentLerpRotationTime;
	private int randomFinalAngle;

	public GameObject IncreaseButton;
	public GameObject ReduceButton;
	public GameObject Circle; 
	public Button TurnButton;			// Rotatable Object with rewards

	BetPlayer _player;

	public SocketIOController io;

    public float totalAmount;		// For wasted coins animation
	public float TurnCost;
	public float EarnAmount;

	// GameReadyStatus Send
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);	
	void Start ()
	{
		InputCoinValue.text = "10";
		TurnCost = float.Parse(InputCoinValue.text);
		_sectorsAngles = new float[] { 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330, 360 };

		io.Connect();
		io.On("connect" , (e) =>
		{
			Debug.Log("Game started");
			io.On("bet result", (res) =>
			{
				StartCoroutine(BetResult(res));
			});
			io.On("error massage", (res) =>
			 {
				 BetError(res);
			 });
		});

		#if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif
	}


	void Update ()
	{
		// Make turn button non interactable if user has not enough money for the turn
		if (_isStarted) {
			TurnButton.interactable = false;
			TurnButton.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
		} else {
			TurnButton.interactable = true;
			TurnButton.GetComponent<Image>().color = new Color(255, 255, 255, 1);
		}

		if (!_isStarted)
			return;

		float maxLerpRotationTime = 10f;
	
		// increment timer once per frame
		_currentLerpRotationTime += Time.deltaTime;
		if (_currentLerpRotationTime > maxLerpRotationTime || Circle.transform.eulerAngles.z == _finalAngle) {
			_currentLerpRotationTime = maxLerpRotationTime;
			_isStarted = false;
			_startAngle = _finalAngle % 360;

		}
	
		// Calculate current position using linear interpolation
		float t = _currentLerpRotationTime / maxLerpRotationTime;
	
		// This formulae allows to speed up at start and speed down at the end of rotation.
		// Try to change this values to customize the speed
		t = t * t * t * (t * (6f * t - 15f) + 10f);
	
		float angle = Mathf.Lerp (_startAngle, _finalAngle, t);
		Circle.transform.eulerAngles = new Vector3 (0, 0, angle);
	}

	public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        Debug.Log("token=--------" + usersInfo["token"]);
        Debug.Log("amount=------------" + usersInfo["amount"]);
        Debug.Log("userName=------------" + usersInfo["userName"]);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];

        float i_balance = float.Parse(usersInfo["amount"]);
        walletAmount_Text.text = (i_balance).ToString();
		totalAmount = float.Parse(walletAmount_Text.text);
    }

	public void InputCoinValue_Changed()
    {

		if (TurnCost <= 10.0)
		{
            InputCoinValue.text = "10";
		}
        else if (TurnCost >= 100.0)
        {
            InputCoinValue.text = "100";
        }
    }


	public void Reducebtn_Clicked()
	{

			if(TurnCost > 10.0)
		{
		TurnCost -= (float)10.0;
		InputCoinValue.text = (TurnCost).ToString();
			Errortext.text = "";
		}

	}


	public void Increasebtn_Clicked()
	{

		if(TurnCost < 200.0)
		{
		TurnCost += (float)10.0;
			InputCoinValue.text = (TurnCost).ToString();
			Errortext.text = "";
		}

	}


	public void SpinBtn_Clicked()
	{
		if (totalAmount >= TurnCost)
		{
			Jsontype JObject = new Jsontype();
			JObject.betAmount = TurnCost;
            JObject.token = _player.token;
            JObject.userName = _player.username;
            io.Emit("bet info", JsonUtility.ToJson(JObject));
		}
		else
        {
			Errortext.text = "TotalAmount is small!";
        }

	}
	IEnumerator BetResult(SocketIOEvent socketIOEvent)
    {
		_isStarted = true;
		totalAmount -= TurnCost;
		walletAmount_Text.text = (totalAmount).ToString();
		var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
		// Show wasted coins
		CoinsDeltaText.text = "-" + TurnCost;
		CoinsDeltaText.gameObject.SetActive (true);

		randomFinalAngle = res.randomNumber;

		int fullCircles = 10;
		_finalAngle = -(fullCircles * 360 + randomFinalAngle);


		_currentLerpRotationTime = 0f;    	
        yield return new WaitForSeconds(10f);
        _isStarted = false;
		totalAmount += res.earnAmount;
		EarnAmount = res.earnAmount;

		if(res.gameResult)
		{
		CoinsDeltaText.text = "+"+(EarnAmount).ToString();
		walletAmount_Text.text = (totalAmount).ToString();

		}
		else 
		{
		CoinsDeltaText.text = "LOSE";
		walletAmount_Text.text = (totalAmount).ToString();


		}

        Debug.Log(res.gameResult + "  " + res.randomNumber);
    }
	void BetError(SocketIOEvent socketIOEvent)
    {
		var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
		totalAmount += TurnCost;
		walletAmount_Text.text = (totalAmount).ToString();
		Errortext.text = res.errorMessage;
	}
}
public class BetPlayer
{
    public string username;
    public string token;
}