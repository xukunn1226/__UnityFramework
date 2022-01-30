using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace StarterAssets
{
    public class CharacterLogic
    {
        static public GameObject Create()
        {
            const string assetPath = "assets/starterassets/thirdpersoncontroller/prefabs/playerarmatureex.prefab";
            return AssetManager.InstantiatePrefab(assetPath);
        }

        private GameObject m_Character;
        private CharacterView m_CharacterView;
        private StarterAssetsInputs m_Input;

        public void Init()
        {
            m_Character = CharacterLogic.Create();
            m_CharacterView = m_Character.GetComponent<CharacterView>();
            m_Input = m_Character.GetComponent<StarterAssetsInputs>();
            m_Input.onMove += OnMove;
            m_Input.onLook += OnLook;
            m_Input.onJump += OnJump;
            m_Input.onSprint += OnSprint;
        }

        public void Uninit()
        {
            Object.Destroy(m_Character);
            m_Input.onMove -= OnMove;
            m_Input.onLook -= OnLook;
            m_Input.onJump -= OnJump;
            m_Input.onSprint -= OnSprint;
        }

        private void OnMove(Vector2 move)
        {

        }

        private void OnLook(Vector2 look)
        {

        }

        private void OnJump(bool jump)
        {

        }

        private void OnSprint(bool sprint)
        {

        }
    }
}