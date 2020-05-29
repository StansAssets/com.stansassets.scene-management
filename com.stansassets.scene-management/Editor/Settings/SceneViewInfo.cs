using System;
using UnityEngine;

namespace StansAssets.SceneManagement
{
    [Serializable]
    class SceneViewInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Pivot;
        public float Size;
        public bool is2D;
        public bool isOrtho;

        public SceneViewInfo(UnityEngine.Vector3 Position, Vector3 Pivot, Quaternion Rotation, float Size, bool is2D, bool isOrtho)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            this.Pivot = Pivot;
            this.Size = Size;
            this.is2D = is2D;
            this.isOrtho = isOrtho;
        }
    }
}
