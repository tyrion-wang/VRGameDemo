using UnityEngine;
using System.Collections;
using LitJson;
using UnityEngine.UI;

public class Callback : MonoBehaviour{


    /// <summary>
    /// 登陆后本地缓存一份token，用于查询
    /// </summary>
    /// <param name="LoginInfo"></param>
    public void LoginCallback(string LoginInfo) {
        JsonData jsrr = JsonMapper.ToObject(LoginInfo);
        
        if (jsrr["access_token"] != null ) {
            CommonDic.getInstance().access_token = jsrr["access_token"].ToString();  
        }
        if( jsrr["open_id"] != null){
            CommonDic.getInstance().open_id = jsrr["open_id"].ToString();
        }
        if(  jsrr["refresh_token"] != null ){
            CommonDic.getInstance().refresh_token = jsrr["refresh_token"].ToString();
        }

        if (jsrr["expires_in"] != null)
        {
            CommonDic.getInstance().expires_in = jsrr["expires_in"].ToString();
        }
        SetMassage(LoginInfo);
        Debug.Log("调用login回调:" + LoginInfo);
    }
    /// <summary>
    /// 接收支付或者查询订单操作的返回结果，根据提示码确定当前状态及订单信息
    /// </summary>
    /// <param name="payInfo"></param>
    public void QueryOrPayCallback(string queryOrPayInfo){
        JsonData jsrr = JsonMapper.ToObject(queryOrPayInfo);
        if (jsrr["code"] != null) {
            CommonDic.getInstance().code = jsrr["code"].ToString();
        }
        if (jsrr["msg"] != null)
        {
            CommonDic.getInstance().msg = jsrr["msg"].ToString();
        }
        if (jsrr != null) {
            CommonDic.getInstance().order_info = jsrr[1].ToString();
        }

        SetMassage(queryOrPayInfo);
        Debug.Log("调用pay回调:" + queryOrPayInfo);
    }

    public void UserInfoCallback(string userInfo) {
        
        CommonDic.getInstance().user_info = userInfo;

        SetMassage(userInfo);
        Debug.Log("调用userInfo回调:" + userInfo);
    }

    public void SetMassage(string massage) {
        GetCurrentGameObject().GetComponent<Text>().text = massage;
    }

    public GameObject GetCurrentGameObject() {
        return GameObject.Find("MassageInfo");
    }
}
