using KTool.Advertisement;
using KTool.Cron;
using KTool.Init;
using System.Collections;
using UnityEngine;

namespace KPlugin.AdMax
{
    public class AdMaxRewarded : AdRewarded, IIniter
    {
        #region Properties
        private const string ERROR_LOAD_FAIL = "Ad Rewarded load fail code: {0}",
            ERROR_DISPLAY_FAIL = "Ad Rewarded display fail code: {0}",
            ERROR_SHOW_FAIL_AD_NOT_READY = "Ad Rewarded show fail: ad not ready",
            ERROR_SHOW_FAIL_AD_IS_SHOWED = "Ad Rewarded show fail: ad is show";

        [SerializeField]
        private bool initIndispensable;
        [SerializeField]
        private bool setInstance;
        [SerializeField, SelectAdId(AdMaxAdType.Rewarded)]
        private int indexAd = 0;

        private bool isLoading;
        private int attemptLoad;
        private InitTrackingSource initTrackingSource;
        private AdRewardedTrackingSource adTrackingSource;

        public string AdId
        {
            get
            {
                AdMaxSettingAdId settingAdId = AdMaxSetting.Instance.Ad_Get(AdMaxAdType.Rewarded, indexAd);
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
        public override bool IsReady => base.IsReady && MaxSdk.IsRewardedAdReady(AdId);
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
            Ad_EventRegister();
            PushEvent_Inited(true);
        }
        public override void Load()
        {
            if (IsDestroy)
                return;
            Init();
            //
            Ad_Create();
        }
        public override void Destroy()
        {
            if (IsDestroy)
                return;
            IsDestroy = true;
            if (IsInited)
            {
                if (!IsShow)
                {
                    Ad_EventUnRegister();
                    PushEvent_Destroy();
                }
            }
            else
            {
                PushEvent_Destroy();
            }
        }
        public override IAdRewardedTracking Show(string placement = "")
        {
            if (IsShow)
                return new AdRewardedTrackingSource(this, ERROR_SHOW_FAIL_AD_IS_SHOWED);
            if (!IsReady)
                return new AdRewardedTrackingSource(this, ERROR_SHOW_FAIL_AD_NOT_READY);
            //
            adTrackingSource = new AdRewardedTrackingSource(this);
            IsShow = true;
            if (string.IsNullOrEmpty(placement))
                MaxSdk.ShowRewardedAd(AdId);
            else
                MaxSdk.ShowRewardedAd(AdId, placement);
            return adTrackingSource;
        }
        #endregion

        #region Ad Event
        private void Ad_EventRegister()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReviewCreativeIdGeneratedEvent += Ad_OnAdReviewCreativeIdGeneratedEvent;
            MaxSdkCallbacks.Rewarded.OnExpiredAdReloadedEvent += Ad_OnExpiredAdReloadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += Ad_OnAdReceivedRewardEvent;
        }
        private void Ad_EventUnRegister()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= Ad_OnAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= Ad_OnAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= Ad_OnAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= Ad_OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= Ad_OnAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= Ad_OnAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= Ad_OnAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdReviewCreativeIdGeneratedEvent -= Ad_OnAdReviewCreativeIdGeneratedEvent;
            MaxSdkCallbacks.Rewarded.OnExpiredAdReloadedEvent -= Ad_OnExpiredAdReloadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += Ad_OnAdReceivedRewardEvent;
        }
        #endregion

        #region Ad
        private void Ad_Create()
        {
            if (IsDestroy || IsLoaded || isLoading)
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
        private void Ad_LoadAd()
        {
            if (IsDestroy)
            {
                isLoading = false;
                return;
            }
            MaxSdk.LoadRewardedAd(AdId);
        }
        private void Ad_OnAdLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            isLoading = false;
            if (IsDestroy)
            {
                return;
            }
            else
            {
                IsLoaded = true;
                attemptLoad = 0;
                //
                PushEvent_Loaded(true);
            }
        }
        private void Ad_OnAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            if (adId != AdId)
                return;
            isLoading = false;
            if (IsDestroy)
            {
                return;
            }
            else
            {
                attemptLoad = Mathf.Min(attemptLoad + 1, 6);
                if (errorInfo != null)
                    Debug.LogError(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
                PushEvent_Loaded(false);
                //
                if (IsAutoReload)
                    Ad_Create();
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
            IsShow = false;
            IsLoaded = false;
            if (IsDestroy)
            {
                Ad_EventUnRegister();
                PushEvent_Destroy();
            }
            else
            {
                if (errorInfo != null)
                    Debug.LogError(string.Format(ERROR_DISPLAY_FAIL, errorInfo.Code));
                PushEvent_Displayed(false);
                adTrackingSource.PushEvent_Displayed(false);
                //
                if (IsAutoReload)
                    Ad_Create();
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
            IsShow = false;
            IsLoaded = false;
            if (IsDestroy)
            {
                Ad_EventUnRegister();
                PushEvent_Destroy();
            }
            else
            {
                PushEvent_Hidden();
                adTrackingSource.PushEvent_Hidden();
                //
                if(IsAutoReload)
                    Ad_Create();
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
        private void Ad_OnAdReviewCreativeIdGeneratedEvent(string adId, string arg2, MaxSdkBase.AdInfo adInfo)
        {

        }
        private void Ad_OnExpiredAdReloadedEvent(string adId, MaxSdkBase.AdInfo adInfo1, MaxSdkBase.AdInfo adInfo2)
        {

        }
        private void Ad_OnAdReceivedRewardEvent(string adId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            //
            AdRewardReceived rewardReceived = new AdRewardReceived(reward.Label, reward.Amount > 0, reward.Amount);
            PushEvent_ReceivedReward(rewardReceived);
            adTrackingSource.PushEvent_ReceivedReward(rewardReceived);
        }
        #endregion
    }
}
