// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;

namespace EFramework.Unity.Utility
{
    /// <summary>
    /// XEvent 提供了轻量级的事件管理器，支持多重监听、单次及泛型回调和批量通知等功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 多重监听：可配置是否允许同一事件注册多个回调
    /// - 单次回调：可设置回调函数仅执行一次后自动注销
    /// - 泛型回调：支持无参数、单参数和多参数的事件回调
    /// 
    /// 使用手册
    /// 1. 创建事件管理器
    /// 
    ///     // 创建支持多重监听的事件管理器
    ///     var eventManager = new XEvent.Manager(true);
    ///     
    ///     // 创建单一监听的事件管理器
    ///     var singleManager = new XEvent.Manager(false);
    /// 
    /// 2. 注册事件回调
    /// 
    ///     // 注册普通回调
    ///     eventManager.Register(1, (args) => Console.WriteLine("Event 1"));
    ///     
    ///     // 注册单次回调
    ///     eventManager.Register(2, (args) => Console.WriteLine("Once"), true);
    ///     
    ///     // 注册泛型回调
    ///     eventManager.Register&lt;string&gt;(3, (msg) => Console.WriteLine(msg));
    /// 
    /// 3. 通知事件
    /// 
    ///     // 无参数通知
    ///     eventManager.Notify(1);
    ///     
    ///     // 带参数通知
    ///     eventManager.Notify(3, "Hello");
    /// 
    /// 4. 注销事件
    /// 
    ///     // 注销指定回调
    ///     eventManager.Unregister(1, callback);
    ///     
    ///     // 注销所有回调
    ///     eventManager.UnregisterAll();
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XEvent
    {
        /// <summary>
        /// Callback 是事件的回调委托类型。
        /// </summary>
        /// <param name="args">事件参数</param>
        public delegate void Callback(params object[] args);

        /// <summary>
        /// GenericProxy 是委托事件的代理类型。
        /// </summary>
        public class GenericProxy
        {
            /// <summary>
            /// ID 是事件的标识。
            /// </summary>
            public int ID;

            /// <summary>
            /// Callback 是事件的回调。
            /// </summary>
            public Callback Callback;
        }

        /// <summary>
        /// Manager 是事件的管理类。
        /// </summary>
        public class Manager
        {
            /// <summary>
            /// Multiple 表示是否支持多重监听。
            /// </summary>
            protected bool Multiple;

            /// <summary>
            /// Callbacks 记录了事件的回调函数。
            /// </summary>
            protected Dictionary<int, List<Callback>> Callbacks;

            /// <summary>
            /// Onces 记录了回调一次的事件。
            /// </summary>
            protected Dictionary<Callback, bool> Onces;

            /// <summary>
            /// Proxies 维护了事件代理的关系。
            /// </summary>
            protected Dictionary<int, List<GenericProxy>> Proxies;

            /// <summary>
            /// Batches 是批量通知的事件列表。
            /// </summary>
            protected List<Callback> Batches;

            /// <summary>
            /// 构造一个 Manager 事件管理器。
            /// </summary>
            /// <param name="multiple">是否支持多重监听</param>
            public Manager(bool multiple = true)
            {
                Multiple = multiple;
                Callbacks = new Dictionary<int, List<Callback>>();
                Onces = new Dictionary<Callback, bool>();
                Proxies = new Dictionary<int, List<GenericProxy>>();
                Batches = new List<Callback>(64);
            }

            /// <summary>
            /// Get 获取事件回调。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <returns>事件回调</returns>
            public virtual List<Callback> Get(Enum id) { return Callbacks.TryGetValue(id.GetHashCode(), out var callbacks) ? callbacks : null; }

            /// <summary>
            /// Get 获取事件回调。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <returns>事件回调</returns>
            public virtual List<Callback> Get(int id) { return Callbacks.TryGetValue(id, out var callbacks) ? callbacks : null; }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register(Enum id, Callback callback, bool once = false) { return Register(id.GetHashCode(), callback, once); }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register(int id, Callback callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Register: nil callback, id={0}.", id);
                    return false;
                }
                if (Callbacks.TryGetValue(id, out List<Callback> callbacks) == false)
                {
                    callbacks = new List<Callback>();
                    Callbacks.Add(id, callbacks);
                }
                if (Multiple == false && callbacks.Count > 0)
                {
                    XLog.Error("XEvent.Manager.Register: doesn't support multiple registrations, id={0}.", id);
                    return false;
                }
                for (var i = 0; i < callbacks.Count; i++)
                {
                    var temp = callbacks[i];
                    if (temp == callback) return false;
                }
                if (once) Onces[callback] = once;
                callbacks.Add(callback);
                return true;
            }

            /// <summary>
            /// 注册事件
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register(Enum id, Action callback, bool once = false) { return Register(id.GetHashCode(), callback, once); }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register(int id, Action callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Register: nil callback, id={0}.", id);
                    return false;
                }
                var ncallback = new Callback(args => callback?.Invoke());
                var ret = Register(id, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(id)) Proxies.Add(id, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[id].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1>(Enum id, Action<T1> callback, bool once = false) { return Register(id.GetHashCode(), callback, once); }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1>(int id, Action<T1> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Register: nil callback, id={0}.", id);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    callback?.Invoke(arg1);
                });
                var ret = Register(id, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(id)) Proxies.Add(id, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[id].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1, T2>(Enum id, Action<T1, T2> callback, bool once = false) { return Register(id.GetHashCode(), callback, once); }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1, T2>(int id, Action<T1, T2> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Register: nil callback, id={0}.", id);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    callback?.Invoke(arg1, arg2);
                });
                var ret = Register(id, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(id)) Proxies.Add(id, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[id].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1, T2, T3>(Enum id, Action<T1, T2, T3> callback, bool once = false) { return Register(id.GetHashCode(), callback, once); }

            /// <summary>
            /// Register 注册事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Register<T1, T2, T3>(int id, Action<T1, T2, T3> callback, bool once = false)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Register: nil callback, id={0}.", id);
                    return false;
                }
                var ncallback = new Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    var arg3 = args != null && args.Length > 2 ? (T3)args[2] : default;
                    callback?.Invoke(arg1, arg2, arg3);
                });
                var ret = Register(id, ncallback, once);
                if (ret)
                {
                    if (!Proxies.ContainsKey(id)) Proxies.Add(id, new List<GenericProxy>());
                    var proxy = new GenericProxy
                    {
                        ID = callback.GetHashCode(),
                        Callback = ncallback
                    };
                    Proxies[id].Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister(Enum id, Callback callback = null) { return Unregister(id.GetHashCode(), callback); }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister(int id, Callback callback = null)
            {
                var ret = false;
                if (Callbacks.TryGetValue(id, out var callbacks))
                {
                    if (callback != null)
                    {
                        if (callbacks.Count > 0)
                        {
                            ret = callbacks.Remove(callback);
                            if (callbacks.Count == 0) Callbacks.Remove(id);
                        }
                        if (Onces.ContainsKey(callback)) Onces.Remove(callback);
                    }
                    else
                    {
                        ret = true;
                        for (var i = 0; i < callbacks.Count; i++)
                        {
                            var temp = callbacks[i];
                            if (Onces.ContainsKey(temp)) Onces.Remove(temp);
                        }
                        Callbacks.Remove(id);
                    }
                }
                return ret;
            }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister(Enum id, Action callback) { return Unregister(id.GetHashCode(), callback); }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="callback">回调函数</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister(int id, Action callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unregister: nil callback, id={0}.", id);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(id, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unregister(id, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1>(Enum id, Action<T1> callback) { return Unregister(id.GetHashCode(), callback); }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1>(int id, Action<T1> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unregister: nil callback, id={0}.", id);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(id, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unregister(id, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1, T2>(Enum id, Action<T1, T2> callback) { return Unregister(id.GetHashCode(), callback); }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1, T2>(int id, Action<T1, T2> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unregister: nil callback, id={0}.", id);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(id, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unregister(id, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1, T2, T3>(Enum id, Action<T1, T2, T3> callback) { return Unregister(id.GetHashCode(), callback); }

            /// <summary>
            /// Unregister 注销事件。
            /// </summary>
            /// <typeparam name="T1">事件参数1</typeparam>
            /// <typeparam name="T2">事件参数2</typeparam>
            /// <typeparam name="T3">事件参数3</typeparam>
            /// <param name="id">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unregister<T1, T2, T3>(int id, Action<T1, T2, T3> callback)
            {
                if (callback == null)
                {
                    XLog.Error("XEvent.Manager.Unregister: nil callback, id={0}.", id);
                    return false;
                }
                var ret = false;
                if (Proxies.TryGetValue(id, out var proxies))
                {
                    for (int i = 0; i < proxies.Count;)
                    {
                        var proxy = proxies[i];
                        if (callback == null || proxy.ID == callback.GetHashCode())
                        {
                            proxies.RemoveAt(i);
                            if (Unregister(id, proxy.Callback)) ret = true;
                        }
                        else i++;
                    }
                }
                return ret;
            }

            /// <summary>
            /// UnregisterAll 清除事件注册。
            /// </summary>
            public virtual void UnregisterAll() { Callbacks.Clear(); Onces.Clear(); Proxies.Clear(); Batches.Clear(); }

            /// <summary>
            /// Notify 通知事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(Enum id, params object[] args) { Notify(id.GetHashCode(), args); }

            /// <summary>
            /// Notify 通知事件。
            /// </summary>
            /// <param name="id">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(int id, params object[] args)
            {
                if (Callbacks.TryGetValue(id, out var callbacks))
                {
                    if (callbacks != null && callbacks.Count > 0)
                    {
                        Batches.Clear();
                        for (int i = 0; i < callbacks.Count;)
                        {
                            var callback = callbacks[i];
                            if (callback == null) callbacks.RemoveAt(i);
                            else
                            {
                                if (Onces.ContainsKey(callback))
                                {
                                    Onces.Remove(callback);
                                    callbacks.RemoveAt(i);
                                }
                                else i++;
                                Batches.Add(callback);
                            }
                        }
                        for (int i = 0; i < Batches.Count; i++)
                        {
                            var callback = Batches[i];
                            callback?.Invoke(args);
                        }
                        Batches.Clear(); // release references
                    }
                }
            }
        }
    }
}