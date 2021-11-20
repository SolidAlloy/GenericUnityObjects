namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEditor;
    using Debug = UnityEngine.Debug;

    public static class EditorCoroutineHelper
    {
        private static readonly List<(Action Action, float Time)> _currentTimeActions = new List<(Action Action, float Time)>();
        private static readonly List<(Action Action, int Frames)> _currentFrameActions = new List<(Action Action, int Frames)>();

        private static readonly Stopwatch _stopwatch;
        private static long _elapsedTime;

        static EditorCoroutineHelper()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            EditorApplication.update += Update;
        }

        public static void Delay(Action action, float timeInSeconds)
        {
            if (timeInSeconds <= 0f)
            {
                action.Invoke();
                return;
            }

            _currentTimeActions.Add((action, timeInSeconds));
        }

        public static void DelayFrames(Action action, int frames)
        {
            if (frames <= 0)
            {
                action.Invoke();
                return;
            }

            _currentFrameActions.Add((action, frames));
        }

        private static void Update()
        {
            InvokeTimeActions();
            InvokeFrameActions();
        }

        private static void InvokeTimeActions()
        {
            float timeSinceLastFrameMilliseconds = _stopwatch.ElapsedMilliseconds - _elapsedTime;
            _elapsedTime = _stopwatch.ElapsedMilliseconds;

            int i = 0;

            while (i < _currentTimeActions.Count)
            {
                (Action action, float time) = _currentTimeActions[i];
                time -= timeSinceLastFrameMilliseconds / 1000f;

                if (time > 0f)
                {
                    _currentTimeActions[i] = (action, time);
                    i++;
                    continue;
                }

                action.Invoke();
                _currentTimeActions.RemoveAt(i);
            }
        }

        private static void InvokeFrameActions()
        {
            int i = 0;

            while (i < _currentFrameActions.Count)
            {
                (Action action, int frames) = _currentFrameActions[i];
                frames--;

                if (frames > 0)
                {
                    _currentFrameActions[i] = (action, frames);
                    i++;
                    continue;
                }

                action.Invoke();
                _currentFrameActions.RemoveAt(i);
            }
        }
    }
}