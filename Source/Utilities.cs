// Utilities.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CommandPalette {
    public static class Utilities {
        public static void ApplyUIScale(float scale) {
            UI.screenWidth = Mathf.RoundToInt(Screen.width / scale);
            UI.screenHeight = Mathf.RoundToInt(Screen.height / scale);
            GUI.matrix = Matrix4x4.TRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity,
                                        new Vector3(scale, scale, 1f));
        }

        public static Rect Bounded(this Rect rect, Rect other) {
            Rect outRect = new Rect( rect );
            if (outRect.width >= other.width) {
                outRect.x = other.x;
                outRect.width = other.width;
            } else {
                if (outRect.xMin < other.xMin) {
                    outRect.x += other.xMin - outRect.xMin;
                }

                if (outRect.xMax > other.xMax) {
                    outRect.x += other.xMax - outRect.xMax;
                }
            }

            if (outRect.height >= other.height) {
                outRect.y = other.y;
                outRect.height = other.height;
            } else {
                if (outRect.yMin < other.yMin) {
                    outRect.y += other.yMin - outRect.yMin;
                }

                if (outRect.yMax > other.yMax) {
                    outRect.y += other.yMax - outRect.yMax;
                }
            }

            return outRect.Rounded();
        }

        public static Rect Bounded(this Rect rect, Vector2 size) {
            return Bounded(rect, new Rect(0, 0, size.x, size.y));
        }

        public static Rect Bounded(this Rect rect, float width, float height) {
            return Bounded(rect, new Rect(0, 0, width, height));
        }
    }
}
