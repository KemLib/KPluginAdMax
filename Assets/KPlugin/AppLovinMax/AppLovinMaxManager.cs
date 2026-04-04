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

        [SerializeField]
        private bool indispensable;
        [SerializeField]
        private bool showDebuggerAfterInit;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxAppOpen[] adAppOpens;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxBanner[] adBanners;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxMRec[] adMRecs;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxInterstitial[] adInterstitials;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AppLovinMaxRewarded[] adRewardeds;

        private InitTrackingSource initTrackingSource;

        private bool IsIniting => initTrackingSource != null;
        #endregion

        #region Unity Event
        private void OnDestroy()
        {
            if (Instance != null && Instance.GetInstanceID() == GetInstanceID())
            {
                Instance = null;
            }
        }
        private void OnApplicationQuit()
        {
            IsInit = false;
        }
        #endregion

        #region Init
        public IInitTracking InitBegin()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                //
                return Max_Init();
            }
            //
            return IInitTracking.Success;
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
        private IInitTracking Max_Init()
        {
            if (IsInit || IsIniting)
                return IInitTracking.Success;
            //
            initTrackingSource = new InitTrackingSource(indispensable);
            if (!string.IsNullOrEmpty(AppLovinMaxSetting.Instance.UserId))
                MaxSdk.SetUserId(AppLovinMaxSetting.Instance.UserId);
            MaxSdkCallbacks.OnSdkInitializedEvent += Max_OnSdkInitializedEvent;
            MaxSdk.InitializeSdk();
            return initTrackingSource;
        }
        private void Max_OnSdkInitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            if (MaxSdk.IsInitialized())
                StartCoroutine(IE_Max_InitComplete());
            else
                StartCoroutine(IE_Max_RetryInit());
        }
        private IEnumerator IE_Max_RetryInit()
        {
            yield return new WaitForEndOfFrame();
            MaxSdk.InitializeSdk();
        }
        private IEnumerator IE_Max_InitComplete()
        {
            yield return new WaitForEndOfFrame();
            //
            IsInit = true;
            CountryCode = MaxSdk.GetSdkConfiguration().CountryCode;
            if (showDebuggerAfterInit)
                ShowDebugger();
            //
            initTrackingSource.CompleteSuccess();
            initTrackingSource = null;
        }
        #endregion

        #region Ad
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

        #region MRec
        public int MRec_Count()
        {
            return adMRecs.Length;
        }
        public AppLovinMaxMRec MRec_Get(int index)
        {
            if (index < 0 || index >= adMRecs.Length)
                return null;
            return adMRecs[index];
        }
        public AppLovinMaxMRec MRec_Get(string adName)
        {
            foreach (var ad in adMRecs)
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
        #endregion
    }
}
