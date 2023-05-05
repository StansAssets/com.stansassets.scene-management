using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StansAssets.ResourceManager
{
    [Serializable]
    class ResourceItem
    {
        [SerializeField] string m_Guid;
        [SerializeField] string m_FileName;
        [SerializeField] string m_DisplayName;
        [SerializeField] bool m_Addressable;

        internal Object ObjectRef
        {
            get
            {
                var path = AssetDatabase.GUIDToAssetPath(Guid);
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                return obj;
            }
            set
            {
                var path = AssetDatabase.GetAssetPath(value);
                Guid = AssetDatabase.AssetPathToGUID(path);
                FileName = System.IO.Path.GetFileNameWithoutExtension(path);
            }
        }

        internal string FileName
        {
            get => m_FileName;
            set => m_FileName = value;
        }

        internal string DisplayName
        {
            get => m_DisplayName;
            set => m_DisplayName = value;
        }

        internal bool Addressable
        {
            get => m_Addressable;
            set => m_Addressable = value;
        }
        
        public string Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }
    }
}