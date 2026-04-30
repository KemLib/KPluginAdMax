using UnityEngine;

namespace KPlugin.AdMax
{
    public class AdMaxSetting : ScriptableObject
    {
        #region Properties
        public const string RESOURCES_PATH_FOLDER = "KPlugin/AdMax",
            RESOURCES_PATH_FILE = "AdMaxSetting";
        private const string RESOURCES_PATH = RESOURCES_PATH_FOLDER + "/" + RESOURCES_PATH_FILE;

        private static AdMaxSetting instance;
        public static AdMaxSetting Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<AdMaxSetting>(RESOURCES_PATH);
                return instance;
            }
        }
        [SerializeField]
        private AdMaxSettingAdId[] appOpenIds,
            bannerIds,
            mrecIds,
            interstitialIds,
            rewardedIds;
        #endregion

        #region Unity Event

        #endregion

        #region Method

        #endregion

        #region Ad
        public int Ad_Count(AdMaxAdType adType)
        {
            switch (adType)
            {
                case AdMaxAdType.AppOpen:
                    return appOpenIds.Length;
                case AdMaxAdType.Banner:
                    return bannerIds.Length;
                case AdMaxAdType.MRec:
                    return mrecIds.Length;
                case AdMaxAdType.Interstitial:
                    return interstitialIds.Length;
                case AdMaxAdType.Rewarded:
                    return rewardedIds.Length;
                default:
                    return 0;
            }
        }
        public AdMaxSettingAdId Ad_Get(AdMaxAdType adType, int index)
        {
            switch (adType)
            {
                case AdMaxAdType.AppOpen:
                    return appOpenIds[index];
                case AdMaxAdType.Banner:
                    return bannerIds[index];
                case AdMaxAdType.MRec:
                    return mrecIds[index];
                case AdMaxAdType.Interstitial:
                    return interstitialIds[index];
                case AdMaxAdType.Rewarded:
                    return rewardedIds[index];
                default:
                    return null;
            }
        }
        #endregion
    }
}
