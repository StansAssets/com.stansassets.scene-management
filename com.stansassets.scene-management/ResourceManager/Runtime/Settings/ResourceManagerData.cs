using System.Collections.Generic;
using System.Linq;
using StansAssets.Plugins;
using UnityEngine;

namespace StansAssets.ResourceManager
{
    class ResourceManagerData : PackageScriptableSettingsSingleton<ResourceManagerData>
    {
        [SerializeField] List<ResourceGroup> m_ResourceGroups = new List<ResourceGroup>();

        internal List<ResourceGroup> ResourceGroups => m_ResourceGroups;

        internal ResourceGroup DefaultGroup => ResourceGroups.FirstOrDefault(g => g.Name.Equals("project"))
                                               ?? new ResourceGroup { Name = "project", DisplayName = "project" };

        public override string PackageName => "com.stansassets.resource-manager";
    }
}