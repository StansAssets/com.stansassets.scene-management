using System;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    [Serializable]
    class SceneViewInfo
    {
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Pivot { get; private set; }
        public float Size { get; private set; }
        public bool Is2D { get; private set; }
        public bool IsOrtho { get; private set; }

        public SceneViewInfo(Vector3 position, Vector3 pivot, Quaternion rotation, float size, bool is2D, bool isOrtho)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Pivot = pivot;
            this.Size = size;
            this.Is2D = is2D;
            this.IsOrtho = isOrtho;
        }
    }
}
