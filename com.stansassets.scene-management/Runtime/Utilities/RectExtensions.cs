using UnityEngine;

namespace StansAssets.SceneManagement.Utilities
{
    public static class RectExtensions
    {
        public static Rect WithWidth(this Rect @this, float width)
        {
            return new Rect(@this.x, @this.y, width, @this.height);
        }
        
        public static Rect WithHeight(this Rect @this, float height)
        {
            return new Rect(@this.x, @this.y, @this.width, height);
        }
        
        public static Rect WithSize(this Rect @this, Vector2 size)
        {
            return new Rect(@this.x, @this.y, size.x, size.y);
        }

        public static Rect RightOf(this Rect @this, Rect other)
        {
            return new Rect(other.x + other.width, @this.y, @this.width, @this.height);
        }

        public static Rect ShiftHorizontally(this Rect @this, float offset)
        {
            return new Rect(@this.x + offset, @this.y, @this.width, @this.height);
        }
        
        public static Rect ShiftVertically(this Rect @this, float offset)
        {
            return new Rect(@this.x, @this.y + offset, @this.width, @this.height);
        }
    }
}
