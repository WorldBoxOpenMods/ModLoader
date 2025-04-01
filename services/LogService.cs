//#define TEST

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NeoModLoader.constants;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.services;
/// <summary>
/// It is a service to log message to console or Unity Console
/// </summary>
public static class LogService
{
    private enum LogType
    {
        Info,
        Warning,
        Error
    }

    private class WrappedMessage
    {
        public string message;
        public LogType type;

        public WrappedMessage(string message, LogType type)
        {
            this.message = message;
            this.type = type;
        }
        public void Reset(string message, LogType type)
        {
            this.message = message;
            this.type = type;
        }
    }
    private static readonly ConcurrentQueue<WrappedMessage> concurrent_log_queue = new();
    private static ConcurrentBag<WrappedMessage> _pool = new();
    private const int pool_size = 100;

    private class ConcurrentLogHandle : MonoBehaviour
    {
        private void Update()
        {
            int log_count = 0;
            while (log_count <= 32 && concurrent_log_queue.TryDequeue(out WrappedMessage message))
            {
                log_count++;
                switch (message.type)
                {
                    case LogType.Info:
                        LogInfo(message.message);
                        break;
                    case LogType.Warning:
                        LogWarning(message.message);
                        break;
                    case LogType.Error:
                        LogError(message.message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if(_pool.Count >= pool_size) continue;
                _pool.Add(message);
            }
        }
    }
    /// <summary>
    /// Pull all concurrent log to current thread. Often used in unit test.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void PullAllConcurrentLogToCurrentThread()
    {
        while (concurrent_log_queue.TryDequeue(out WrappedMessage message))
        {
            switch (message.type)
            {
                case LogType.Info:
                    LogInfo(message.message);
                    break;
                case LogType.Warning:
                    LogWarning(message.message);
                    break;
                case LogType.Error:
                    LogError(message.message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (_pool.Count >= pool_size) continue;
            _pool.Add(message);
        }
    }
    internal static void Init()
    {
        WorldBoxMod.Transform.gameObject.AddComponent<ConcurrentLogHandle>();
    }
    /// <summary>
    /// Log Info message with [NML] prefix for sub thread
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInfoConcurrent(string message)
    {
        if (_pool.TryTake(out WrappedMessage result))
        {
            result.Reset(message, LogType.Info);
        }
        else
        {
            result = new WrappedMessage(message, LogType.Info);
        }
        concurrent_log_queue.Enqueue(result);
    }
    /// <summary>
    /// Log Warning message with [NML] prefix for sub thread
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarningConcurrent(string message)
    {
        if (_pool.TryTake(out WrappedMessage result))
        {
            result.Reset(message, LogType.Warning);
        }
        else
        {
            result = new WrappedMessage(message, LogType.Warning);
        }
        concurrent_log_queue.Enqueue(result);
    }
    /// <summary>
    /// Log Error message with [NML] prefix for sub thread
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogErrorConcurrent(string message)
    {
        if (_pool.TryTake(out WrappedMessage result))
        {
            result.Reset(message, LogType.Error);
        }
        else
        {
            result = new WrappedMessage(message, LogType.Error);
        }
        concurrent_log_queue.Enqueue(result);
    }
    public static void LogException(Exception exception) {
        if (Others.unity_player_enabled) {
            UnityEngine.Debug.LogException(exception);
        } else {
            System.Console.WriteLine(exception);
        }
    }
    /// <summary>
    /// Log Error message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.LogError("[NML]: " + message);
        }
        else
        {
            System.Console.Error.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log Warning message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.LogWarning("[NML]: " + message);
        }
        else
        {
            System.Console.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log message with [NML] prefix
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInfo(string message)
    {
        if (Others.unity_player_enabled)
        {
            UnityEngine.Debug.Log("[NML]: " + message);
        }
        else
        {
            System.Console.WriteLine("[NML]: " + message);
        }
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Info
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsInfo()
    {
        LogInfo(OtherUtils.GetStackTrace(2));
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Warning
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsWarning()
    {
        LogWarning(OtherUtils.GetStackTrace(2));
    }
    /// <summary>
    /// Log StackTrace from where call this method with [NML] prefix as Error
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogStackTraceAsError()
    {
        LogError(OtherUtils.GetStackTrace(2));
    }
}