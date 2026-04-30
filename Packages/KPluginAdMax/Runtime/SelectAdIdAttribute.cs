using System;
using UnityEngine;

namespace KPlugin.AdMax
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SelectAdIdAttribute : PropertyAttribute
    {
        #region Properties
        private AdMaxAdType type;

        public AdMaxAdType Type => type;
        #endregion

        #region Construction
        public SelectAdIdAttribute(AdMaxAdType type) : base()
        {
            this.type = type;
        }
        #endregion

        #region Method

        #endregion
    }
}
