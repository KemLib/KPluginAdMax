using KTool.Attribute;
using KTool.Init;
using System.Collections;
using UnityEngine;

namespace KPlugin.AppLovinMax
{
    public class AppLovinMaxManager : MonoBehaviour, IIniter
    {
        #region Properties
        public const string MAX_SCOURCE = "AppLovinMax",
            MAX_CURRENCY = "USD";

        public static AppLovinMaxManager Instance
        {
            get;
            private set;
        }
        public static bool IsInit
        {
            get;
            private set;
        }
        public static string CountryCode
        {
            get;
            private set;
        }
        public static bool AdMute
        {
            get => MaxSdk.IsMuted();
            set => MaxSdk.SetMuted(value);
        }
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxAppOpen[] adAppOpens;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxBanner[] adBanners;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxInterstitial[] adInterstitials;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxRewarded[] adRewardeds;

        private InitTrackingSource initTrackingSource;

        private bool IsIniting => initTrackingSource != null;
        #endregion

        #region Unity Event

        #endregion

        #region Init
        public InitTracking InitBegin()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return Max_Init();
            }
            //
            return InitTracking.Success;
        }

        public void InitEnd()
        {

        }
        #endregion

        #region Methods
        public void ShowDebugger()
        {
#if UNITY_ANDROID || UNITY_IOS
            MaxSdk.ShowMediationDebugger();
#endif
        }
        #endregion

        #region Max
        private InitTracking Max_Init()
        {
            if (IsInit || IsIniting)
                return InitTracking.Success;
            //
            initTrackingSource = new InitTrackingSource(true);
            if (!string.IsNullOrEmpty(AppLovinMaxSetting.Instance.UserId))
                MaxSdk.SetUserId(AppLovinMaxSetting.Instance.UserId);
            MaxSdkCallbacks.OnSdkInitializedEvent += Max_OnSdkInitializedEvent;
            MaxSdk.InitializeSdk();
            return initTrackingSource;
        }
        private void Max_OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            if (MaxSdk.IsInitialized())
                StartCoroutine(IE_CompleteInit());
            else
                StartCoroutine(IE_MaxInit());
        }
        private IEnumerator IE_MaxInit()
        {
            yield return new WaitForEndOfFrame();
            MaxSdk.InitializeSdk();
        }
        private IEnumerator IE_CompleteInit()
        {
            yield return new WaitForSeconds(2);
            //
            IsInit = true;
            initTrackingSource.CompleteSuccess();
            initTrackingSource = null;
            CountryCode = MaxSdk.GetSdkConfiguration().CountryCode;
        }
        #endregion

        #region AppOpen
        public int AppOpen_Count()
        {
            return adAppOpens.Length;
        }
        public AppLovinMaxAppOpen AppOpen_Get(int index)
        {
            if (index < 0 || index >= adAppOpens.Length)
                return null;
            return adAppOpens[index];
        }
        public AppLovinMaxAppOpen AppOpen_Get(string adName)
        {
            foreach (var ad in adAppOpens)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Banner
        public int Banner_Count()
        {
            return adBanners.Length;
        }
        public AppLovinMaxBanner Banner_Get(int index)
        {
            if (index < 0 || index >= adBanners.Length)
                return null;
            return adBanners[index];
        }
        public AppLovinMaxBanner Banner_Get(string adName)
        {
            foreach (var ad in adBanners)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Interstitial
        public int Interstitial_Count()
        {
            return adInterstitials.Length;
        }
        public AppLovinMaxInterstitial Interstitial_Get(int index)
        {
            if (index < 0 || index >= adInterstitials.Length)
                return null;
            return adInterstitials[index];
        }
        public AppLovinMaxInterstitial Interstitial_Get(string adName)
        {
            foreach (var ad in adInterstitials)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion

        #region Rewarded
        public int Rewarded_Count()
        {
            return adRewardeds.Length;
        }
        public AppLovinMaxRewarded Rewarded_Get(int index)
        {
            if (index < 0 || index >= adRewardeds.Length)
                return null;
            return adRewardeds[index];
        }
        public AppLovinMaxRewarded Rewarded_Get(string adName)
        {
            foreach (var ad in adRewardeds)
                if (ad.Name == adName)
                    return ad;
            return null;
        }
        #endregion
    }
}
