using UnityEngine;

namespace KPlugin.AppLovinMax
{
    public class AppLovinMaxSetting : ScriptableObject
    {
        #region Properties
        public const string RESOURCES_PATH_FOLDER = "KPlugin/AppLovinMax",
            RESOURCES_PATH_FILE = "AppLovinMaxSetting";
        private const string RESOURCES_PATH = RESOURCES_PATH_FOLDER + "/" + RESOURCES_PATH_FILE;

        private static AppLovinMaxSetting instance;
        public static AppLovinMaxSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<AppLovinMaxSetting>(RESOURCES_PATH);
                return instance;
            }
        }

        [SerializeField]
        private string userId;
        [SerializeField]
        private AppLovinMaxSettingAdId[] appOpenIds,
            bannerIds,
            mrecIds,
            interstitialIds,
            rewardedIds;

        public string UserId => userId;
        #endregion

        #region Unity Event

        #endregion

        #region Method

        #endregion

        #region Ad
        public int Ad_Count(AppLovinMaxAdType adType)
        {
            switch (adType)
            {
                case AppLovinMaxAdType.AppOpen:
                    return appOpenIds.Length;
                case AppLovinMaxAdType.Banner:
                    return bannerIds.Length;
                case AppLovinMaxAdType.Mrec:
                    return mrecIds.Length;
                case AppLovinMaxAdType.Interstitial:
                    return interstitialIds.Length;
                case AppLovinMaxAdType.Rewarded:
                    return rewardedIds.Length;
                default:
                    return 0;
            }
        }
        public AppLovinMaxSettingAdId Ad_Get(AppLovinMaxAdType adType, int index)
        {
            switch (adType)
            {
                case AppLovinMaxAdType.AppOpen:
                    return appOpenIds[index];
                case AppLovinMaxAdType.Banner:
                    return bannerIds[index];
                case AppLovinMaxAdType.Mrec:
                    return mrecIds[index];
                case AppLovinMaxAdType.Interstitial:
                    return interstitialIds[index];
                case AppLovinMaxAdType.Rewarded:
                    return rewardedIds[index];
                default:
                    return null;
            }
        }
        #endregion
    }
}
