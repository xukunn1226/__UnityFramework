using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System.Diagnostics;

namespace Application.Runtime
{
    public class WorldGameInfo : GameInfo
    {
        public override GameState Id { get { return GameState.World; } }

        public override void OnEnter(IState<GameState> prevState)
        {
            base.OnEnter(prevState);
            playerController.enabled = true;
        }

        public override void OnLeave(IState<GameState> nextState)
        {
            playerController.enabled = false;
            base.OnLeave(nextState);
        }

        public override void OnUpdate(float deltaTime)
        {
            InputForDebug();
        }

        private void OnGUI()
        {
            if(GUI.Button(new Rect(Screen.width-200, Screen.height/2, 200, 100), "Create Actors"))
            {
                for(int i = 0; i < 10; ++i)
                    TestActorManager.CreateActor();
            }

            if(GUI.Button(new Rect(Screen.width-200, Screen.height/2 - 120, 200, 100), "Destroy Actors"))
            {
                TestActorManager.DestroyAll();                
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void InputForDebug()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                ((WorldPlayerController)playerController).PanCamera(Vector3.zero);
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.Debug.Log($"print center hit point: {((WorldPlayerController)playerController).GetCenterHitPoint()}");
            }            
        }
    }
}