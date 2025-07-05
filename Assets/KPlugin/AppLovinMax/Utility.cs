using KTool.Advertisement;
using UnityEngine;

namespace KPlugin.AppLovinMax
{
    public static class Utility
    {
        #region Properties
        public static float DensityScreen
        {
            get
            {
#if UNITY_EDITOR
                return Screen.height / 800f;
#else
                return MaxSdkUtils.GetScreenDensity();
#endif
            }
        }
        #endregion

        #region Method
        public static Vector2 Unity_GetScreen()
        {
            return new Vector2(Screen.width, Screen.height);
        }
        public static Vector2 Unity_BannerSize()
        {
            Vector2 sizeAppLovinMax = Max_BannerSize();
            return Convert_MaxToUnity(sizeAppLovinMax);
        }
        public static Vector2 Unity_MrecSize()
        {
            Vector2 sizeAppLovinMax = Max_MrecSize();
            return Convert_MaxToUnity(sizeAppLovinMax);
        }
        public static Vector2 Unity_GetPosition(AdPosition adPosition, Vector2 bannerSize)
        {
            Vector2 positionAppLovinMax = Max_GetPosition(adPosition, Convert_UnityToMax(bannerSize));
            return Convert_MaxToUnity(positionAppLovinMax);
        }
        public static Vector2 Convert_UnityToMax(Vector2 vecter)
        {
            float scale = DensityScreen;
            return new Vector2(vecter.x / scale, vecter.y / scale);
        }
        public static Vector2 Convert_MaxToUnity(Vector2 vecter)
        {
            float scale = DensityScreen;
            return new Vector2(vecter.x * scale, vecter.y * scale);
        }
        public static MaxSdkBase.AdViewPosition ConvertPosition(AdPosition adPosition)
        {
            switch (adPosition)
            {
                case AdPosition.TopLeft:
                    return MaxSdkBase.AdViewPosition.TopLeft;
                case AdPosition.TopCenter:
                    return MaxSdkBase.AdViewPosition.TopCenter;
                case AdPosition.TopRight:
                    return MaxSdkBase.AdViewPosition.TopRight;
                case AdPosition.MidLeft:
                    return MaxSdkBase.AdViewPosition.CenterLeft;
                case AdPosition.MidCenter:
                    return MaxSdkBase.AdViewPosition.Centered;
                case AdPosition.MidRight:
                    return MaxSdkBase.AdViewPosition.CenterRight;
                case AdPosition.BotLeft:
                    return MaxSdkBase.AdViewPosition.BottomLeft;
                case AdPosition.BotCenter:
                    return MaxSdkBase.AdViewPosition.BottomCenter;
                case AdPosition.BotRight:
                    return MaxSdkBase.AdViewPosition.BottomRight;
                default:
                    return MaxSdkBase.AdViewPosition.BottomCenter;

            }
        }
        #endregion

        #region AppLovinMax
        public static Vector2 Max_GetScreen()
        {
            Vector2 unityScreen = Unity_GetScreen();
            return Convert_UnityToMax(unityScreen);
        }
        public static Vector2 Max_BannerSize()
        {
            if (MaxSdkUtils.IsTablet())
                return new Vector2(728, 90);
            else
                return new Vector2(320, 50);
        }
        public static Vector2 Max_MrecSize()
        {
            return new Vector2(300, 250);
        }
        public static Vector2 Max_GetPosition(AdPosition adPosition, Vector2 bannerSize)
        {
            Vector2 screenSize = Max_GetScreen();
            switch (adPosition)
            {
                case AdPosition.TopLeft:
                    return new Vector2(0, 0);
                case AdPosition.TopCenter:
                    return new Vector2((screenSize.x - bannerSize.x) / 2, 0);
                case AdPosition.TopRight:
                    return new Vector2(screenSize.x - bannerSize.x, 0);
                //
                case AdPosition.MidLeft:
                    return new Vector2(0, (screenSize.y - bannerSize.y) / 2);
                case AdPosition.MidCenter:
                    return new Vector2((screenSize.x - bannerSize.x) / 2, (screenSize.y - bannerSize.y) / 2);
                case AdPosition.MidRight:
                    return new Vector2((screenSize.x - bannerSize.x) / 2, (screenSize.y - bannerSize.y) / 2);
                //
                case AdPosition.BotLeft:
                    return new Vector2(0, screenSize.y - bannerSize.y);
                case AdPosition.BotCenter:
                    return new Vector2((screenSize.x - bannerSize.x) / 2, screenSize.y - bannerSize.y);
                case AdPosition.BotRight:
                    return new Vector2(screenSize.x - bannerSize.x, screenSize.y - bannerSize.y);
                default:
                    return new Vector2(0, 0);
            }
        }
        #endregion
    }
}
