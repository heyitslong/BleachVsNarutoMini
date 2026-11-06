using System;
using System.Windows.Media;

namespace BleachVsNarutoMini.Engine
{
    public class GameLoop
    {
        private bool _isRunning;
        public event Action? Update;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            CompositionTarget.Rendering += OnRendering;
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (!_isRunning) return;
            Update?.Invoke();
        }
    }
}