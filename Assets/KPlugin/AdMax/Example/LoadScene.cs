using KTool.Attribute;
using KTool.Init;
using UnityEngine;

namespace KPlugin.AdMax.Example
{
    public class LoadScene : MonoBehaviour
    {
        #region Properties
        [SerializeField, SelectScene]
        private string scene;
        #endregion

        #region Methods Unity

        #endregion

        #region Methods
        public void NextScene()
        {
            InitManager.Instance.LoadScene(scene);
        }
        #endregion
    }
}
