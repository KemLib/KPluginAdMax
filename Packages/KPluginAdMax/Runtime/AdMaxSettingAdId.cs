using UnityEngine;

namespace KPlugin.AdMax
{
    [System.Serializable]
    public class AdMaxSettingAdId
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
