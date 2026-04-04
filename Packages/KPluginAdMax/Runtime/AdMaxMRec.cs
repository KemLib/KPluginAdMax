using KTool;
using KTool.Advertisement;
using KTool.Init;
using System.Collections;
using UnityEngine;
using static MaxSdkBase;

namespace KPlugin.AdMax
{
    public class AdMaxMRec : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad load fail code: {0}",
            ERROR_IS_DESTROY = "Ad is destroy";

        [SerializeField]
        private bool indispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMaxAdType.MRec)]
        private int indexAd = 0;

        private bool isIniting,
            isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;
        private AdBannerTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AdMaxSettingAdId settingAdId = AdMaxSetting.Instance.Ad_Get(AdMaxAdType.MRec, indexAd);
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
                        MaxSdk.StartMRecAutoRefresh(AdId);
                    else
                        MaxSdk.StopMRecAutoRefresh(AdId);
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
                    MaxSdk.UpdateMRecPosition(AdId, Utility.ConvertPosition(PositionType));
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
                    MaxSdk.UpdateMRecPosition(AdId, maxPosition.x, maxPosition.y);
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
            initTrackingSource = new InitTrackingSource(indispensable);
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
                MaxSdk.HideMRec(AdId);
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
            MaxSdk.ShowMRec(AdId);
            PushEvent_Displayed(true);
            adTrackingSource.PushEvent_Displayed(true);
        }
        #endregion

        #region Ad
        private IEnumerator Ad_Create()
        {
            while (!AdMaxManager.IsInit)
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
            MaxSdk.CreateMRec(AdId, adViewConfiguration);
            if (!IsAutoReload)
                MaxSdk.StopMRecAutoRefresh(AdId);
            //
            IsInited = true;
            isIniting = false;
            PushEvent_Inited(true);
        }
        private void Ad_Destroy()
        {
            Ad_EventUnRegister();
            MaxSdk.DestroyMRec(AdId);
            PushEvent_Destroy();
        }
        private IEnumerator Ad_Load()
        {
            if (attemptLoad > 0)
                yield return new WaitForSecondsRealtime(attemptLoad * 2);
            else
                yield return new WaitForEndOfFrame();
            //
            while (!AdMaxManager.IsInit)
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
            MaxSdk.LoadMRec(AdId);
        }
        private void Ad_EventRegister()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent += Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent += Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdReviewCreativeIdGeneratedEvent += Ad_OnAdReviewCreativeIdGeneratedEvent;
        }
        private void Ad_EventUnRegister()
        {
            MaxSdkCallbacks.MRec.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.MRec.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.MRec.OnAdExpandedEvent -= Ad_OnAdExpandedEvent;
            MaxSdkCallbacks.MRec.OnAdCollapsedEvent -= Ad_OnAdCollapsedEvent;
            MaxSdkCallbacks.MRec.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.MRec.OnAdReviewCreativeIdGeneratedEvent -= Ad_OnAdReviewCreativeIdGeneratedEvent;
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
                    MaxSdk.ShowMRec(AdId);
                    PushEvent_Displayed(true);
                    adTrackingSource.PushEvent_Displayed(true);
                }
                else
                {
                    MaxSdk.HideMRec(AdId);
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
