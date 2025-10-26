// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace EFramework.Unity.Utility
{
    #region 基础类型
    /// <summary>
    /// XPrefs 是一个灵活高效的配置系统，实现了多源化配置的读写，支持自定义编辑器、变量求值和命令行参数覆盖等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 多源化配置：支持内置配置（只读）、本地配置（可写）和远端配置（只读），支持多个配置源按优先级顺序读取
    /// - 多数据类型：支持基础类型（整数、浮点数、布尔值、字符串）、数组类型及配置实例（IBase）
    /// - 变量求值：支持通过命令行参数动态覆盖配置项，使用 ${Preferences.Key} 语法引用其他配置项
    /// - 自定义编辑器：通过自定义编辑器实现可视化编辑，支持在保存、应用和构建流程中注入自定义逻辑
    /// 
    /// 使用手册
    /// 1. 基础配置操作
    /// 
    /// 1.1 检查配置项
    ///     // 检查配置项是否存在
    ///     var exists = XPrefs.HasKey("configKey");
    /// 
    /// 1.2 读写基本类型
    ///     // 写入配置
    ///     XPrefs.Local.Set("intKey", 42);
    ///     XPrefs.Local.Set("floatKey", 3.14f);
    ///     XPrefs.Local.Set("boolKey", true);
    ///     XPrefs.Local.Set("stringKey", "value");
    /// 
    ///     // 读取配置
    ///     var intValue = XPrefs.GetInt("intKey", 0);
    ///     var floatValue = XPrefs.GetFloat("floatKey", 0f);
    ///     var boolValue = XPrefs.GetBool("boolKey", false);
    ///     var stringValue = XPrefs.GetString("stringKey", "");
    /// 
    /// 1.3 读写数组类型
    ///     // 写入数组
    ///     XPrefs.Local.Set("intArray", new[] { 1, 2, 3 });
    ///     XPrefs.Local.Set("stringArray", new[] { "a", "b", "c" });
    /// 
    ///     // 读取数组
    ///     var intArray = XPrefs.GetInts("intArray");
    ///     var stringArray = XPrefs.GetStrings("stringArray");
    /// 
    /// 2. 配置源管理
    /// 
    /// 2.1 内置配置（只读）
    ///     // 读取内置配置
    ///     var value = XPrefs.Asset.GetString("key");
    /// 
    /// 2.2 本地配置（可写）
    ///     // 写入本地配置
    ///     XPrefs.Local.Set("key", "value");
    ///     XPrefs.Local.Save();
    /// 
    ///     // 读取本地配置
    ///     var value = XPrefs.Local.GetString("key");
    /// 
    /// 2.3 远端配置（只读）
    ///     
    ///     public class RemoteHandler : XPrefs.IRemote.IHandler
    ///     {
    ///         // 实现远端配置处理器
    ///     }
    /// 
    ///     // 读取远端配置
    ///     RunCoroutine(XPrefs.Remote.Read(new RemoteHandler()));
    /// 
    /// 3. 变量求值
    /// 
    /// 3.1 基本用法
    ///     // 设置配置项
    ///     XPrefs.Local.Set("name", "John");
    ///     XPrefs.Local.Set("greeting", "Hello ${Preferences.name}");
    /// 
    ///     // 解析变量引用
    ///     var result = XPrefs.Local.Eval("${Preferences.greeting}"); // 输出: Hello John
    /// 
    /// 3.2 多级路径
    ///     // 设置嵌套配置
    ///     XPrefs.Local.Set("user.name", "John");
    ///     XPrefs.Local.Set("user.age", 30);
    /// 
    ///     // 使用多级路径引用
    ///     var result = XPrefs.Local.Eval("${Preferences.user.name} is ${Preferences.user.age}");
    /// 
    /// 3.3 构建处理
    ///     支持在构建流程的 `IPreprocessBuildWithReport` 阶段对内置的配置进行变量求值，规则及示例如下：
    ///     {
    ///         "environment_key": "${Environment.ProjectPath}/Build", // 引用环境变量会被求值
    ///         "preferences_key": "${Preferences.environment_key}",   // 引用配置变量会被求值
    ///         "const_key@Const": "${Environment.LocalPath}",         // 标记 @Const 的值不会被求值
    ///         "editor_key@Editor": "editor_value"                    // 标记 @Editor 的配置会被移除
    ///     }
    /// 
    /// 4. 命令行参数
    /// 
    /// 4.1 覆盖配置路径
    ///     --Preferences@Asset=path/to/asset.json    # 覆盖内置配置路径（仅支持编辑器环境）
    ///     --Preferences@Local=path/to/local.json    # 覆盖本地配置路径
    /// 
    /// 4.2 覆盖配置值
    ///     --Preferences@Asset.key=value             # 覆盖内置配置项
    ///     --Preferences@Local.key=value             # 覆盖本地配置项
    ///     --Preferences.key=value                   # 覆盖所有配置源
    /// 
    /// 5. 自定义编辑器
    /// 
    ///     通过自定义编辑器实现可视化编辑，支持在保存、应用和构建流程中注入自定义逻辑：
    /// 
    ///     public class MyPreferencesEditor : XPrefs.IEditor
    ///     {
    ///         // 实现自定义编辑器
    ///     }
    /// 
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XPrefs
    {
        /// <summary>
        /// IBase 是配置基类，提供配置的基本读写和变量替换功能。
        /// </summary>
        public partial class IBase : JSONObject, XObject.Json.IEncoder, XString.IEvaluator
        {
            /// <summary>
            /// File 是配置文件的路径。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual string File { get; set; }

            /// <summary>
            /// Error 表示错误信息。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual string Error { get; set; }

            /// <summary>
            /// Dirty 表示是否有未保存的修改。
            /// </summary>
            [XObject.Json.Exclude]
            public virtual bool Dirty { get; internal set; }

            /// <summary>
            /// writable 表示是否可写。
            /// </summary>
            internal bool writable = true;

            /// <summary>
            /// encrypt 是否加密存储。
            /// </summary>
            internal bool encrypt = false;

            public IBase() : base() { }

            /// <summary>
            /// 使用指定的读写和加密选项初始化配置实例。
            /// </summary>
            /// <param name="writable">是否可写</param>
            /// <param name="encrypt">是否加密存储</param>
            public IBase(bool writable = true, bool encrypt = false)
            {
                this.writable = writable;
                this.encrypt = encrypt;
            }

            /// <summary>
            /// Eval 解析配置中的变量引用，支持 ${Preferences.Key} 语法。
            /// </summary>
            /// <param name="input">包含变量引用的字符串</param>
            /// <returns>替换后的字符串</returns>
            public string Eval(string input)
            {
                var pattern = @"\$\{Preferences\.([^}]+?)\}";
                var visited = new HashSet<string>();

                string ReplaceFunc(Match match)
                {
                    var path = match.Groups[1].Value;
                    if (path.Contains("${")) return $"{match.Value}(Nested)";
                    if (!visited.Add(path)) return $"${{Preferences.{path}}}(Recursive)";
                    try
                    {
                        if (path.Contains('.'))
                        {
                            var paths = path.Split('.');
                            var current = this as JSONNode;
                            for (int i = 0; i < paths.Length - 1; i++)
                            {
                                if (!current.HasKey(paths[i]))
                                {
                                    return $"${{Preferences.{path}}}(Unknown)";
                                }
                                current = current[paths[i]];
                            }
                            if (current.HasKey(paths[^1]))
                            {
                                var value = current[paths[^1]];
                                if (string.IsNullOrEmpty(value)) return $"${{Preferences.{path}}}(Unknown)";
                                return Regex.Replace(value, pattern, ReplaceFunc);
                            }
                        }
                        else if (HasKey(path))
                        {
                            var value = this[path];
                            if (string.IsNullOrEmpty(value)) return $"${{Preferences.{path}}}(Unknown)";
                            return Regex.Replace(value, pattern, ReplaceFunc);
                        }
                        return $"${{Preferences.{path}}}(Unknown)";
                    }
                    finally { visited.Remove(path); }
                }
                return Regex.Replace(input, pattern, ReplaceFunc);
            }

            /// <summary>
            /// Encode 将配置实例编码为 JSON 节点。
            /// </summary>
            /// <returns>JSON 节点</returns>
            public JSONNode Encode() { return Encode(new HashSet<IBase>()); }

            /// <summary>
            /// Encode 是内部的编码方法，处理循环引用。
            /// </summary>
            /// <param name="visited">已访问的实例集合</param>
            /// <returns>JSON 节点</returns>
            internal JSONNode Encode(HashSet<IBase> visited)
            {
                var jobj = new JSONObject();
                if (visited.Add(this))
                {
                    foreach (var kvp in this)
                    {
                        jobj.Add(kvp.Key, kvp.Value);
                    }
                }
                return jobj;
            }

            /// <summary>
            /// Json 将配置实例转换为 JSON 字符串。
            /// </summary>
            /// <param name="pretty">是否格式化输出</param>
            /// <param name="sort">是否根据键名排序</param>
            /// <returns>JSON 字符串</returns>
            public virtual string Json(bool pretty = true, bool sort = true)
            {
                var jobj = Encode();
                if (sort)
                {
                    var keys = new List<string>();
                    foreach (var kvp in jobj) keys.Add(kvp.Key);
                    keys.Sort(); // 按照字母表排序
                    var njobj = new JSONObject();
                    foreach (var key in keys) njobj[key] = jobj[key];
                    return pretty ? njobj.ToString(4) : njobj.ToString();
                }
                else return pretty ? jobj.ToString(4) : jobj.ToString();
            }

            /// <summary>
            /// Read 从文件中读取并解析配置。
            /// </summary>
            /// <param name="file">配置文件路径，为空则使用当前 File 属性</param>
            /// <returns>是否读取成功</returns>
            public virtual bool Read(string file)
            {
                Error = string.Empty;
                File = file;
                for (var i = Count - 1; i >= 0; i--) Remove(i);
                if (string.IsNullOrEmpty(File)) Error = "Null file for instantiating preferences.";
                else if (!XFile.HasFile(File)) Error = $"Non exist file {File} for instantiating preferences.";
                else if (!Parse(encrypt ? XString.Decrypt(XFile.OpenText(File)) : XFile.OpenText(File), out var perror)) Error = perror;
                if (!string.IsNullOrEmpty(Error))
                {
                    if (string.IsNullOrEmpty(File)) XLog.Error($"XPrefs.IBase.Read: {Error}");
                    else XLog.Error($"XPrefs.IBase.Read: load <a href=\"file:///{Path.GetFullPath(File)}\">{Path.GetRelativePath(XEnv.ProjectPath, File)}</a> with error: {Error}");
                }
                return string.IsNullOrEmpty(Error);
            }

            /// <summary>
            /// Parse 解析配置文本。
            /// </summary>
            /// <param name="text">配置文本</param>
            /// <param name="error">错误信息输出</param>
            /// <returns>是否解析成功</returns>
            public virtual bool Parse(string text, out string error)
            {
                Dirty = false;
                error = "";
                try
                {
                    var node = JSON.Parse(text);
                    if (node == null)
                    {
                        error = "Null instance.";
                        return false;
                    }
                    if (node.IsString)
                    {
                        error = "Invalid instance.";
                        return false;
                    }
                    foreach (var kvp in node.AsObject)
                    {
                        Add(kvp.Key, kvp.Value);
                    }
                }
                catch (Exception e)
                {
                    XLog.Panic(e);
                    error = e.Message;
                    return false;
                }
                finally
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Preferences."))
                        {
                            var path = pair.Key["Preferences.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else
                            {
                                this[path] = value;
                            }
                            XLog.Notice($"XPrefs.IBase.Parse: override {path} = {value}.");
                        }
                    }
                }
                error = null;
                return true;
            }

            /// <summary>
            /// Save 保存配置到文件。
            /// </summary>
            /// <param name="pretty">是否格式化输出</param>
            /// <param name="sort">是否根据键名排序</param>
            /// <returns>是否保存成功</returns>
            public virtual bool Save(bool pretty = true, bool sort = true)
            {
                if (!writable)
                {
                    XLog.Error("XPrefs.IBase.Save: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if (string.IsNullOrEmpty(File))
                {
                    XLog.Error("XPrefs.IBase.Save: nil file path.");
                    return false;
                }
                else
                {
#if UNITY_EDITOR
                    var editors = new List<IEditor>();
                    var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IEditor>();
                    foreach (var type in types)
                    {
                        try
                        {
                            IEditor obj = null;
                            if (type.IsSubclassOf(typeof(ScriptableObject))) obj = ScriptableObject.CreateInstance(type) as IEditor;
                            else obj = Activator.CreateInstance(type) as IEditor;
                            if (obj != null) editors.Add(obj);
                        }
                        catch (Exception e) { XLog.Panic(e); }
                    }
                    editors.Sort((e1, e2) => e1.Priority.CompareTo(e2.Priority));
                    foreach (var editor in editors)
                    {
                        if (editor == null) continue;
                        if (!editor.OnSave(this))
                        {
                            XLog.Error("XPrefs.IBase.Save: save preferences to <a href=\"file:///{0}\">{1}</a> failed: {2} handle error.", Path.GetFullPath(File), Path.GetRelativePath(XEnv.ProjectPath, File), editor.GetType().FullName);
                            return false;
                        }
                    }
#endif
                    Dirty = false;
                    var text = Json(pretty, sort);
                    XFile.SaveText(File, encrypt ? XString.Encrypt(text) : text);
                    XLog.Notice("XPrefs.IBase.Save: save preferences to <a href=\"file:///{0}\">{1}</a> succeeded.", Path.GetFullPath(File), Path.GetRelativePath(XEnv.ProjectPath, File));
                    return true;
                }
            }

            /// <summary>
            /// Has 检查键是否存在。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <returns>是否存在</returns>
            public virtual bool Has(string key) { return HasKey(key); }

            /// <summary>
            /// Set 设置配置项的值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="value">配置值</param>
            /// <returns>是否设置成功</returns>
            public virtual bool Set(string key, object value)
            {
                if (!writable)
                {
                    XLog.Error("XPrefs.IBase.Set: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if (value == null) return false;
                var type = value.GetType();
                if (type == typeof(string))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (string)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(int))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (int)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(bool))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (bool)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(float))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (float)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(long))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (long)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(double))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (double)value;
                    Dirty = true;
                    return true;
                }
                else if (type == typeof(byte))
                {
                    if (HasKey(key))
                    {
                        var ovalue = this[key];
                        if (ovalue == value) return false;
                    }
                    this[key] = (byte)value;
                    Dirty = true;
                    return true;
                }
                else if (value is Array arr) // 仅支持二维数组
                {
                    var jarr = new JSONArray();
                    this[key] = jarr;
                    foreach (var ele in arr)
                    {
                        var etype = ele.GetType();
                        if (etype == typeof(string)) jarr.Add((string)ele);
                        else if (etype == typeof(int)) jarr.Add((int)ele);
                        else if (etype == typeof(bool)) jarr.Add((bool)ele);
                        else if (etype == typeof(float)) jarr.Add((float)ele);
                        else if (etype == typeof(long)) jarr.Add((long)ele);
                        else if (etype == typeof(double)) jarr.Add((double)ele);
                        else if (etype == typeof(byte)) jarr.Add((byte)ele);
                    }
                    Dirty = true;
                    return true;
                }
                else if (typeof(IBase).IsAssignableFrom(type))
                {
                    var preferences = value as IBase;
                    if (Has(key))
                    {
                        var ovalue = Get<IBase>(key);
                        if (preferences.Equals(ovalue)) return false;
                    }
                    this[key] = preferences;
                    Dirty = true;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Unset 移除配置项。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <returns>是否移除成功</returns>
            public virtual bool Unset(string key)
            {
                if (!writable)
                {
                    XLog.Error("XPrefs.IBase.Unset: preferences of {0} is readonly.", GetType().FullName);
                    return false;
                }
                if (HasKey(key))
                {
                    Remove(key);
                    Dirty = true;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Get 获取指定类型的配置值。
            /// </summary>
            /// <typeparam name="T">目标类型</typeparam>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>配置值，不存在或类型不匹配时返回默认值</returns>
            public virtual T Get<T>(string key, T defval = default)
            {
                var type = typeof(T);
                if (HasKey(key))
                {
                    var val = this[key];
                    if (val == null) return defval;
                    if (type == typeof(int))
                        return (T)(object)val.AsInt;
                    if (type == typeof(long))
                        return (T)(object)val.AsLong;
                    if (type == typeof(float))
                        return (T)(object)val.AsFloat;
                    if (type == typeof(bool))
                        return (T)(object)val.AsBool;
                    if (type == typeof(string))
                        return (T)(object)val.Value;
                    if (val.IsArray)
                    {
                        var jarr = val.AsArray;
                        var etype = type.GetElementType();
                        if (etype == typeof(int))
                        {
                            var arr = new int[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsInt;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(long))
                        {
                            var arr = new long[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsLong;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(float))
                        {
                            var arr = new float[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsFloat;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(bool))
                        {
                            var arr = new bool[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].AsBool;
                            return (T)(object)arr;
                        }
                        if (etype == typeof(string))
                        {
                            var arr = new string[jarr.Count];
                            for (int i = 0; i < jarr.Count; i++)
                                arr[i] = jarr[i].Value;
                            return (T)(object)arr;
                        }
                    }
                    if (typeof(IBase).IsAssignableFrom(type) && val.Tag == JSONNodeType.Object)
                    {
                        var newval = (IBase)Activator.CreateInstance(type);
                        foreach (var kvp in val)
                        {
                            newval[kvp.Key] = kvp.Value;
                        }
                        return (T)(object)newval;
                    }
                }
                return defval;
            }

            /// <summary>
            /// Gets 获取指定类型的数组配置值。
            /// </summary>
            /// <typeparam name="T">数组元素类型</typeparam>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>数组配置值，不存在或类型不匹配时返回默认值</returns>
            public virtual T[] Gets<T>(string key, T[] defval = null)
            {
                if (HasKey(key))
                {
                    var val = this[key];
                    if (!val.IsArray) return defval;

                    var jsonArr = val.AsArray;
                    var type = typeof(T);

                    if (type == typeof(int))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsInt;
                        return result;
                    }
                    if (type == typeof(long))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsLong;
                        return result;
                    }
                    if (type == typeof(float))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsFloat;
                        return result;
                    }
                    if (type == typeof(bool))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].AsBool;
                        return result;
                    }
                    if (type == typeof(string))
                    {
                        var result = new T[jsonArr.Count];
                        for (int i = 0; i < jsonArr.Count; i++)
                            result[i] = (T)(object)jsonArr[i].Value;
                        return result;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetInt 获取整数配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>整数值，不存在时返回默认值</returns>
            public virtual int GetInt(string key, int defval = 0)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null)
                    {
                        return value.AsInt;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetInts 获取整数数组配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>整数数组，不存在时返回默认值</returns>
            public virtual int[] GetInts(string key, int[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new int[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsInt;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetLong 获取长整数配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>长整数值，不存在时返回默认值</returns>
            public virtual long GetLong(string key, long defval = 0)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsLong;
                }
                return defval;
            }

            /// <summary>
            /// GetLongs 获取长整数数组配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>长整数数组，不存在时返回默认值</returns>
            public virtual long[] GetLongs(string key, long[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new long[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsLong;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetFloat 获取浮点数配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>浮点数值，不存在时返回默认值</returns>
            public virtual float GetFloat(string key, float defval = 0f)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsFloat;
                }
                return defval;
            }

            /// <summary>
            /// GetFloats 获取浮点数数组配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>浮点数数组，不存在时返回默认值</returns>
            public virtual float[] GetFloats(string key, float[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new float[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsFloat;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetBool 获取布尔配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>布尔值，不存在时返回默认值</returns>
            public virtual bool GetBool(string key, bool defval = false)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsBool;
                }
                return defval;
            }

            /// <summary>
            /// GetBools 获取布尔数组配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>布尔数组，不存在时返回默认值</returns>
            public virtual bool[] GetBools(string key, bool[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null && value.IsArray)
                    {
                        var arr = new bool[value.Count];
                        var jarr = value.AsArray;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            arr[i] = jarr[i].AsBool;
                        }
                        return arr;
                    }
                }
                return defval;
            }

            /// <summary>
            /// GetString 获取字符串配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>字符串值，不存在时返回默认值</returns>
            public virtual string GetString(string key, string defval = "")
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.Value;
                }
                return defval;
            }

            /// <summary>
            /// GetStrings 获取字符串数组配置值。
            /// </summary>
            /// <param name="key">配置键</param>
            /// <param name="defval">默认值</param>
            /// <returns>字符串数组，不存在时返回默认值</returns>
            public virtual string[] GetStrings(string key, string[] defval = null)
            {
                if (HasKey(key))
                {
                    var value = this[key];
                    if (value != null) return value.AsStringArray;
                }
                return defval;
            }

            /// <summary>
            /// Equals 比较两个配置实例是否相等。
            /// </summary>
            /// <param name="obj">要比较的实例</param>
            /// <returns>是否相等</returns>
            public override bool Equals(object obj)
            {
                if (obj is not IBase target) return false;
                if (File != target.File) return false;
                if (Count != target.Count) return false;
                foreach (var kvp in this)
                {
                    if (!target.HasKey(kvp.Key)) return false;
                    var val1 = kvp.Value;
                    var val2 = target[kvp.Key];
                    var ret = val1.Equals(val2);
                    if (!ret) return false;
                }
                return true;
            }

            /// <summary>
            /// GetHashCode 获取配置实例的哈希码。
            /// </summary>
            /// <returns>哈希码</returns>
            public override int GetHashCode() { return base.GetHashCode(); }
        }
    }
    #endregion

    #region 内置配置（只读）
    public partial class XPrefs
    {
        /// <summary>
        /// IAsset 是内置配置类，用于管理打包在应用程序中的只读配置。
        /// </summary>
        public partial class IAsset : IBase
#if UNITY_EDITOR
        , UnityEditor.Build.IPreprocessBuildWithReport
#endif
        {
            /// <summary>
            /// Uri 是配置文件的路径。
            /// 编辑器环境优先返回环境变量中设置的 Preferences@Asset 字段，其次返回 EditorPrefs 持久化的值。
            /// 运行时环境下返回 XEnv.AssetPath 目录下的 Preferences.json 文件。
            /// </summary>
            public static string Uri
            {
                get
                {
#if UNITY_EDITOR
                    var path = XEnv.GetArg("Preferences@Asset");
                    if (!string.IsNullOrEmpty(path)) return path;

                    var key = XFile.PathJoin(Path.GetFullPath("./"), "Preferences");
                    return UnityEditor.EditorPrefs.GetString(key);
#else
                    return XFile.PathJoin(XEnv.AssetPath, "Preferences.json");
#endif
                }
                set
                {
#if UNITY_EDITOR
                    var key = XFile.PathJoin(Path.GetFullPath("./"), "Preferences");
                    UnityEditor.EditorPrefs.SetString(key, value);
#endif
                }
            }

            public IAsset() : base(writable: Application.isEditor, encrypt: !Application.isEditor) { } // 公开构造函数以适配 IPreprocessBuildWithReport 事件

            public override bool Parse(string text, out string error)
            {
                var ret = base.Parse(text, out error);

#if !EFRAMEWORK_PREFERENCES_INSECURE
                // 仅编辑器或 Dev/Test 模式支持变量覆盖
                var mode = GetString(XEnv.Preferences.Mode, "");
                if (Application.isEditor || mode == XEnv.ModeDev || mode == XEnv.ModeTest)
#endif
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Preferences@Asset."))
                        {
                            var path = pair.Key["Preferences@Asset.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else this[path] = value;
                            XLog.Notice($"XPrefs.IAsset.Parse: override {path} = {value}.");
                        }
                    }
                }

                return ret;
            }

            public override bool Read(string file)
            {
                if (!base.Read(file)) return false;
#if UNITY_EDITOR
                var editors = new List<IEditor>();
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IEditor>();
                foreach (var type in types)
                {
                    try
                    {
                        IEditor obj = null;
                        if (type.IsSubclassOf(typeof(ScriptableObject))) obj = ScriptableObject.CreateInstance(type) as IEditor;
                        else obj = Activator.CreateInstance(type) as IEditor;
                        if (obj != null) editors.Add(obj);
                    }
                    catch (Exception e) { XLog.Panic(e); }
                }
                editors.Sort((e1, e2) => e1.Priority.CompareTo(e2.Priority));
                foreach (var editor in editors)
                {
                    if (editor == null) continue;
                    if (!editor.OnApply(this))
                    {
                        XLog.Error("XPrefs.IAsset.Read: apply preferences of <a href=\"file:///{0}\">{1}</a> failed: {2} handle error.", Path.GetFullPath(file), Path.GetRelativePath(XEnv.ProjectPath, file), editor.GetType().FullName);
                        return false;
                    }
                }
                XLog.Notice("XPrefs.IAsset.Read: apply preferences of <a href=\"file:///{0}\">{1}</a> succeeded.", Path.GetFullPath(file), Path.GetRelativePath(XEnv.ProjectPath, file));
#endif
                return true;
            }

#if UNITY_EDITOR
            public override bool Save(bool pretty = true, bool sort = true)
            {
                if (File != Uri) File = Uri;
                if (!base.Save(pretty, sort)) return false;
                var editors = new List<IEditor>();
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IEditor>();
                foreach (var type in types)
                {
                    try
                    {
                        IEditor obj = null;
                        if (type.IsSubclassOf(typeof(ScriptableObject))) obj = ScriptableObject.CreateInstance(type) as IEditor;
                        else obj = Activator.CreateInstance(type) as IEditor;
                        if (obj != null) editors.Add(obj);
                    }
                    catch (Exception e) { XLog.Panic(e); }
                }
                editors.Sort((e1, e2) => e1.Priority.CompareTo(e2.Priority));
                foreach (var editor in editors)
                {
                    if (editor == null) continue;
                    if (!editor.OnApply(this))
                    {
                        XLog.Error("XPrefs.IAsset.Save: apply preferences of <a href=\"file:///{0}\">{1}</a> failed: {2} handle error.", Path.GetFullPath(File), Path.GetRelativePath(XEnv.ProjectPath, File), editor.GetType().FullName);
                        return false;
                    }
                }
                XLog.Notice("XPrefs.IAsset.Save: apply preferences of <a href=\"file:///{0}\">{1}</a> succeeded.", Path.GetFullPath(File), Path.GetRelativePath(XEnv.ProjectPath, File));
                return true;
            }
#endif

#if UNITY_EDITOR
            int UnityEditor.Build.IOrderedCallback.callbackOrder => -1;

            void UnityEditor.Build.IPreprocessBuildWithReport.OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
            {
#if !EFRAMEWORK_PREFERENCES_NO_STREAMING
                static void doEval(IBase preferences)
                {
                    var visited = new HashSet<string>();  // 防止循环引用
                    void evalNode(JSONNode node, string path = "")
                    {
                        if (node == null) return;
                        switch (node.Tag)
                        {
                            case JSONNodeType.String:
                                if (!string.IsNullOrEmpty(node.Value) && node.Value.Contains("${"))
                                {
                                    if (!visited.Add(path))
                                    {
                                        XLog.Warn($"XPrefs.IAsset.OnPreprocessBuild: detected recursive reference in {path}.");
                                        return;
                                    }
                                    var value = node.Value.Eval(XEnv.Instance, preferences);
                                    node.Value = value;
                                    visited.Remove(path);
                                }
                                break;
                            case JSONNodeType.Object:
                                foreach (var kvp in node.AsObject)
                                {
                                    if (kvp.Key.Contains("@Const")) continue; // 常量不处理
                                    var childPath = string.IsNullOrEmpty(path) ? kvp.Key : $"{path}.{kvp.Key}";
                                    evalNode(kvp.Value, childPath);
                                }
                                break;
                            case JSONNodeType.Array:
                                for (int i = 0; i < node.Count; i++)
                                {
                                    evalNode(node[i], $"{path}[{i}]");
                                }
                                break;
                        }
                    }

                    var editorKeys = new List<string>(); // 移除编辑器配置
                    foreach (var kvp in preferences)
                    {
                        if (kvp.Key.Contains("@Editor")) editorKeys.Add(kvp.Key);
                    }
                    foreach (var key in editorKeys)
                    {
                        preferences.Unset(key);
                        XLog.Notice("XPrefs.IAsset.OnPreprocessBuild: remove editor prefs: {0}.", key);
                    }

                    evalNode(preferences); // 递归处理所有节点
                }

                if (!XFile.HasFile(Uri)) // 配置不存在
                {
                    throw new UnityEditor.Build.BuildFailedException("XPrefs.IAsset.OnPreprocessBuild: no preferences was found, please apply preferences before build.");
                }

                var preferences = new IBase();
                if (!preferences.Parse(Asset.Json(), out var error)) // 配置解析异常
                {
                    throw new UnityEditor.Build.BuildFailedException("XPrefs.IAsset.OnPreprocessBuild: streaming preferences of <a href=\"file:///{0}\">{1}</a> failed: {2}".Format(Path.GetFullPath(Uri), Path.GetFileName(Uri), error));
                }

                doEval(preferences); // 执行递归求值

                var editors = new List<IEditor>();
                var types = UnityEditor.TypeCache.GetTypesDerivedFrom<IEditor>();
                foreach (var type in types)
                {
                    try
                    {
                        IEditor obj = null;
                        if (type.IsSubclassOf(typeof(ScriptableObject))) obj = ScriptableObject.CreateInstance(type) as IEditor;
                        else obj = Activator.CreateInstance(type) as IEditor;
                        if (obj != null) editors.Add(obj);
                    }
                    catch (Exception e) { XLog.Panic(e); }
                }
                editors.Sort((e1, e2) => e1.Priority.CompareTo(e2.Priority));
                foreach (var editor in editors)
                {
                    if (editor == null) continue;
                    if (editor == null) continue;
                    if (!editor.OnBuild(preferences))
                    {
                        throw new UnityEditor.Build.BuildFailedException("XPrefs.IAsset.OnPreprocessBuild: streaming preferences of <a href=\"file:///{0}\">{1}</a> failed: {2} handle error.".Format(Path.GetFullPath(Uri), Path.GetFileName(Uri), editor.GetType().FullName));
                    }
                }

                XFile.SaveText(XFile.PathJoin(XEnv.AssetPath, "Preferences.json"), preferences.Json(false).Encrypt());
                UnityEditor.AssetDatabase.Refresh();
                XLog.Notice("XPrefs.IAsset.OnPreprocessBuild: streaming preferences of <a href=\"file:///{0}\">{1}</a> succeeded.", Path.GetFullPath(Uri), Path.GetFileName(Uri));
#endif
            }

#if UNITY_INCLUDE_TESTS
            class TestListener : UnityEditor.TestTools.TestRunner.Api.ICallbacks
            {
                public void RunStarted(UnityEditor.TestTools.TestRunner.Api.ITestAdaptor _) { }

                public void RunFinished(UnityEditor.TestTools.TestRunner.Api.ITestResultAdaptor _)
                {
                    Asset.Read(Uri); // 重新初始化内置配置，丢弃测试过程中的修改项
                }

                public void TestStarted(UnityEditor.TestTools.TestRunner.Api.ITestAdaptor _) { }

                public void TestFinished(UnityEditor.TestTools.TestRunner.Api.ITestResultAdaptor _) { }
            }

            [UnityEditor.InitializeOnLoadMethod]
            static void OnInitialize()
            {
                var testRunnerApi = ScriptableObject.CreateInstance<UnityEditor.TestTools.TestRunner.Api.TestRunnerApi>();
                testRunnerApi.RegisterCallbacks(new TestListener());
            }
#endif
#endif
        }

        internal static IAsset asset;
        /// <summary>
        /// Asset 是内置的配置（只读）。
        /// </summary>
        public static IAsset Asset
        {
            get
            {
                if (asset == null)
                {
                    asset = new IAsset();
                    asset.Read(IAsset.Uri);
                }
                return asset;
            }
        }
    }
    #endregion

    #region 本地配置（可写）
    public partial class XPrefs
    {
        /// <summary>
        /// ILocal 是本地配置类，用于管理本地可写配置。
        /// </summary>
        public partial class ILocal : IBase
        {
            /// <summary>
            /// Uri 是配置文件的路径。
            /// 优先返回环境变量中设置的 Preferences@Local 字段，其次返回 XEnv.LocalPath 目录下的 Preferences.json 文件。
            /// </summary>
            public static string Uri
            {
                get
                {
                    var path = XEnv.GetArg("Preferences@Local");
                    if (!string.IsNullOrEmpty(path)) return path;

                    return XFile.PathJoin(XEnv.LocalPath, "Preferences.json");
                }
            }

            internal ILocal() : base(writable: true, encrypt: !(Application.isEditor || XEnv.Mode <= XEnv.ModeType.Test)) { }

            public override bool Parse(string text, out string error)
            {
                var ret = base.Parse(text, out error);

#if !EFRAMEWORK_PREFERENCES_INSECURE
                // 仅编辑器或 Dev/Test 模式支持变量覆盖
                if (Application.isEditor || XEnv.Mode <= XEnv.ModeType.Test)
#endif
                {
                    var args = XEnv.GetArgs();
                    foreach (var pair in args)
                    {
                        if (pair.Key.StartsWith("Preferences@Local."))
                        {
                            var path = pair.Key["Preferences@Local.".Length..];
                            var value = pair.Value.Trim('"');
                            if (path.Contains('.'))
                            {
                                var paths = path.Split('.');
                                var parent = this as JSONObject;
                                for (int i = 0; i < paths.Length - 1; i++)
                                {
                                    var part = paths[i];
                                    if (!parent.HasKey(part))
                                    {
                                        parent[part] = new JSONObject();
                                    }
                                    parent = parent[part].AsObject;
                                }
                                parent[paths[^1]] = value;
                            }
                            else
                            {
                                this[path] = value;
                            }
                            XLog.Notice($"XPrefs.ILocal.Parse: override {path} = {value}.");
                        }
                    }
                }

                return ret;
            }
        }

        internal static ILocal local;
        /// <summary>
        /// Local 是本地的配置（可写）。
        /// </summary>
        public static ILocal Local
        {
            get
            {
                if (local == null)
                {
                    local = new ILocal();
                    if (Application.isPlaying)
                    {
                        Application.quitting += () => local.Save();
                        SceneManager.activeSceneChanged += (_, _) => local.Save();
                    }

                    if (XFile.HasFile(ILocal.Uri)) local.Read(ILocal.Uri);
                    else local.File = ILocal.Uri;
                }
                return local;
            }
        }
    }
    #endregion

    #region 远端配置（只读）
    public partial class XPrefs
    {
        /// <summary>
        /// IRemote 是远端配置类，用于管理从远端服务器获取的配置。
        /// </summary>
        public partial class IRemote : IBase
        {
            internal IRemote() : base(writable: false, encrypt: false) { }

            /// <summary>
            /// IHandler 是读取行为的流程控制器，用于控制各阶段行为。
            /// </summary>
            public interface IHandler
            {
                /// <summary>
                /// Uri 是远端的地址。
                /// </summary>
                string Uri { get; }

                /// <summary>
                /// OnStarted 是流程启动的回调。
                /// </summary>
                /// <param name="context">上下文实例</param>
                void OnStarted(IRemote context);

                /// <summary>
                /// OnRequest 是预请求的回调。
                /// </summary>
                /// <param name="context">上下文实例</param>
                /// <param name="request">HTTP 请求实例</param>
                void OnRequest(IRemote context, UnityWebRequest request);

                /// <summary>
                /// OnRetry 是错误重试的回调。
                /// </summary>
                /// <param name="context">上下文实例</param>
                /// <param name="count">重试次数</param>
                /// <param name="pending">重试等待</param>
                /// <returns></returns>
                bool OnRetry(IRemote context, int count, out float pending);

                /// <summary>
                /// OnSucceeded 是请求成功的回调。
                /// </summary>
                /// <param name="context">上下文实例</param>
                void OnSucceeded(IRemote context);

                /// <summary>
                /// OnFailed 是请求失败的回调。
                /// </summary>
                /// <param name="context">上下文实例</param>
                void OnFailed(IRemote context);
            }

            /// <summary>
            /// Read 从远端地址读取并解析配置。
            /// </summary>
            /// <param name="handler">流程处理器</param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException">参数异常</exception>
            public IEnumerator Read(IHandler handler)
            {
                if (handler == null) throw new ArgumentNullException("handler");

                if (string.IsNullOrEmpty(handler.Uri)) Error = "Null uri for requesting preferences.";
                else
                {
                    var executeCount = 0;
                    while (true)
                    {
                        Error = string.Empty;
                        XLog.Notice("XPrefs.IRemote.Read: requesting <a href=\"{0}\">{1}</a>.", handler.Uri, handler.Uri);
                        if (executeCount == 0) handler.OnStarted(this);

                        executeCount++;

                        using var req = UnityWebRequest.Get(handler.Uri);
                        req.timeout = 10;
                        handler.OnRequest(this, req);
                        yield return req.SendWebRequest();
                        if (req.responseCode == 200)
                        {
                            if (Parse(req.downloadHandler.text, out var perror) == false)
                            {
                                Error = XString.Format("Request preferences succeeded, but parsing failed: {0}, content: {1}", perror, req.downloadHandler.text);
                            }
                        }
                        else Error = XString.Format("Request preferences response: {0}, error: {1}", req.responseCode, req.error);

                        if (string.IsNullOrEmpty(Error) == false)
                        {
                            XLog.Error($"XPrefs.IRemote.Read: request <a href=\"{handler.Uri}\">{handler.Uri}</a> with error: {Error}");
                            if (handler.OnRetry(this, executeCount, out var pending) && pending > 0)
                            {
                                yield return new WaitForSeconds(pending);
                            }
                            else
                            {
                                handler.OnFailed(this);
                                break;
                            }
                        }
                        else
                        {
                            XLog.Notice("XPrefs.IRemote.Read: request and parse preferences succeeded.");
                            handler.OnSucceeded(this);
                            break;
                        }
                    }
                }

                yield return null;
            }

            public override bool Save(bool pretty = true, bool sort = true) { throw new Exception($"{GetType().FullName} is readonly."); }
        }

        internal static IRemote remote;
        /// <summary>
        /// Remote 是远端的配置（只读）。
        /// </summary>
        public static IRemote Remote { get => remote ??= new IRemote(); }
    }
    #endregion

    #region 公开接口（静态）
    public partial class XPrefs
    {
        /// <summary>
        /// HasKey 检查指定键是否存在于配置源中。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>是否存在</returns>
        public static bool HasKey(string key, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.Has(key);
            foreach (var source in sources)
            {
                if (source.Has(key)) return true;
            }
            return false;
        }

        /// <summary>
        /// GetInt 获取整数配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>整数值，不存在时返回默认值</returns>
        public static int GetInt(string key, int defval = 0, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetInt(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetInt(key);
            }
            return defval;
        }

        /// <summary>
        /// GetInts 获取整数数组配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>整数数组，不存在时返回默认值</returns>
        public static int[] GetInts(string key, int[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetInts(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetInts(key);
            }
            return defval;
        }

        /// <summary>
        /// GetLong 获取长整数配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>长整数值，不存在时返回默认值</returns>
        public static long GetLong(string key, long defval = 0, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetLong(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetLong(key);
            }
            return defval;
        }

        /// <summary>
        /// GetLongs 获取长整数数组配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>长整数数组，不存在时返回默认值</returns>
        public static long[] GetLongs(string key, long[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetLongs(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetLongs(key);
            }
            return defval;
        }

        /// <summary>
        /// GetFloat 获取浮点数配置值。
        /// </summary>
        /// <param name="key">配置键。</param>
        /// <param name="defval">默认值。</param>
        /// <param name="sources">配置源列表，按优先级排序。</param>
        /// <returns>浮点数值，不存在时返回默认值。</returns>
        public static float GetFloat(string key, float defval = 0f, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetFloat(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetFloat(key);
            }
            return defval;
        }

        /// <summary>
        /// GetFloats 获取浮点数数组配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>浮点数数组，不存在时返回默认值</returns>
        public static float[] GetFloats(string key, float[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetFloats(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetFloats(key);
            }
            return defval;
        }

        /// <summary>
        /// GetBool 获取布尔配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>布尔值，不存在时返回默认值</returns>
        public static bool GetBool(string key, bool defval = false, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetBool(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetBool(key);
            }
            return defval;
        }

        /// <summary>
        /// GetBools 获取布尔数组配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>布尔数组，不存在时返回默认值</returns>
        public static bool[] GetBools(string key, bool[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetBools(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetBools(key);
            }
            return defval;
        }

        /// <summary>
        /// GetString 获取字符串配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>字符串值，不存在时返回默认值</returns>
        public static string GetString(string key, string defval = "", params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetString(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetString(key);
            }
            return defval;
        }

        /// <summary>
        /// GetStrings 获取字符串数组配置值。
        /// </summary>
        /// <param name="key">配置键</param>
        /// <param name="defval">默认值</param>
        /// <param name="sources">配置源列表，按优先级排序</param>
        /// <returns>字符串数组，不存在时返回默认值</returns>
        public static string[] GetStrings(string key, string[] defval = null, params IBase[] sources)
        {
            if (sources == null || sources.Length == 0 || !Application.isPlaying) return Asset.GetStrings(key, defval);
            foreach (var source in sources)
            {
                if (source.Has(key)) return source.GetStrings(key);
            }
            return defval;
        }
    }
    #endregion

    #region 编辑工具
    public partial class XPrefs
    {
        /// <summary>
        /// IEditor 是配置的编辑器接口，规范了配置的可视化、校验、保存、应用等行为。
        /// </summary>
        public interface IEditor
        {
            /// <summary>
            /// Section 是配置分组的名称。
            /// </summary>
            string Section { get; }

            /// <summary>
            /// Tooltip 是配置分组的提示。
            /// </summary>
            string Tooltip { get; }

            /// <summary>
            /// Foldable 表示是否支持折叠。
            /// </summary>
            bool Foldable { get; }

            /// <summary>
            /// Priority 获取显示的优先级。
            /// </summary>
            int Priority { get; }

            /// <summary>
            /// OnActivate 在面板激活时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文</param>
            /// <param name="rootElement">根元素</param>
            /// <param name="context">配置上下文</param>
            void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement, IBase context);

            /// <summary>
            /// OnVisualize 在面板绘制时调用。
            /// </summary>
            /// <param name="searchContext">搜索上下文</param>
            /// <param name="context">配置上下文</param>
            void OnVisualize(string searchContext, IBase context);

            /// <summary>
            /// OnDeactivate 在面板停用时调用。
            /// </summary>
            /// <param name="context">配置上下文</param>
            void OnDeactivate(IBase context);

            /// <summary>
            /// OnSave 在保存配置时调用。
            /// </summary>
            /// <param name="context">配置上下文</param>
            /// <returns>是否成功</returns>
            bool OnSave(IBase context);

            /// <summary>
            /// OnApply 在应用配置时调用。
            /// </summary>
            /// <param name="context">配置上下文</param>
            /// <returns>是否成功</returns>
            bool OnApply(IBase context);

            /// <summary>
            /// OnBuild 在项目构建时调用。
            /// </summary>
            /// <param name="context">配置上下文</param>
            /// <returns>是否成功</returns>
            bool OnBuild(IBase context);
        }
    }
    #endregion
}
