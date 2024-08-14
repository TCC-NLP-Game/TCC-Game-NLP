using UnityEngine;

namespace Convai.Scripts.Utils
{
    public static class RectExtension
    {
        public static Rect With(this Rect rect, float? x = null, float? y = null, float? height = null, float? width = null)
        {
            return new Rect(x ?? rect.x, y ?? rect.y, width ?? rect.width, height ?? rect.height);
        }
    }
}