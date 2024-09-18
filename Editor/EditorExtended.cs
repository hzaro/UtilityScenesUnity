using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace UtilityEditor
{
    public static class EditorExtended
    {
        //Box
        public const string MINI_BOX = "OL box";
        public const string GROUP_BOX = "GroupBox";
        public const string FRAME_BOX = "FrameBox";
        public const string BLACK_BOX = "LODBlackBox";
        public const string WIZARD_BOX = "Wizard Box";
        public const string CREATE_BOX = "U2D.createRect";
        public const string OUTLINE_BOX = "EyeDropperPickedPixel";
        public const string GENERAL_BOX = "ChannelStripBg";
        public const string FADE_BOX = "AnimationEventBackground";

        //DropDown
        public static string BLACK_DROPDOWN = "PreviewPackageInUse";
        public static string GIZMO_DROPDOWN = "GV Gizmo DropDown";

        //Color
        private static Color s_defaultBackgroundColor;
        public static Color backgroundColor
        {
            get
            {
                if (s_defaultBackgroundColor.a == 0)
                {
                    var method = typeof(EditorGUIUtility)
                        .GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
                    s_defaultBackgroundColor = (Color)method.Invoke(null, null);
                }
                return s_defaultBackgroundColor;
            }
        }
    }
}
