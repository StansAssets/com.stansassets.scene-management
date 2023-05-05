using System;
using System.Collections.Generic;
using UnityEngine;

namespace StansAssets.ResourceManager
{
    [Serializable]
    class ResourceGroup
    {
        [SerializeField] string m_Name;
        [SerializeField] string m_DisplayName;
        [SerializeField] List<ResourceItem> m_Resources = new List<ResourceItem>();

        internal string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        internal string DisplayName
        {
            get => m_DisplayName;
            set => m_DisplayName = value;
        }

        internal List<ResourceItem> Resources => m_Resources;
    }
}