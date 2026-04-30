using KTool.Advertisement;
using TMPro;
using UnityEngine;

namespace KPlugin.AdMax.Example
{
    public class PanelAdMRec : MonoBehaviour
    {
        #region Properties
        private const string CLICK_INIT = "Ad MRec: Click init",
            CLICK_LOAD = "Ad MRec: Click load",
            CLICK_SHOW = "Ad MRec: Click show",
            CLICK_HIDE = "Ad MRec: Click hide";
        private const string AD_EVENT_INIT = "Ad MRec: even Init",
            AD_EVENT_LOADED = "Ad MRec: even Loaded {0}",
            AD_EVENT_DISPLAYED = "Ad MRec: even Displayed {0}",
            AD_EVENT_CLICKED = "Ad MRec: even Clicked",
            AD_EVENT_HIDDEN = "Ad MRec: even Hidden",
            AD_EVENT_REVENUE_PAID = "Ad MRec: even RevenuePaid {0}-{1}",
            AD_EVENT_DESTROY = "Ad MRec: even Destroy",
            AD_EVENT_EXPANDED = "Ad MRec: even Expanded {0}";
        private const string ERROR_ADD_EMPTY = "Ad MRec: No objects to select",
            ERROR_AD_IS_INITED = "Ad MRec: ad is inited",
            ERROR_AD_IS_NOT_INIT = "Ad MRec: ad not init",
            ERROR_AD_IS_LOADED = "Ad MRec: ad is loaded";

        [SerializeField]
        private TMP_Dropdown dropdownAd;

        private PanelLog panelLog;
        private AdMaxMRec selectAd;

        private AdMaxManager manager => AdMaxManager.Instance;
        public bool IsShow => gameObject.activeSelf;
        public int Count => dropdownAd.options.Count;
        public AdMaxMRec SelectAd => selectAd;
        #endregion

        #region Unity Events

        #endregion

        #region Methods
        public void Init(PanelLog panelLog)
        {
            this.panelLog = panelLog;
            //
            if (manager == null || manager.Banner_Count() == 0)
            {
                dropdownAd.options.Clear();
                return;
            }
            //
            dropdownAd.options.Clear();
            int count = manager.Banner_Count();
            for (int i = 0; i < count; i++)
            {
                AdMaxMRec ad = manager.MRec_Get(i);
                dropdownAd.options.Add(new TMP_Dropdown.OptionData(ad.Name));
            }
            dropdownAd.value = 0;
        }
        public void Show()
        {
            if (IsShow)
                return;
            //
            if (Count == 0)
            {
                panelLog.AddLog(ERROR_ADD_EMPTY);
                return;
            }
            gameObject.SetActive(true);
            OnSelectAd(dropdownAd.value);
        }
        public void Hide()
        {
            if (!IsShow)
                return;
            //
            gameObject.SetActive(false);
            SelectAd_EventUnRegister();
            selectAd = null;
        }
        #endregion

        #region Unity Events
        public void OnSelectAd(int value)
        {
            SelectAd_EventUnRegister();
            selectAd = manager.MRec_Get(value);
            SelectAd_EventRegister();
        }
        public void OnClick_Load()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_LOAD);
            //
            if (SelectAd.IsLoaded)
                panelLog.AddLog(ERROR_AD_IS_LOADED);
            else
                SelectAd.Load();
        }
        public void OnClick_Show()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_SHOW);
            //
            SelectAd.Show();
        }
        public void OnClick_Hide()
        {
            if (!IsShow)
                return;
            //
            panelLog.AddLog(CLICK_HIDE);
            //
            SelectAd.Hide();
        }
        #endregion

        #region Ad Event
        private void SelectAd_EventRegister()
        {
            if (selectAd == null)
                return;
            //
            selectAd.OnAdLoaded += SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed += SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked += SelectAd_OnAdClicked;
            selectAd.OnAdHidden += SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid += SelectAd_OnAdRevenuePaid;
            selectAd.OnAdDestroy += SelectAd_OnAdDestroy;
            selectAd.OnAdExpanded += SelectAd_OnAdExpanded;
        }
        private void SelectAd_EventUnRegister()
        {
            if (selectAd == null)
                return;
            //
            selectAd.OnAdLoaded -= SelectAd_OnAdLoaded;
            selectAd.OnAdDisplayed -= SelectAd_OnAdDisplayed;
            selectAd.OnAdClicked -= SelectAd_OnAdClicked;
            selectAd.OnAdHidden -= SelectAd_OnAdHidden;
            selectAd.OnAdRevenuePaid -= SelectAd_OnAdRevenuePaid;
            selectAd.OnAdDestroy -= SelectAd_OnAdDestroy;
            selectAd.OnAdExpanded -= SelectAd_OnAdExpanded;
        }
        private void SelectAd_OnAdLoaded(AdBase adSource, bool isSuccess)
        {
            panelLog.AddLog(string.Format(AD_EVENT_LOADED, isSuccess));
        }
        private void SelectAd_OnAdDisplayed(AdBase adSource, bool isSuccess, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_DISPLAYED, isSuccess));
        }
        private void SelectAd_OnAdClicked(AdBase adSource, string placement)
        {
            panelLog.AddLog(AD_EVENT_CLICKED);
        }
        private void SelectAd_OnAdHidden(AdBase adSource, string placement)
        {
            panelLog.AddLog(AD_EVENT_HIDDEN);
        }
        private void SelectAd_OnAdRevenuePaid(AdBase adSource, AdRevenuePaid revenuePaid, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_REVENUE_PAID, revenuePaid.Value, revenuePaid.Currency));
        }
        private void SelectAd_OnAdExpanded(AdBase adSource, bool isExpanded, string placement)
        {
            panelLog.AddLog(string.Format(AD_EVENT_EXPANDED, isExpanded));
        }
        private void SelectAd_OnAdDestroy(AdBase adSource)
        {
            panelLog.AddLog(AD_EVENT_DESTROY);
        }
        #endregion
    }
}
