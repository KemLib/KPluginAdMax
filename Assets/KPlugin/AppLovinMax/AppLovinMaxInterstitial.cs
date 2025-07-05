using KTool.Advertisement;
using KTool.Init;
using KTool;
using System.Collections;
using UnityEngine;

namespace KPlugin.AppLovinMax
{
    public class AppLovinMaxInterstitial : AdInterstitial, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Interstitial load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad Interstitial display fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad Interstitial show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad Interstitial show fail: ad is show";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AppLovinMaxAdType.Interstitial)]
        private int indexAd = 0;

        private bool isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;
        private AdInterstitialTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AppLovinMaxSettingAdId settingAdId = AppLovinMaxSetting.Instance.Ad_Get(AppLovinMaxAdType.Interstitial, indexAd);
                if (settingAdId != null)
                    return settingAdId.AdID;
                return string.Empty;
            }
        }
        public override bool IsAutoReload
        {
            get => base.IsAutoReload;
            protected set
            {
                if (value == base.IsAutoReload)
                    return;
                base.IsAutoReload = value;
                if (base.IsAutoReload)
                    Load();
            }
        }
        public override bool IsReady => base.IsReady && MaxSdk.IsInterstitialReady(AdId);
        #endregion

        #region Methods Unity
        private void OnDestroy()
        {
            if (instance != null && instance.GetInstanceID() == GetInstanceID())
                instance = null;
            Destroy();
        }
        #endregion

        #region Init
        public InitTracking InitBegin()
        {
            initTrackingSource = new InitTrackingSource(initIndispensable);
            Load();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        #endregion

        #region Methods

        public override void Init()
        {
            if (IsInited)
                return;
            //
            if (setInstance)
                instance = this;
            IsInited = true;
            Ad_EventRegister();
            PushEvent_Inited();
        }
        public override void Load()
        {
            Init();
            //
            if (IsLoaded)
                return;
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (!IsShow)
            {
                Ad_EventUnRegister();
                PushEvent_Destroy();
            }
        }
        public override AdInterstitialTracking Show()
        {
            if (IsShow)
                return new AdInterstitialTrackingSource(ERROR_SHOW_FAIL_AD_IS_SHOWED);
            if (!IsReady)
                return new AdInterstitialTrackingSource(ERROR_SHOW_FAIL_AD_NOT_READY);
            //
            adTrackingSource = new AdInterstitialTrackingSource(this);
            IsShow = true;
            MaxSdk.ShowInterstitial(AdId);
            return adTrackingSource;
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (isLoading)
                return;
            isLoading = true;
            //
            CoroutineManager.Instance.Coroutine_Start(Ad_LoadAd());
        }
        private IEnumerator Ad_LoadAd()
        {
            if (attemptLoad > 0)
            {
                float delay = Mathf.Pow(2, attemptLoad);
                yield return new WaitForSecondsRealtime(delay);
            }
            //
            while (!AppLovinMaxManager.IsInit)
                yield return new WaitForEndOfFrame();
            //
            MaxSdk.LoadInterstitial(AdId);
        }
        private void Ad_EventRegister()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdReviewCreativeIdGeneratedEvent += Ad_OnAdReviewCreativeIdGeneratedEvent;
            MaxSdkCallbacks.Interstitial.OnExpiredAdReloadedEvent += Ad_OnExpiredAdReloadedEvent;
        }
        private void Ad_EventUnRegister()
        {
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Interstitial.OnAdReviewCreativeIdGeneratedEvent -= Ad_OnAdReviewCreativeIdGeneratedEvent;
            MaxSdkCallbacks.Interstitial.OnExpiredAdReloadedEvent -= Ad_OnExpiredAdReloadedEvent;
        }
        private void Ad_OnAdLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            isLoading = false;
            //
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteSuccess();
                initTrackingSource = null;
            }
            //
            attemptLoad = 0;
            IsLoaded = true;
            PushEvent_Loaded(true);
        }
        private void Ad_OnAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adId != AdId)
                return;
            isLoading = false;
            //
            if (errorInfo != null)
                Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
            //
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            PushEvent_Loaded(false);
            if (!IsDestroy && IsAutoReload)
                Ad_Create();
            return;
        }
        private void Ad_OnAdDisplayedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Displayed(true);
            adTrackingSource.Displayed(true);
        }
        private void Ad_OnAdDisplayFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            IsShow = false;
            //
            if (errorInfo != null)
                Debug.LogWarning(string.Format(ERROR_DISPLAY_FAIL, errorInfo.Code));
            PushEvent_Displayed(false);
            adTrackingSource.Displayed(false);
            //
            if (IsDestroy)
            {
                Ad_EventUnRegister();
                PushEvent_Destroy();
            }
            else if (IsAutoReload)
                Ad_Create();
        }
        private void Ad_OnAdClickedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Clicked();
            adTrackingSource.Clicked();
        }
        private void Ad_OnAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            IsShow = false;
            //
            PushEvent_Hidden();
            adTrackingSource.Hidden();
            //
            if (IsDestroy)
            {
                Ad_EventUnRegister();
                PushEvent_Destroy();
            }
            else if (IsAutoReload)
                Ad_Create();
        }
        private void Ad_OnAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            if (adInfo == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                AppLovinMaxManager.MAX_SCOURCE,
                adInfo.NetworkName,
                AdId,
                AppLovinMaxManager.CountryCode,
                AdType,
                adInfo.Revenue,
                AppLovinMaxManager.MAX_CURRENCY);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource.RevenuePaid(revenuePaid);
        }
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
        }
        private void Ad_OnExpiredAdReloadedEvent(string adId, MaxSdkBase.AdInfo adInfo1, MaxSdkBase.AdInfo adInfo2)
        {
            if (adId != AdId)
                return;
        }
        #endregion
    }
}
