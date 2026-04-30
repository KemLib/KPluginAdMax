using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using UnityEngine;
using static MaxSdkBase;

namespace KPlugin.AdMax
{
    public class AdMaxMRec : AdBanner, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad MRec load fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad MRec show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad MRec show fail: ad is show";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMaxAdType.MRec)]
        private int indexAd = 0;

        private bool isInit,
            isCreating,
            isCreated,
            isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;

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
                if (isCreated)
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
                if (value == base.PositionType)
                    return;
                //
                base.PositionType = value;
                if (isCreated)
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
                if (value == base.Position)
                    return;
                //
                base.Position = value;
                if (isCreated && PositionType == AdPosition.Custom)
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
            if (IsDestroy || isInit || initTrackingSource != null)
                return IInitTracking.Fail;
            //
            initTrackingSource = new InitTrackingSource(initIndispensable);
            OnAdLoaded += Init_OnLoaded;
            Load();
            return initTrackingSource;
        }
        public void InitEnd()
        {

        }
        private void Init_OnLoaded(AdBase source, bool isSuccess)
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
        public override void Load()
        {
            if (IsDestroy)
                return;
            //
            if (!isInit)
            {
                isInit = true;
                //
                if (setInstance)
                    instance = this;
            }
            //
            if (isCreated)
            {
                if (!IsAutoReload)
                    Ad_Load_Begin();
            }
            else
            {
                Ad_Create_Begin();
            }
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            //
            IsDestroy = true;
            Ad_Destroy();
            PushEvent_Destroy();
        }
        protected override bool OnShow(out string error)
        {
            if (IsShow)
            {
                error = ERROR_SHOW_FAIL_AD_IS_SHOWED;
                return false;
            }
            if (!IsReady)
            {
                error = ERROR_SHOW_FAIL_AD_NOT_READY;
                return false;
            }
            //
            IsShow = true;
            MaxSdk.SetMRecPlacement(AdId, Placement);
            MaxSdk.ShowMRec(AdId);
            PushEvent_Displayed(true);
            //
            error = null;
            return true;
        }
        protected override bool OnHide()
        {
            if (!IsShow)
                return false;
            //
            MaxSdk.HideMRec(AdId);
            IsShow = false;
            PushEvent_Hidden();
            //
            return true;
        }
        #endregion

        #region Ad Event
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
        #endregion

        #region Ad
        private void Ad_Destroy()
        {
            if (!isCreated)
                return;
            //
            if (IsLoaded)
            {
                if (IsShow)
                {
                    MaxSdk.HideMRec(AdId);
                    IsShow = false;
                    //
                    PushEvent_Hidden();
                }
                IsLoaded = false;
            }
            //
            Ad_EventUnRegister();
            MaxSdk.DestroyMRec(AdId);
            isCreated = false;
        }
        private void Ad_Create_Begin()
        {
            if (isCreated || isCreating)
                return;
            isCreating = true;
            //
            if (AdMaxManager.IsReady())
            {
                Ad_Create();
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionDelegate.Create(AdMaxManager.IsReady))
                    .Add(CallbackAction.Create(Ad_Create))
                    .Run();
            }
        }
        private void Ad_Create()
        {
            if (IsDestroy)
            {
                isCreating = false;
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
            MaxSdk.CreateMRec(AdId, adViewConfiguration);
            if (!IsAutoReload)
                MaxSdk.StopMRecAutoRefresh(AdId);
            //
            isCreating = false;
            isCreated = true;
            //
            if (!IsAutoReload)
                Ad_Load_Begin();
        }
        private void Ad_Load_Begin()
        {
            if (!isCreated || IsLoaded || isLoading)
                return;
            isLoading = true;
            //
            float delay = attemptLoad > 0 ? Mathf.Pow(2, attemptLoad) : 0;
            if (delay <= 0)
            {
                Ad_Load();
            }
            else
            {
                CronObject.Create()
                    .Add(ConditionReadTime.Create(delay))
                    .Add(CallbackAction.Create(Ad_Load))
                    .Run();
            }
        }
        private void Ad_Load()
        {
            if (IsDestroy)
            {
                isLoading = false;
            }
            else
            {
                MaxSdk.LoadMRec(AdId);
            }
        }
        private void Ad_OnAdLoadedEvent(string adId, AdInfo adInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
            isLoading = false;
            if (IsDestroy)
                return;
            //
            IsLoaded = true;
            attemptLoad = 0;
            PushEvent_Loaded(true);
        }
        private void Ad_OnAdLoadFailedEvent(string adId, ErrorInfo errorInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
            isLoading = false;
            if (IsDestroy)
                return;
            //
            attemptLoad = Mathf.Min(attemptLoad + 1, 6);
            if (errorInfo != null)
                Debug.LogError(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
            PushEvent_Loaded(false);
            //
            if (!IsAutoReload)
                Ad_Load_Begin();
        }
        private void Ad_OnAdExpandedEvent(string adId, AdInfo adInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
            PushEvent_Expanded(true);
        }
        private void Ad_OnAdCollapsedEvent(string adId, AdInfo adInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
            PushEvent_Expanded(false);
        }
        private void Ad_OnAdClickedEvent(string adId, AdInfo adInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
            PushEvent_Clicked();
        }
        private void Ad_OnAdRevenuePaidEvent(string adId, AdInfo adInfo)
        {
            if (!string.Equals(adId, AdId))
                return;
            //
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
        }
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, AdInfo adInfo)
        {

        }
        #endregion
    }
}
