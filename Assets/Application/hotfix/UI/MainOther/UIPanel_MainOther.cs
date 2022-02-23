using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Application.Logic
{
    public class UIPanel_MainOther : UIPanelBase
    {
        private Button btn_Shop;
        private Button btn_Settings;

        public UIPanel_MainOther(UIDefines_MainOther defines) : base(defines) {}

        public override void OnInit() { UIManager.Instance.RegisterUpdateEvent(defines.id); }
        public override void OnCreate(GameObject go)
        {
            btn_Shop = go.transform.Find("Button_Shop").GetComponent<Button>();
            btn_Shop.onClick.AddListener(delegate ()
            {
                Debug.Log("click button");
                UIManager.Instance.Open(UIPanelID.Shop);
            });

            btn_Settings = go.transform.Find("Button_Settings").GetComponent<Button>();
            btn_Settings.onClick.AddListener(delegate ()
            {
                UIManager.Instance.Open(UIPanelID.Setting);
            });
        }
    }
}