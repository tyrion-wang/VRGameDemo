#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections;
using LitJson;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_ANDROID
public class DemoController : MonoBehaviour {
    Callback callback;
    GameObject msg;
    PicoVRManager picoVrManager;
    string currentOrderID;

    


    void Start()
    {
        msg = GameObject.Find("MassageInfo");
        InitDelegate();
        callback = new Callback();

        picoVrManager = GameObject.Find("PicoVR").GetComponent<PicoVRManager>();
       
        
    }	

	void Update () {

        
        //X键校准    
        if (picoVrManager != null)  {
            if (Input.GetKeyDown(KeyCode.Y) || Input.GetButtonDown("ResetHead"))  {
                Debug.Log("update");
                picoVrManager.ResetHead();
            }
        }
        //B键退出
        if (Input.GetKeyDown(KeyCode.Joystick1Button1)) {
            Application.Quit();
        }
      
	}

    void InitDelegate(){
        //绑定事件
        ArrayList btnsName = new ArrayList();
    
        btnsName.Add("Login");
        btnsName.Add("GetUserAPI");
        btnsName.Add("PayOne");
        btnsName.Add("QueryOrder");

        foreach (string btnName in btnsName){
            GameObject btnObj = GameObject.Find(btnName);
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(delegate() { OnClick(btnObj); });
        } 
    }



    //ButtonClickedEvent
    void OnClick(GameObject btnObj) {
        switch (btnObj.name)
        {        
            case "Login": //登陆
                
                PicoPaymentSDK.Login();
                break;

            case "PayOne": //支付1P币
                CommonDic.getInstance().setParameters("subject", "game");
                CommonDic.getInstance().setParameters("body", "gamePay");
                CommonDic.getInstance().setParameters("order_id", getRamdomTestOrderID());
                CommonDic.getInstance().setParameters("total", "1");
                CommonDic.getInstance().setParameters("goods_tag", "game");
                CommonDic.getInstance().setParameters("notify_url", "www.picovr.com");
               
                
                PicoPaymentSDK.Pay(CommonDic.getInstance().PayOrderString());
                      
                break;

            case "QueryOrder": //查询订单
                PicoPaymentSDK.QueryOrder(currentOrderID);
                break;

            case "GetUserAPI": //查看用户信息
                PicoPaymentSDK.GetUserAPI();
                
                break;

        }
    }


    public string getRamdomTestOrderID(){
        currentOrderID = (Random.value * 65535).ToString();
        return currentOrderID;
    }

}
#endif
