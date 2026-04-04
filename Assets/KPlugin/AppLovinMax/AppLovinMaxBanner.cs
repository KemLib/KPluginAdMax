using KTool.Advertisement;
using KTool.Init;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

namespace KPlugin.AppLovinMax
{
    public class AppLovinMaxBanner : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Banner load fail code: {0}",
            ERROR_IS_DESTROY = "Ad Banner is destroy";

        [SerializeField]
        private bool indispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AppLovinMaxAdType.Banner)]
        private int indexAd = 0;
        [SerializeField]
        private bool customBackgroundColor;
        [SerializeField]
        private Color backgroundColor = Color.white;

        private int attemptLoad;
        private bool isCreateAdObject;
        private InitTrackingSource initTrackingSource;
        private Coroutine coroutineLoading;
        private AdBannerTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AppLovinMaxSettingAdId settingAdId = AppLovinMaxSetting.Instance.Ad_Get(AppLovinMaxAdType.Banner, indexAd);
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
                if (IsReady)
                {
                    if (IsAutoReload)
                        MaxSdk.StartBannerAutoRefresh(AdId);
                    else
                        MaxSdk.StopBannerAutoRefresh(AdId);
                }
            }
        }
        public bool CustomBackgroundColor
        {
            get => customBackgroundColor;
            set
            {
                customBackgroundColor = value;
                if (customBackgroundColor && isCreateAdObject)
                {
                    MaxSdk.SetBannerBackgroundColor(AdId, backgroundColor);
                }
            }
        }
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                if (customBackgroundColor && isCreateAdObject)
                {
                    MaxSdk.SetBannerBackgroundColor(AdId, backgroundColor);
                }
            }
        }
        public override AdPosition PositionType
        {
            get => base.PositionType;
            protected set
            {
                if (value == base.PositionType)
                    return;
                //
                base.PositionType = value;
                if (IsReady)
                {
                    MaxSdk.UpdateBannerPosition(AdId, Utility.ConvertPosition(PositionType));
                }
            }
        }
        public override Vector2 Position
        {
            get => base.Position;
            protected set
            {
                if (value == base.Position)
                    return;
                //
                base.Position = value;
                if (IsReady)
                {
                    Vector2 maxPosition = Utility.Convert_UnityToMax(Position);
                    MaxSdk.UpdateBannerPosition(AdId, maxPosition.x, maxPosition.y);
                }
            }
        }
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
            initTrackingSource = new InitTrackingSource(indispensable);
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
            PushEvent_Inited(true);
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
            Ad_Destroy();
            PushEvent_Destroy();
        }
        public override IAdBannerTracking Show()
        {
            if (IsDestroy)
                return new AdBannerTrackingSource(this, ERROR_IS_DESTROY);
            //
            if (IsShow)
            {
                return adTrackingSource;
            }
            else
            {
                adTrackingSource = new AdBannerTrackingSource(this);
                IsShow = true;
                if (IsLoaded)
                    StartCoroutine(Delay_DisplayedAd());
                return adTrackingSource;
            }
        }
        public override void Hide()
        {
            if (IsShow)
            {
                MaxSdk.HideBanner(AdId);
                IsShow = false;
                //
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                adTrackingSource = null;
            }
            else
            {
                IsShow = false;
            }
        }
        private IEnumerator Delay_DisplayedAd()
        {
            yield return new WaitForEndOfFrame();
            MaxSdk.ShowBanner(AdId);
            PushEvent_Displayed(true);
            adTrackingSource.PushEvent_Displayed(true);
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (coroutineLoading != null)
                return;
            //
            coroutineLoading = StartCoroutine(Ad_LoadAd());
        }
        private void Ad_Destroy()
        {
            if (coroutineLoading != null)
            {
                StopCoroutine(coroutineLoading);
                coroutineLoading = null;
            }
            //
            if (!isCreateAdObject)
            {
                IsShow = false;
                return;
            }
            //
            if (IsLoaded)
            {
                if (IsShow)
                {
                    MaxSdk.HideBanner(AdId);
                    IsShow = false;
                    //
                    PushEvent_Hidden();
                    adTrackingSource.PushEvent_Hidden();
                    adTrackingSource = null;
                }
                IsLoaded = false;
            }
            else if (IsShow)
                IsShow = false;
            //
            Banner_EventUnRegister();
            MaxSdk.DestroyBanner(AdId);
            isCreateAdObject = false;
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
            AdViewConfiguration adViewConfiguration;
            if (PositionType == AdPosition.Custom)
            {
                Vector2 maxPosition = Utility.Convert_UnityToMax(Position);
                adViewConfiguration = new MaxSdk.AdViewConfiguration(maxPosition.x, maxPosition.y);
            }
            else
            {
                AdViewPosition viewPosition = Utility.ConvertPosition(PositionType);
                adViewConfiguration = new MaxSdk.AdViewConfiguration(viewPosition);
            }
            Banner_EventRegister();
            MaxSdk.CreateBanner(AdId, adViewConfiguration);
            if (customBackgroundColor)
                MaxSdk.SetBannerBackgroundColor(AdId, backgroundColor);
            isCreateAdObject = true;
        }
        private void Banner_EventRegister()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent += Ad_OnAdReviewCreativeIdGeneratedEvent;
        }
        private void Banner_EventUnRegister()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent -= Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent -= Ad_OnAdReviewCreativeIdGeneratedEvent;
        }
        private void Ad_OnAdLoadedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            coroutineLoading = null;
            attemptLoad = 0;
            IsLoaded = true;
            //
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteSuccess();
                initTrackingSource = null;
            }
            //
            if (IsShow)
                MaxSdk.ShowBanner(AdId);
            else
                MaxSdk.HideBanner(AdId);
            PushEvent_Loaded(true);
        }
        private void Ad_OnAdLoadFailedEvent(string adId, ErrorInfo errorInfo)
        {
            if (adId != AdId)
                return;
            //
            coroutineLoading = null;
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            //
            if (errorInfo != null)
                Debug.LogError(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
            if (initTrackingSource != null)
            {
                initTrackingSource.CompleteFail();
                initTrackingSource = null;
            }
            //
            PushEvent_Loaded(false);
            if (!IsDestroy && !IsAutoReload)
            {
                MaxSdk.LoadBanner(AdId);
            }
        }
        private void Ad_OnAdExpandedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Expanded(true);
            adTrackingSource?.PushEvent_Expanded(true);
        }
        private void Ad_OnAdCollapsedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Expanded(false);
            adTrackingSource?.PushEvent_Expanded(false);
        }
        private void Ad_OnAdClickedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Clicked();
            adTrackingSource?.PushEvent_Clicked();
        }
        private void Ad_OnAdRevenuePaidEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            if (adInfo == null)
                return;
            //
            AdRevenuePaid revenuePaid = new AdRevenuePaid(
                source: AppLovinMaxManager.MAX_SCOURCE,
                network_name: adInfo.NetworkName,
                idAd: AdId,
                adType: AdType,
                countryCode: AppLovinMaxManager.CountryCode,
                placement: adInfo.Placement,
                value: adInfo.Revenue,
                currency: AppLovinMaxManager.MAX_CURRENCY);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource?.PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
        }
        #endregion
    }
}
