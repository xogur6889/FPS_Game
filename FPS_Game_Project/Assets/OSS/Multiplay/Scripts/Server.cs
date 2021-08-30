using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;

public class Server : MonoBehaviour
{
    private void Awake()
    {
        var obj = FindObjectsOfType<Server>();

        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Backend.InitializeAsync(true, callback =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log("Success initialize server");
            }
            else
            {
                Debug.Log("failed initialize server");
            }
        });
    }

    public bool SignUp(in string id, in string password)
    {
        BackendReturnObject returnObject = Backend.BMember.CustomSignUp(id, password);
        if (returnObject.IsSuccess())
        {
            Debug.Log("회원가입 성공 ID : " + id);
            return true;
        }
        else
        {
            int statusCode = int.Parse(returnObject.GetStatusCode());
            Debug.Log("회원가입 실패: (" + statusCode + " error code :" + returnObject.GetErrorCode() + " " + returnObject.GetMessage());
            return false;
        }
    }

    public bool SignIn(in string id, in string password)
    {
        BackendReturnObject returnObject = Backend.BMember.CustomLogin(id, password);
        if (returnObject.IsSuccess())
        {
            Debug.Log("로그인 성공");
            return true;
        }
        else
        {
            int statusCode = int.Parse(returnObject.GetStatusCode());
            Debug.Log("회원가입 실패: (" + statusCode + " error code :" + returnObject.GetErrorCode() + " " + returnObject.GetMessage());
            return false;
        }
    }
}
