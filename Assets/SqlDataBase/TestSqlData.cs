using UnityEngine; 

namespace SQL
{
    public class TestSqlData : MonoBehaviour
    {
        private void Awake()
        {
            
        }

        private void OnDestroy()
        {
            SqlManager.Instance.OnDestroy();
        }

        // Use this for initialization
        void Start()
        {
            Debug.Log(UnityEngine.Application.persistentDataPath);

            
            Debug.Log("Awake...");
            SqlManager.Instance.OnAwake();
            Debug.Log("Awake...2222");

            Debug.Log("--------------------Start...");

            Debug.Log("name的值为 " + CommonSaveUtil.GetString("name"));
            Debug.Log("score的值为 " + CommonSaveUtil.GetString("score"));

            //CommonSaveUtil.SetString("name", "onelei");
            //CommonSaveUtil.SetInt("score", 100);

            Debug.Log("name的值为 " + CommonSaveUtil.GetString("name"));
            Debug.Log("score的值为common    " + CommonSaveUtil.GetString("score"));

            //SaveUtil.DeleteValue("score");
            Debug.Log("score的值为 " + CommonSaveUtil.GetInt("score"));


            Debug.Log("====Player======================================");

            Debug.Log("name的值为 " + PlayerSaveUtil.GetString("name"));
            Debug.Log("score的值为 " + PlayerSaveUtil.GetString("score"));

            //PlayerSaveUtil.SetString("name", "onelei");
            //PlayerSaveUtil.SetInt("score", 99);

            Debug.Log("name的值为 " + PlayerSaveUtil.GetString("name"));
            Debug.Log("score的值为 " + PlayerSaveUtil.GetString("score"));

            //SaveUtil.DeleteValue("score");
            Debug.Log("score的值为 " + PlayerSaveUtil.GetInt("score"));
        }

    }
}
