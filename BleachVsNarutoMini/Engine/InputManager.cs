using System.Collections.Generic;
using System.Windows.Input;

namespace BleachVsNarutoMini.Engine
{
    public static class InputManager
    {
        private static readonly HashSet<Key> PressedKeys = new HashSet<Key>();

        public static void KeyDown(Key key)
        {
            if (!PressedKeys.Contains(key))
                PressedKeys.Add(key);
        }

        public static void KeyUp(Key key)
        {
            if (PressedKeys.Contains(key))
                PressedKeys.Remove(key);
        }

        public static bool IsKeyDown(Key key)
        {
            return PressedKeys.Contains(key);
        }
    }
}