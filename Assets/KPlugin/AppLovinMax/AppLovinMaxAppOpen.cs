using KTool.Advertisement;
using KTool.Init;
using System.Collections;
using UnityEngine;

namespace KPlugin.AppLovinMax
{
    public class AppLovinMaxAppOpen : AdAppOpen, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad display fail code: {0}",
            ERROR_SHOW_FAIL_AD_IS_DESTROY = "Ad show fail: ad  is destroyed",
            ERROR_SHOW_FAIL_AD_NOT_INIT = "Ad show fail: ad not inited",
            ERROR_SHOW_FAIL_AD_NOT_LOADED = "Ad show fail: ad not loaded",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad show fail: ad is showing";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AppLovinMaxAdType.AppOpen)]
        private int indexAd = 0;

        private bool isIniting,
            isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;
        private AdAppOpenTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AppLovinMaxSettingAdId settingAdId = AppLovinMaxSetting.Instance.Ad_Get(AppLovinMaxAdType.AppOpen, indexAd);
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
                base.IsAutoReload = value;
                if (base.IsAutoReload)
                    Load();
            }
        }
        public override bool IsReady => base.IsReady && MaxSdk.IsAppOpenAdReady(AdId);
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
        public IInitTracking InitBegin()
        {
            if (IsDestroy || IsInited || initTrackingSource != null)
                return IInitTracking.Fail;
            //
            initTrackingSource = new InitTrackingSource(initIndispensable, true);
            OnAdInited += Init_OnAdInited;
            Init();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        private void Init_OnAdInited(Ad source, bool isSuccess)
        {
            OnAdInited -= Init_OnAdInited;
            if (isSuccess)
            {
                OnAdLoaded += Init_OnLoaded;
                Load();
            }
            else
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
        }
        private void Init_OnLoaded(Ad source, bool isSuccess)
        {
            OnAdLoaded -= Init_OnLoaded;
            if (isSuccess)
                initTrackingSource.CompleteSuccess();
            else
                initTrackingSource.CompleteFail();
            initTrackingSource = null;
        }
        #endregion

        #region Methods
        public override void Init()
        {
            if (IsInited || isIniting)
                return;
            isIniting = true;
            //
            if (setInstance)
                instance = this;
            StartCoroutine(Ad_Create());
        }
        public override void Load()
        {
            if (!IsInited || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            StartCoroutine(Ad_Load());
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (isIniting || isLoading || IsShow)
                return;
            if (IsInited)
                Ad_Destroy();
            else
                PushEvent_Destroy();
        }
        public override IAdTracking Show()
        {
            if (IsDestroy)
                return new AdAppOpenTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_DESTROY);
            if (!IsInited)
                return new AdAppOpenTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_INIT);
            if (!IsLoaded)
                return new AdAppOpenTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_LOADED);
            if (!IsReady)
                return new AdAppOpenTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_READY);
            if (IsShow)
                return new AdAppOpenTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_SHOWED);
            //
            adTrackingSource = new AdAppOpenTrackingSource(this);
            IsShow = true;
            MaxSdk.ShowAppOpenAd(AdId);
            return adTrackingSource;
        }
        #endregion

        #region Ad
        private IEnumerator Ad_Create()
        {
            while (!AppLovinMaxManager.Instance.IsInit)
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isIniting = false;
                PushEvent_Inited(false);
                //
                PushEvent_Destroy();
                yield break;
            }
            //
            Ad_EventRegister();
            IsInited = true;
            isIniting = false;
            PushEvent_Inited(true);
        }
        private void Ad_Destroy()
        {
            Ad_EventUnRegister();
            PushEvent_Destroy();
        }
        private IEnumerator Ad_Load()
        {
            if (attemptLoad > 0)
                yield return new WaitForSecondsRealtime(attemptLoad * 2);
            else
                yield return new WaitForEndOfFrame();
            //
            if (IsDestroy)
            {
                isLoading = false;
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
                yield break;
            }
            //
            MaxSdk.LoadAppOpenAd(AdId);
        }
        private void Ad_EventRegister()
        {
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnExpiredAdReloadedEvent += Ad_OnExpiredAdReloadedEvent;
        }
        private void Ad_EventUnRegister()
        {
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent -= Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent -= Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent -= Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.AppOpen.OnExpiredAdReloadedEvent -= Ad_OnExpiredAdReloadedEvent;
        }
        private void Ad_OnAdLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            attemptLoad = 0;
            IsLoaded = true;
            isLoading = false;
            if (IsDestroy)
            {
                PushEvent_Loaded(true);
                //
                Ad_Destroy();
            }
            else
            {
                PushEvent_Loaded(true);
            }
        }
        private void Ad_OnAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adId != AdId)
                return;
            //
            if (errorInfo != null)
                Debug.LogWarning(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
            //
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            IsLoaded = false;
            if (IsDestroy)
            {
                isLoading = false;
                PushEvent_Loaded(false);
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                    StartCoroutine(Ad_Load());
                else
                    isLoading = false;
                PushEvent_Loaded(false);
            }
        }
        private void Ad_OnAdDisplayedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Displayed(true);
            adTrackingSource.PushEvent_Displayed(true);
        }
        private void Ad_OnAdDisplayFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            if (errorInfo != null)
                Debug.LogWarning(string.Format(ERROR_DISPLAY_FAIL, errorInfo.Code));
            //
            IsLoaded = false;
            IsShow = false;
            if (IsDestroy)
            {
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
            }
        }
        private void Ad_OnAdClickedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            IsLoaded = false;
            IsShow = false;
            if (IsDestroy)
            {
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                //
                Ad_Destroy();
            }
            else
            {
                if (IsAutoReload)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
            }
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
            adTrackingSource.PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnExpiredAdReloadedEvent(string adId, MaxSdkBase.AdInfo adInfo1, MaxSdkBase.AdInfo adInfo2)
        {
            if (adId != AdId)
                return;
        }
        #endregion
    }
}
