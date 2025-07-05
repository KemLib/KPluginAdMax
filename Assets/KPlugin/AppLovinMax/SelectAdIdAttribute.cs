using System;
using UnityEngine;

namespace KPlugin.AppLovinMax
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SelectAdIdAttribute : PropertyAttribute
    {
        #region Properties
        private AppLovinMaxAdType type;

        public AppLovinMaxAdType Type => type;
        #endregion

        #region Construction
        public SelectAdIdAttribute(AppLovinMaxAdType type) : base()
        {
            this.type = type;
        }
        #endregion

        #region Method

        #endregion
    }
}
