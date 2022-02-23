using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Application.Logic
{
    public class UIPanel_MessageBox : UIPanelBase
    {
        public class MessageInfo
        {
            public string msg;
            public string confirmMsg;
            public string cancelMsg;
            public System.Action confirmCallback;
            public System.Action cancelCallback;
        }

        private Button btn_Close;
        private Button btn_Confirm;
        private Button btn_Cancel;
        private Text m_Message;
        private Text m_ConfirmMessage;
        private Text m_CancelMessage;
        private MessageInfo m_UserData;

        public UIPanel_MessageBox(UIDefines_MessageBox defines) : base(defines) {}

        public override void OnCreate(GameObject go)
        {
            btn_Close = go.transform.Find("Window/Button_Close").GetComponent<Button>();
            btn_Confirm = go.transform.Find("Window/Button_Confirm").GetComponent<Button>();
            btn_Cancel = go.transform.Find("Window/Button_Cancel").GetComponent<Button>();
            m_Message = go.transform.Find("Window/Text").GetComponent<Text>();
            m_ConfirmMessage = go.transform.Find("Window/Button_Confirm/Text").GetComponent<Text>();
            m_CancelMessage = go.transform.Find("Window/Button_Cancel/Text").GetComponent<Text>();

            btn_Close.onClick.AddListener(() => { this.Close(); });
        }

        public override void OnShow(object userData = null)
        {
            m_UserData = (MessageInfo)userData;
            if(m_UserData == null)
                return;
                
            SetMessageBox(m_UserData.msg, m_UserData.confirmMsg, m_UserData.cancelMsg, m_UserData.confirmCallback, m_UserData.cancelCallback);
        }

        private void SetMessageBox(string msg, string confirmMsg, string cancelMsg, System.Action confirmCallback, System.Action cancelCallback)
        {
            m_Message.text = msg;
            m_ConfirmMessage.text = confirmMsg;
            m_CancelMessage.text = cancelMsg;
            btn_Confirm.onClick.AddListener(() => confirmCallback?.Invoke());
            btn_Cancel.onClick.AddListener(() => cancelCallback?.Invoke());
        }
    }
}