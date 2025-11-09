// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using UnityEngine;

namespace EFramework.Unity.Utility
{
    public partial class XLog
    {
        /// <summary>
        /// StdAdapter 是日志标准输出适配器，实现日志的控制台输出功能。
        /// 支持日志着色和级别过滤等特性。
        /// </summary>
        internal partial class StdAdapter : IAdapter
        {
            /// <summary>
            /// logBrush 是日志的着色器，用于生成带颜色的日志文本。
            /// </summary>
            /// <param name="color">颜色值</param>
            /// <returns>着色函数</returns>
            internal static Func<string, string> logBrush(string color) { return (text) => $"<color={color}><b>{text}</b></color>"; }

            /// <summary>
            /// logBrushes 是日志级别对应的着色函数数组。
            /// </summary>
            internal static readonly Func<string, string>[] logBrushes = new Func<string, string>[] {
                logBrush("black"), // Emergency
                logBrush("cyan"), // Alert
                logBrush("magenta"), // Critical
                logBrush("red"), // Error
                logBrush("yellow"), // Warn
                logBrush("green"), // Notice
                logBrush("grey"), // Info
                logBrush("blue"), // Debug
            };

            /// <summary>
            /// level 是日志输出的级别。
            /// </summary>
            internal LevelType level;

            /// <summary>
            /// colored 表示是否启用日志着色。
            /// </summary>
            internal bool colored;

            /// <summary>
            /// Initialize 初始化标准输出适配器。
            /// </summary>
            /// <param name="preferences">配置参数</param>
            /// <returns>日志输出级别</returns>
            public LevelType Initialize(XPrefs.IBase preferences)
            {
                if (preferences == null) return LevelType.Undefined;
                if (!Enum.TryParse(preferences.GetString(Preferences.Level, Preferences.LevelDefault), out level))
                {
                    level = LevelType.Undefined;
                }
                colored = preferences.GetBool(Preferences.Color, Preferences.ColorDefault);
                return level;
            }

            /// <summary>
            /// Write 写入日志数据。
            /// </summary>
            /// <param name="data">日志数据</param>
            public void Write(LogData data)
            {
                try
                {
                    if (data == null) return;
                    if (data.Level > level && !data.Force) return;
                    if (data.Level == LevelType.Emergency && data.Data is Exception exception)
                    {
                        Handler.Default.LogException(exception, null);
                    }
                    else
                    {
                        var text = data.Text(true);
                        if (colored && !batchMode)
                        {
                            var idx = (int)data.Level;
                            text = text.Replace(logLabels[idx], logBrushes[idx](logLabels[idx]));
                        }

                        var timeStr = XTime.Format(data.Time, "MM/dd HH:mm:ss.fff");
                        var fullText = $"[{timeStr}] {text}";

                        if (data.Level == LevelType.Emergency) Handler.Default.LogFormat(LogType.Exception, null, "{0}", fullText);
                        else if (data.Level <= LevelType.Error) Handler.Default.LogFormat(LogType.Error, null, "{0}", fullText);
                        else Handler.Default.LogFormat(LogType.Log, null, "{0}", fullText);
                    }
                }
                catch (Exception e) { Handler.Default.LogException(e, null); }
                finally { LogData.Put(data); }
            }

            /// <summary>
            /// Flush 刷新日志缓冲区。
            /// </summary>
            public void Flush() { }

            /// <summary>
            /// Close 关闭日志适配器。
            /// </summary>
            public void Close() { }
        }

        internal partial class StdAdapter
        {
            public class Preferences : XPrefs.IEditor
            {
                public const string Config = "XLog/Std";

                public static readonly XPrefs.IBase ConfigDefault = new();

                public const string Level = "Level";

                public static readonly string LevelDefault = LevelType.Info.ToString();

                public const string Color = "Color";

                public static readonly bool ColorDefault = true;

                string XPrefs.IEditor.Section => "XLog";

                string XPrefs.IEditor.Tooltip => string.Empty;

                bool XPrefs.IEditor.Foldable => true;

                int XPrefs.IEditor.Priority => 10;

                [SerializeField] protected bool foldout = true;

                void XPrefs.IEditor.OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement, XPrefs.IBase context) { }

                void XPrefs.IEditor.OnVisualize(string searchContext, XPrefs.IBase context)
                {
#if UNITY_EDITOR
                    var config = context.Get(Config, ConfigDefault);
                    UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                    foldout = UnityEditor.EditorGUILayout.Foldout(foldout, new GUIContent("Std", "Standard Output Adapter."));
                    if (foldout)
                    {
                        UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Level", "Log Level."), GUILayout.Width(60));
                        Enum.TryParse<LevelType>(config.GetString(Level, LevelDefault), out var levelType);
                        config.Set(Level, UnityEditor.EditorGUILayout.EnumPopup("", levelType).ToString());

                        GUILayout.Label(new GUIContent("Color", "Enable Colored Log."), GUILayout.Width(60));
                        config.Set(Color, UnityEditor.EditorGUILayout.Toggle("", config.GetBool(Color, ColorDefault)));
                        UnityEditor.EditorGUILayout.EndHorizontal();
                        UnityEditor.EditorGUILayout.EndVertical();
                    }
                    UnityEditor.EditorGUILayout.EndVertical();
                    if (!context.Has(Config) || config.Dirty) context.Set(Config, config);
#endif
                }

                void XPrefs.IEditor.OnDeactivate(XPrefs.IBase context) { }

                bool XPrefs.IEditor.OnSave(XPrefs.IBase context) { return true; }

                bool XPrefs.IEditor.OnApply(XPrefs.IBase context) { return true; }

                bool XPrefs.IEditor.OnBuild(XPrefs.IBase context) { return true; }
            }
        }
    }
}
