namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEditor;
    using Debug = UnityEngine.Debug;

    public static class EditorCoroutineHelper
    {
        private static readonly List<(Action Action, float Time)> _currentActions = new List<(Action Action, float Time)>();

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

            _currentActions.Add((action, timeInSeconds));
        }

        private static void Update()
        {
            float timeSinceLastFrameMilliseconds = _stopwatch.ElapsedMilliseconds - _elapsedTime;
            _elapsedTime = _stopwatch.ElapsedMilliseconds;

            int i = 0;

            while (i < _currentActions.Count)
            {
                (Action action, float time) = _currentActions[i];
                time -= timeSinceLastFrameMilliseconds / 1000f;

                if (time > 0f)
                {
                    _currentActions[i] = (action, time);
                    i++;
                    continue;
                }

                action.Invoke();
                _currentActions.RemoveAt(i);
            }
        }
    }
}