// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Threading.Tasks;
using System.Collections.Generic;
using EFramework.Unity.Utility;
using System.Collections;
using System.Text.RegularExpressions;

/// <summary>
/// TestXPool 是 XPool 的单元测试。
/// </summary>
public class TestXPool
{
    [Test]
    public void Object()
    {
        // 测试基本的Get/Put功能
        var obj1 = XPool.Object<List<int>>.Get();
        Assert.That(obj1, Is.Not.Null, "从对象池获取的实例不应为空");
        obj1.Add(1);
        XPool.Object<List<int>>.Put(obj1);

        // 测试对象复用
        var obj2 = XPool.Object<List<int>>.Get();
        Assert.That(obj2, Is.SameAs(obj1), "对象池应返回之前缓存的同一个实例");
        Assert.That(obj2, Has.Count.EqualTo(1), "复用的对象应保持原有状态");

        // 测试池子上限
        var objects = new List<List<int>>();
        for (int i = 0; i < XPool.Object<List<int>>.PoolMax + 10; i++)
        {
            objects.Add(XPool.Object<List<int>>.Get());
        }
        objects.ForEach(XPool.Object<List<int>>.Put);
        Assert.That(XPool.Object<List<int>>.pools, Has.Count.LessThanOrEqualTo(XPool.Object<List<int>>.PoolMax), "对象池数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 1000;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var obj = XPool.Object<List<int>>.Get();
                    Assert.That(obj, Is.Not.Null, "多线程环境下从对象池获取的实例不应为空");
                    XPool.Object<List<int>>.Put(obj);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }

    [UnityTest]
    public IEnumerator GameObject()
    {
        // 创建测试预制体
        var obj = new GameObject("TestObject");
        var key = "test_object";

        // 测试Set功能
        var onSet = XPool.GameObject.OnSet;
        string onSetKey = null;
        GameObject onSetOrigin = null;
        XPool.GameObject.CacheType onSetCache = XPool.GameObject.CacheType.Scene;
        XPool.GameObject.OnSet = new System.Func<string, GameObject, XPool.GameObject.CacheType, GameObject>((key, origin, cache) =>
        {
            onSetKey = key;
            onSetOrigin = origin;
            onSetCache = cache;
            return origin;
        });
        Assert.That(XPool.GameObject.Set(key, obj, XPool.GameObject.CacheType.Global), Is.True, "设置预制体到全局对象池应成功");
        Assert.That(XPool.GameObject.Has(key), Is.True, "对象池中应能找到已设置的预制体");
        Assert.That(onSetKey, Is.EqualTo(key), "设置预制体的钩子函数 key 参数应当和传入的相等");
        Assert.That(onSetOrigin, Is.SameAs(obj), "设置预制体的钩子函数 origin 参数应当和传入的相等");
        Assert.That(onSetCache, Is.EqualTo(XPool.GameObject.CacheType.Global), "设置预制体的钩子函数 cache 参数应当和传入的相等");
        XPool.GameObject.OnSet = onSet;

        LogAssert.Expect(LogType.Error, new Regex(Regex.Escape("XPool.GameObject.Set: key is null.")));
        Assert.That(XPool.GameObject.Set(null), Is.False, "设置空键的对象池应当不成功");
        LogAssert.Expect(LogType.Error, new Regex(Regex.Escape("XPool.GameObject.Set: key is null.")));
        Assert.That(XPool.GameObject.Set(""), Is.False, "设置空键的对象池应当不成功");
        Assert.That(XPool.GameObject.Set(key), Is.False, "重复设置相同键的对象池应当不成功");
        Assert.That(XPool.GameObject.Set(key + "2", null), Is.False, "设置空对象的对象池应当不成功");

        // 测试Get功能
        var obj1 = XPool.GameObject.Get(key);
        Assert.That(obj1, Is.Not.Null, "从对象池实例化的游戏对象不应为空");
        Assert.That(obj1.name, Is.EqualTo(obj.name), "实例化的对象名称应与预制体一致");

        // 测试Put功能
        XPool.GameObject.Put(obj1);

        // 测试对象复用
        var obj2 = XPool.GameObject.Get(key);
        Assert.That(obj2, Is.SameAs(obj1), "对象池应返回之前回收的同一个游戏对象实例");
        XPool.GameObject.Put(obj2);

        // 测试自动回收
        var obj3 = XPool.GameObject.Get(key, life: 500);
        yield return new WaitForSeconds(1);
        Assert.That(obj3.activeSelf, Is.False, "对象池应自动回收游戏对象实例");

        // 测试延迟回收
        var obj4 = XPool.GameObject.Get(key);
        XPool.GameObject.Put(obj4, delay: 500);
        yield return new WaitForSeconds(1);
        Assert.That(obj4.activeSelf, Is.False, "对象池应延迟回收游戏对象实例");

        // 测试Del功能
        Assert.That(XPool.GameObject.Unset(key), Is.True, "从对象池中删除预制体应成功");
        Assert.That(XPool.GameObject.Has(key), Is.False, "删除后对象池中不应再存在该预制体");

        // 清理
        UnityEngine.Object.Destroy(obj);
        UnityEngine.Object.Destroy(obj1);
    }

    [Test]
    public void StreamBuffer()
    {
        // 测试Get创建新对象
        var buffer1 = XPool.StreamBuffer.Get(1024);
        Assert.That(buffer1, Is.Not.Null, "从缓冲池获取的字节流不应为空");
        Assert.That(buffer1.Capacity, Is.EqualTo(1024), "字节流容量应与请求的大小一致");
        Assert.That(buffer1.Length, Is.EqualTo(0), "新创建的字节流长度应为0");
        Assert.That(buffer1.Position, Is.EqualTo(0), "新创建的字节流位置应为0");

        // 测试写入和Flush
        var testData = new byte[] { 1, 2, 3, 4 };
        buffer1.Writer.Write(testData);
        Assert.That(buffer1.Position, Is.EqualTo(4), "写入数据后流位置应等于写入的数据长度");
        buffer1.Flush();
        Assert.That(buffer1.Length, Is.EqualTo(4), "Flush后流长度应等于最后写入位置");
        Assert.That(buffer1.Position, Is.EqualTo(0), "Flush后流位置应重置为0");

        // 测试Put和对象池
        var originalBuffer = buffer1.Buffer;
        XPool.StreamBuffer.Put(buffer1);
        var buffer2 = XPool.StreamBuffer.Get(1024);
        Assert.That(buffer2, Is.SameAs(buffer1), "缓冲池应返回之前缓存的同一个字节流实例");
        Assert.That(buffer2.Length, Is.EqualTo(-1), "复用的字节流长度应被重置为-1");
        Assert.That(buffer2.Position, Is.EqualTo(0), "复用的字节流位置应被重置为0");

        // 测试获取更大容量的buffer
        var buffer3 = XPool.StreamBuffer.Get(2048);
        Assert.That(buffer3, Is.Not.SameAs(buffer1), "请求更大容量时应创建新的字节流实例");
        Assert.That(buffer3.Capacity, Is.EqualTo(2048), "新字节流容量应与请求的大小一致");

        // 测试ByteMax限制
        var largeBuffer = XPool.StreamBuffer.Get(XPool.StreamBuffer.ByteMax + 1);
        var largeBufferArray = largeBuffer.Buffer;
        XPool.StreamBuffer.Put(largeBuffer);
        var newLargeBuffer = XPool.StreamBuffer.Get(XPool.StreamBuffer.ByteMax + 1);
        Assert.That(newLargeBuffer, Is.Not.SameAs(largeBuffer), "超过最大字节限制的缓冲不应被复用");

        // 测试PoolMax限制
        var buffers = new List<XPool.StreamBuffer>();
        for (int i = 0; i < XPool.StreamBuffer.PoolMax + 10; i++)
        {
            buffers.Add(XPool.StreamBuffer.Get(1024));
        }
        buffers.ForEach(XPool.StreamBuffer.Put);
        Assert.That(XPool.StreamBuffer.buffers, Has.Count.LessThanOrEqualTo(XPool.StreamBuffer.PoolMax), "缓冲池中的实例数量不应超过设定的上限值");

        // 测试多线程安全性
        var tasks = new List<Task>();
        var threadCount = 10;
        var operationsPerThread = 100;
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    var buffer = XPool.StreamBuffer.Get(1024);
                    buffer.Writer.Write(j);
                    buffer.Flush();
                    XPool.StreamBuffer.Put(buffer);
                }
            }));
        }
        Task.WaitAll(tasks.ToArray());
    }
}
