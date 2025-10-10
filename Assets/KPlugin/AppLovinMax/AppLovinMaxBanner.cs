using KTool;
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
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_IS_DESTROY = "Ad is destroy";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AppLovinMaxAdType.Banner)]
        private int indexAd = 0;
        [SerializeField]
        private bool customBackgroundColor;
        [SerializeField]
        private Color backgroundColor = Color.white;

        private bool isIniting,
            isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;
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
                if (customBackgroundColor && IsInited)
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
                if (customBackgroundColor && IsInited)
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
                base.PositionType = value;
                if (IsInited)
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
                base.Position = value;
                if (PositionType == AdPosition.Custom && IsInited)
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
                if (!isLoading)
                {
                    isLoading = true;
                    StartCoroutine(Ad_Load());
                }
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
            if (!IsInited || IsLoaded || IsAutoReload || isLoading)
                return;
            isLoading = true;
            //
            StartCoroutine(Ad_Load());
        }
        public override void Destroy()
        {
            IsDestroy = true;
            if (isIniting || isLoading)
                return;
            if (IsInited)
            {
                if (IsShow)
                    Hide();
                Ad_Destroy();
            }
            else
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
                    CoroutineManager.Instance.Coroutine_Start(Delay_DisplayedAd());
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
            MaxSdk.CreateBanner(AdId, adViewConfiguration);
            if (customBackgroundColor)
                MaxSdk.SetBannerBackgroundColor(AdId, backgroundColor);
            if (!IsAutoReload)
                MaxSdk.StopBannerAutoRefresh(AdId);
            //
            IsInited = true;
            isIniting = false;
            PushEvent_Inited(true);
        }
        private void Ad_Destroy()
        {
            Ad_EventUnRegister();
            MaxSdk.DestroyBanner(AdId);
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
            MaxSdk.LoadBanner(AdId);
        }
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
        private void Ad_OnAdLoadedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            attemptLoad = 0;
            IsLoaded = true;
            isLoading = false;
            //
            if (IsDestroy)
            {
                PushEvent_Loaded(true);
                //
                Ad_Destroy();
            }
            else
            {
                PushEvent_Loaded(true);
                if (IsShow)
                {
                    MaxSdk.ShowBanner(AdId);
                    PushEvent_Displayed(true);
                    adTrackingSource.PushEvent_Displayed(true);
                }
                else
                {
                    MaxSdk.HideBanner(AdId);
                }
            }
        }
        private void Ad_OnAdLoadFailedEvent(string adId, ErrorInfo errorInfo)
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
        private void Ad_OnAdExpandedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Expanded(true);
            adTrackingSource.PushEvent_Expanded(true);
        }
        private void Ad_OnAdCollapsedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Expanded(false);
            adTrackingSource.PushEvent_Expanded(false);
        }
        private void Ad_OnAdClickedEvent(string adId, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            PushEvent_Clicked();
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnAdRevenuePaidEvent(string adId, AdInfo adInfo)
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
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
        }
        #endregion
    }
}
