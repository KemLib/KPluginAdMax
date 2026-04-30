using KTool.Attribute;
using KTool.Cron;
using KTool.Init;
using UnityEngine;
using static MaxSdkBase;

namespace KPlugin.AdMax
{
    public class AdMaxManager : MonoBehaviour, IIniter
    {
        #region Properties
        public const string MAX_SCOURCE = "AppLovinMax",
            MAX_CURRENCY = "USD";

        public static AdMaxManager Instance
        {
            get;
            private set;
        }
        public static string CountryCode
        {
            get;
            private set;
        }

        [SerializeField]
        private bool initIndispensable;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMaxAppOpen[] adAppOpens;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMaxBanner[] adBanners;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMaxMRec[] adMRecs;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMaxInterstitial[] adInterstitials;
        [SerializeField, GetComponent(GetComponentType.InGameObject_AllChildren, true)]
        private AdMaxRewarded[] adRewardeds;

        private bool isInit,
            isIniting;
        private InitTrackingSource initTrackingSource;

        public bool Mute
        {
            get => isInit && MaxSdk.IsMuted();
            set
            {
                if (!isInit)
                    return;
                MaxSdk.SetMuted(value);
            }
        }
        #endregion

        #region Unity Event
        private void OnDestroy()
        {
            if (Instance != null && Instance.GetInstanceID() == GetInstanceID())
                Instance = null;
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
        public static bool IsReady()
        {
            return Instance != null && Instance.isInit;
        }
        public void ShowMediationDebugger()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            MaxSdk.ShowMediationDebugger();
#endif
        }
        #endregion

        #region Max Init
        private IInitTracking Max_Init()
        {
            if (isInit || isIniting)
                return IInitTracking.Success;
            isIniting = true;
            //
            initTrackingSource = new InitTrackingSource(initIndispensable);
            MaxSdkCallbacks.OnSdkInitializedEvent += Max_OnSdkInitializedEvent;
            MaxSdk.InitializeSdk();
            return initTrackingSource;
        }
        private void Max_OnSdkInitializedEvent(SdkConfiguration sdkConfiguration)
        {
            if (MaxSdk.IsInitialized())
            {
                isIniting = false;
                isInit = true;
                CountryCode = MaxSdk.GetSdkConfiguration().CountryCode;
                //
                initTrackingSource.CompleteSuccess();
                initTrackingSource = null;
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionFrame.Create(1))
                    .Add(CallbackAction.Create(Max_Init_Retry))
                    .Run();
            }
        }
        private void Max_Init_Retry()
        {
            MaxSdk.InitializeSdk();
        }
        #endregion

        #region Ad
        #region AppOpen
        public int AppOpen_Count()
        {
            return adAppOpens.Length;
        }
        public AdMaxAppOpen AppOpen_Get(int index)
        {
            if (index < 0 || index >= adAppOpens.Length)
                return null;
            return adAppOpens[index];
        }
        public AdMaxAppOpen AppOpen_Get(string adName)
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
        public AdMaxBanner Banner_Get(int index)
        {
            if (index < 0 || index >= adBanners.Length)
                return null;
            return adBanners[index];
        }
        public AdMaxBanner Banner_Get(string adName)
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
        public AdMaxMRec MRec_Get(int index)
        {
            if (index < 0 || index >= adMRecs.Length)
                return null;
            return adMRecs[index];
        }
        public AdMaxMRec MRec_Get(string adName)
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
        public AdMaxInterstitial Interstitial_Get(int index)
        {
            if (index < 0 || index >= adInterstitials.Length)
                return null;
            return adInterstitials[index];
        }
        public AdMaxInterstitial Interstitial_Get(string adName)
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
        public AdMaxRewarded Rewarded_Get(int index)
        {
            if (index < 0 || index >= adRewardeds.Length)
                return null;
            return adRewardeds[index];
        }
        public AdMaxRewarded Rewarded_Get(string adName)
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
