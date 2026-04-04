using KTool.Advertisement;
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
        private bool indispensable;
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
            Ad_EventRegister();
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
            if (!IsShow)
            {
                Ad_EventUnRegister();
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

        #region Ad
        private void Ad_Create()
        {
            if (isLoading)
                return;
            isLoading = true;
            //
            StartCoroutine(Ad_LoadAd());
        }
        private IEnumerator Ad_LoadAd()
        {
            if (attemptLoad > 0)
            {
                float delay = Mathf.Pow(2, attemptLoad);
                yield return new WaitForSecondsRealtime(delay);
            }
            //
            while (!AdMaxManager.IsInit)
                yield return new WaitForEndOfFrame();
            //
            MaxSdk.LoadRewardedAd(AdId);
        }
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
                Debug.LogError(string.Format(ERROR_LOAD_FAIL, errorInfo.Code));
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
            adTrackingSource.PushEvent_Displayed(true);
        }
        private void Ad_OnAdDisplayFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            IsShow = false;
            //
            if (errorInfo != null)
                Debug.LogError(string.Format(ERROR_DISPLAY_FAIL, errorInfo.Code));
            PushEvent_Displayed(false);
            adTrackingSource.PushEvent_Displayed(false);
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
            adTrackingSource.PushEvent_Clicked();
        }
        private void Ad_OnAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            if (adId != AdId)
                return;
            IsShow = false;
            //
            PushEvent_Hidden();
            adTrackingSource.PushEvent_Hidden();
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
            if (adId != AdId)
                return;
        }
        private void Ad_OnExpiredAdReloadedEvent(string adId, MaxSdkBase.AdInfo adInfo1, MaxSdkBase.AdInfo adInfo2)
        {
            if (adId != AdId)
                return;
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
