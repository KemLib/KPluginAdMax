using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

namespace KPlugin.AdMax
{
    public class AdMaxBanner : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Banner load fail code: {0}",
            ERROR_IS_DESTROY = "Ad Banner is destroy";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMaxAdType.Banner)]
        private int indexAd = 0;
        [SerializeField]
        private bool customBackgroundColor;
        [SerializeField]
        private Color backgroundColor = Color.white;

        private bool isLoading;
        private int attemptLoad;
        private bool isCreateAdObject;
        private InitTrackingSource initTrackingSource;
        private AdBannerTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AdMaxSettingAdId settingAdId = AdMaxSetting.Instance.Ad_Get(AdMaxAdType.Banner, indexAd);
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
            initTrackingSource = new InitTrackingSource(initIndispensable);
            OnAdLoaded += Init_OnAdLoaded;
            Load();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        private void Init_OnAdLoaded(Ad source, bool isSuccess)
        {
            OnAdLoaded -= Init_OnAdLoaded;
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
            if (IsDestroy || IsInited)
                return;
            IsInited = true;
            //
            if (setInstance)
                instance = this;
            PushEvent_Inited(true);
        }
        public override void Load()
        {
            if (IsDestroy)
                return;
            Init();
            //
            if (isCreateAdObject)
            {
                if (!IsAutoReload)
                    MaxSdk.LoadBanner(AdId);
            }
            else
            {
                Ad_Create();
            }
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
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
                {
                    MaxSdk.ShowBanner(AdId);
                    PushEvent_Displayed(true);
                    adTrackingSource.PushEvent_Displayed(true);
                }
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
        #endregion

        #region Ad Event
        private void Ad_EventRegister()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent += Ad_OnAdReviewCreativeIdGeneratedEvent;
        }
        private void Ad_EventUnRegister()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent -= Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdReviewCreativeIdGeneratedEvent -= Ad_OnAdReviewCreativeIdGeneratedEvent;
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            float delay = attemptLoad > 0 ? Mathf.Pow(2, attemptLoad) : 0;
            CronObject.Create()
                .Add(ConditionReadTime.Create(delay))
                .Add(ConditionDelegate.Create(AdMaxManager.IsReady))
                .Add(CallbackAction.Create(Ad_LoadAd))
                .Run();
        }
        private void Ad_Destroy()
        {
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
            Ad_EventUnRegister();
            MaxSdk.DestroyBanner(AdId);
            isCreateAdObject = false;
        }
        private void Ad_LoadAd()
        {
            if (IsDestroy)
            {
                isLoading = false;
                return;
            }
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
            Ad_EventRegister();
            MaxSdk.CreateBanner(AdId, adViewConfiguration);
            if (customBackgroundColor)
                MaxSdk.SetBannerBackgroundColor(AdId, backgroundColor);
            isCreateAdObject = true;
        }
        private void Ad_OnAdLoadedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            isLoading = false;
            IsLoaded = true;
            attemptLoad = 0;
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
            isLoading = false;
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            //
            if (errorInfo != null)
                Debug.LogError(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
            //
            PushEvent_Loaded(false);
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
                source: AdMaxManager.MAX_SCOURCE,
                network_name: adInfo.NetworkName,
                idAd: AdId,
                adType: AdType,
                countryCode: AdMaxManager.CountryCode,
                placement: adInfo.Placement,
                value: adInfo.Revenue,
                currency: AdMaxManager.MAX_CURRENCY);
            //
            PushEvent_RevenuePaid(revenuePaid);
            adTrackingSource?.PushEvent_RevenuePaid(revenuePaid);
        }
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, AdInfo adInfo)
        {

        }
        #endregion
    }
}
