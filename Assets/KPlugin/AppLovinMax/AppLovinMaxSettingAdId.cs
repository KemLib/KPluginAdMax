using UnityEngine;

namespace KPlugin.AppLovinMax
{
    [System.Serializable]
    public class AppLovinMaxSettingAdId
    {
        #region Properties
        [SerializeField]
        private string androidId,
            iosId;

        public string AdID
        {
            get
            {
#if UNITY_ANDROID
                return androidId;
#elif UNITY_IOS
                return iosId;
#else
                return string.Empty;
#endif
            }
        }
        #endregion

        #region Method

        #endregion
    }
}
