using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FirebaseController : MonoBehaviour
{
   public GameObject loginPanel,signupPanel;
   public TMP_InputField loginEmail,loginPassword,signUpEmail,signUpPassword,signUpCPassword,signUpUserName;
   public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
    }
    public void OpenSignupPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
    }
    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) && string.IsNullOrEmpty(loginPassword.text))
        {
            return;
        }
        //dosignup
    }
    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signUpEmail.text) && string.IsNullOrEmpty(signUpPassword.text) && string.IsNullOrEmpty(signUpCPassword.text) && string.IsNullOrEmpty(signUpUserName.text))
        {
            return;
        }
    }//do signup
}
