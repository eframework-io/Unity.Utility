// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EFramework.Unity.Utility;

/// <summary>
/// TestXPrefs 是 XPrefs 的单元测试。
/// </summary>
public class TestXPrefs
{
    [Test]
    public void Basic()
    {
        #region 1. 基本操作测试
        {
            var preferences = new XPrefs.IBase();
            // 验证不存在的键返回 false
            Assert.That(preferences.Has("nonexistent"), Is.False, "不存在的键应返回 false");

            // 验证设置和检查键值
            preferences.Set("key", "value");
            Assert.That(preferences.Has("key"), Is.True, "设置后的键应该存在");

            // 验证移除键值
            preferences.Unset("key");
            Assert.That(preferences.Has("key"), Is.False, "移除后的键应该不存在");
        }
        #endregion

        #region 2. 基本类型测试
        {
            var preferences = new XPrefs.IBase();

            var basicTests = new (string name, string key, object value, object expected)[]
            {
                ("String", "strKey", "value", "value"),
                ("Int", "intKey", 42, 42),
                ("Bool", "boolKey", true, true),
                ("Float", "floatKey", 3.14f, 3.14f)
            };

            foreach (var (name, key, value, expected) in basicTests)
            {
                preferences.Set(key, value);
                object result = name switch
                {
                    "String" => preferences.GetString(key),
                    "Int" => preferences.GetInt(key),
                    "Bool" => preferences.GetBool(key),
                    "Float" => preferences.GetFloat(key),
                    _ => null
                };
                Assert.That(result, Is.EqualTo(expected), $"{name} 类型的值应正确存储和读取");
            }
        }
        #endregion

        #region 3. IBase对象测试
        {
            var preferences = new XPrefs.IBase();
            var child = new XPrefs.IBase();
            child.Set("stringKey", "childValue");
            child.Set("intKey", 42);
            child.Set("arrayKey", new[] { 1, 2, 3 });

            // 验证嵌套对象的存储
            Assert.That(preferences.Set("childPrefs", child), Is.True, "应成功存储嵌套的配置对象");
            var retrieved = preferences.Get<XPrefs.IBase>("childPrefs");
            Assert.That(retrieved, Is.Not.Null, "应能获取到嵌套的配置对象");
            Assert.That(retrieved.GetString("stringKey"), Is.EqualTo("childValue"), "嵌套对象中的字符串值应正确保存");
            Assert.That(retrieved.GetInt("intKey"), Is.EqualTo(42), "嵌套对象中的整数值应正确保存");
            Assert.That(retrieved.GetInts("arrayKey"), Is.EqualTo(new[] { 1, 2, 3 }), "嵌套对象中的数组应正确保存");

            // 深层嵌套测试
            var grandChild = new XPrefs.IBase();
            grandChild.Set("deepKey", "deepValue");
            child.Set("grandChild", grandChild);

            var deepRetrieved = preferences.Get<XPrefs.IBase>("childPrefs").Get<XPrefs.IBase>("grandChild");
            Assert.That(deepRetrieved, Is.Not.Null, "应能获取到深层嵌套的配置对象");
            Assert.That(deepRetrieved.GetString("deepKey"), Is.EqualTo("deepValue"), "深层嵌套对象中的值应正确保存");
        }
        #endregion

        #region 4. 默认值测试
        {
            var preferences = new XPrefs.IBase();
            // 验证各种类型的默认值返回
            Assert.That(preferences.Get("missing", "default"), Is.EqualTo("default"), "缺失的字符串键应返回默认值");
            Assert.That(preferences.Get("missing", 100), Is.EqualTo(100), "缺失的整数键应返回默认值");
            Assert.That(preferences.Get("missing", true), Is.True, "缺失的布尔键应返回默认值");
            Assert.That(preferences.Get("missing", 1.23f), Is.EqualTo(1.23f), "缺失的浮点数键应返回默认值");

            // 验证数组类型的默认值返回
            Assert.That(preferences.Get("missing", new[] { "default" }), Is.EqualTo(new[] { "default" }), "缺失的字符串数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { 1, 2 }), Is.EqualTo(new[] { 1, 2 }), "缺失的整数数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { 1.1f }), Is.EqualTo(new[] { 1.1f }), "缺失的浮点数数组键应返回默认数组");
            Assert.That(preferences.Get("missing", new[] { true }), Is.EqualTo(new[] { true }), "缺失的布尔数组键应返回默认数组");
        }
        #endregion

        #region 5. 数组类型测试
        {
            var preferences = new XPrefs.IBase();

            var arrayTests = new (string name, string key, object value, object expected)[]
            {
                    ("String Array", "strArray", new[] { "a", "b", "c" }, new[] { "a", "b", "c" }),
                    ("Int Array", "intArray", new[] { 1, 2, 3 }, new[] { 1, 2, 3 }),
                    ("Float Array", "floatArray", new[] { 1.1f, 2.2f, 3.3f }, new[] { 1.1f, 2.2f, 3.3f }),
                    ("Bool Array", "boolArray", new[] { true, false, true }, new[] { true, false, true })
            };

            foreach (var (name, key, value, expected) in arrayTests)
            {
                preferences.Set(key, value);
                object result = name switch
                {
                    "String Array" => preferences.GetStrings(key),
                    "Int Array" => preferences.GetInts(key),
                    "Float Array" => preferences.GetFloats(key),
                    "Bool Array" => preferences.GetBools(key),
                    _ => null
                };
                Assert.That((System.Array)expected, Is.EqualTo((System.Array)result), $"{name} 类型的数组应正确存储和读取");
            }
        }
        #endregion

        #region 6. 相等性测试
        {
            var preferences1 = new XPrefs.IBase();
            preferences1.Set("intKey", 42);
            preferences1.Set("floatKey", 3.14f);
            preferences1.Set("boolKey", true);
            preferences1.Set("stringsKey", new[] { "a", "b", "c" });
            preferences1.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            preferences1.Set("boolsKey", new[] { true, false, true });

            var child1 = new XPrefs.IBase();
            child1.Set("key", "childValue");
            preferences1.Set("child", child1);

            var preferences2 = new XPrefs.IBase();
            preferences2.Set("intKey", 42);
            preferences2.Set("floatKey", 3.14f);
            preferences2.Set("boolKey", true);
            preferences2.Set("stringsKey", new[] { "a", "b", "c" });
            preferences2.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            preferences2.Set("boolsKey", new[] { true, false, true });

            var child2 = new XPrefs.IBase();
            child2.Set("key", "childValue");
            preferences2.Set("child", child2);

            Assert.That(preferences1.Equals(preferences2), Is.True, "具有相同内容的配置对象应该相等");
        }
        #endregion
    }

    [Test]
    public void Sources()
    {
        try
        {
            #region 1. 初始化测试数据
            LogAssert.ignoreFailingMessages = true;
            // 初始化Asset测试数据
            XPrefs.asset = null;
            XPrefs.Asset.writable = true; // 设置为可写
            XPrefs.Asset.Set("intKey", 42);
            XPrefs.Asset.Set("intsKey", new[] { 1, 2, 3 });
            XPrefs.Asset.Set("stringKey", "assetValue");
            XPrefs.Asset.Set("floatKey", 3.14f);
            XPrefs.Asset.Set("boolKey", true);
            XPrefs.Asset.Set("stringsKey", new[] { "a", "b", "c" });
            XPrefs.Asset.Set("floatsKey", new[] { 1.1f, 2.2f, 3.3f });
            XPrefs.Asset.Set("boolsKey", new[] { true, false, true });

            // 初始化Local测试数据
            XPrefs.Local.Set("localIntKey", 100);
            XPrefs.Local.Set("localIntsKey", new[] { 4, 5, 6 });
            XPrefs.Local.Set("localStringKey", "localValue");
            XPrefs.Local.Set("overrideKey", "localOverride");
            #endregion

            #region 2. HasKey测试
            // 验证键存在检查
            Assert.That(XPrefs.HasKey("intKey"), Is.True, "Asset 配置中应存在 intKey");
            Assert.That(XPrefs.HasKey("nonexistentKey"), Is.False, "不存在的键应返回 false");
            Assert.That(XPrefs.HasKey("localIntKey", XPrefs.Local), Is.True, "Local 配置中应存在 localIntKey");
            Assert.That(XPrefs.HasKey("intKey", XPrefs.Local, XPrefs.Asset), Is.True, "多配置源中应能找到 intKey");
            Assert.That(XPrefs.HasKey("nonexistentKey", XPrefs.Local, XPrefs.Asset), Is.False, "多配置源中不存在的键应返回 false");
            #endregion

            #region 3. GetInt测试
            // 验证整数值获取
            Assert.That(XPrefs.GetInt("intKey"), Is.EqualTo(42), "应正确获取 Asset 中的整数值");
            Assert.That(XPrefs.GetInt("localIntKey", 0, XPrefs.Local), Is.EqualTo(100), "应正确获取 Local 中的整数值");
            Assert.That(XPrefs.GetInt("nonexistentKey", 999, XPrefs.Local, XPrefs.Asset), Is.EqualTo(999), "获取不存在的键应返回默认值");
            Assert.That(XPrefs.GetInt("floatKey"), Is.EqualTo(3), "浮点数应正确转换为整数");
            #endregion

            #region 4. GetInts测试
            // 验证整数数组获取
            Assert.That(XPrefs.GetInts("intsKey"), Is.EqualTo(new[] { 1, 2, 3 }), "应正确获取 Asset 中的整数数组");
            Assert.That(XPrefs.GetInts("localIntsKey", null, XPrefs.Local), Is.EqualTo(new[] { 4, 5, 6 }), "应正确获取 Local 中的整数数组");
            Assert.That(XPrefs.GetInts("nonexistentKey", new[] { 7, 8, 9 }, XPrefs.Local, XPrefs.Asset), Is.EqualTo(new[] { 7, 8, 9 }), "获取不存在的数组应返回默认值");
            #endregion

            #region 5. Get基本类型测试
            // 验证基本类型值获取
            Assert.That(XPrefs.GetString("stringKey"), Is.EqualTo("assetValue"), "应正确获取字符串值");
            Assert.That(XPrefs.GetFloat("floatKey"), Is.EqualTo(3.14f), "应正确获取浮点数值");
            Assert.That(XPrefs.GetBool("boolKey"), Is.True, "应正确获取布尔值");
            Assert.That(XPrefs.GetString("overrideKey", "", XPrefs.Local, XPrefs.Asset), Is.EqualTo("localOverride"), "Local 配置应覆盖 Asset 配置");
            #endregion

            #region 6. 类型特定测试
            // 验证各种类型的特定方法
            Assert.That(XPrefs.GetString("stringKey"), Is.EqualTo("assetValue"), "GetString 应正确获取字符串值");
            Assert.That(XPrefs.GetString("nonexistentKey", "default"), Is.EqualTo("default"), "GetString 应返回默认值");
            Assert.That(XPrefs.GetStrings("stringsKey"), Is.EqualTo(new[] { "a", "b", "c" }), "GetStrings 应正确获取字符串数组");
            Assert.That(XPrefs.GetFloat("floatKey"), Is.EqualTo(3.14f), "GetFloat 应正确获取浮点数值");

            var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
            var actualFloats = XPrefs.GetFloats("floatsKey");
            for (int i = 0; i < expectedFloats.Length; i++)
            {
                Assert.That(actualFloats[i], Is.EqualTo(expectedFloats[i]), "GetFloats 应正确获取浮点数数组");
            }

            Assert.That(XPrefs.GetBool("boolKey"), Is.True, "GetBool 应正确获取布尔值");
            Assert.That(XPrefs.GetBools("boolsKey"), Is.EqualTo(new[] { true, false, true }), "GetBools 应正确获取布尔数组");
            #endregion

            #region 7. 边界情况测试
            // 验证边界情况
            Assert.That(XPrefs.GetInt("intKey", 0, null), Is.EqualTo(42), "空配置源列表应默认使用 Asset");
            Assert.That(XPrefs.GetInt("intKey", 0), Is.EqualTo(42), "无配置源应默认使用 Asset");
            #endregion

            #region 8. 类型不匹配测试
            // 验证类型不匹配情况
            XPrefs.Asset.Set("mismatchKey", "not an int");
            Assert.That(XPrefs.GetInt("mismatchKey"), Is.EqualTo(0), "类型不匹配时应返回类型默认值");
            #endregion
        }
        finally
        {
            // 清理测试数据
            XPrefs.Asset.Unset("intKey");
            XPrefs.Asset.Unset("intsKey");
            XPrefs.Asset.Unset("stringKey");
            XPrefs.Asset.Unset("floatKey");
            XPrefs.Asset.Unset("boolKey");
            XPrefs.Asset.Unset("stringsKey");
            XPrefs.Asset.Unset("floatsKey");
            XPrefs.Asset.Unset("boolsKey");
            XPrefs.Asset.Unset("mismatchKey");

            XPrefs.Local.Unset("localIntKey");
            XPrefs.Local.Unset("localIntsKey");
            XPrefs.Local.Unset("localStringKey");
            XPrefs.Local.Unset("overrideKey");
        }
    }

    [Test]
    public void Persist()
    {
        #region 1. 准备测试环境
        LogAssert.ignoreFailingMessages = true;
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs");
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            var testFile = XFile.PathJoin(tmpDir, "test_persist.json");
            var preferences = new XPrefs.IBase();

            // 准备测试数据
            var testData = @"{
                    ""stringKey"": ""stringValue"",
                    ""intKey"": 123,
                    ""boolKey"": true,
                    ""intSliceKey"": [1, 2, 3],
                    ""floatSliceKey"": [1.1, 2.2, 3.3],
                    ""stringSliceKey"": [""a"", ""b"", ""c""],
                    ""boolSliceKey"": [true, false, true]
                }";

            // 写入测试文件
            XFile.SaveText(testFile, testData);
            #endregion

            #region 2. 测试读取配置
            Assert.That(preferences.Read(testFile), Is.True, "Should read file successfully");

            // 验证各种类型的数据
            Assert.That(preferences.GetString("stringKey"), Is.EqualTo("stringValue"), "Should read string value");
            Assert.That(preferences.GetInt("intKey"), Is.EqualTo(123), "Should read int value");
            Assert.That(preferences.GetBool("boolKey"), Is.True, "Should read bool value");
            Assert.That(preferences.GetInts("intSliceKey"), Is.EqualTo(new[] { 1, 2, 3 }), "Should read ints value");

            var expectedFloats = new[] { 1.1f, 2.2f, 3.3f };
            var actualFloats = preferences.GetFloats("floatSliceKey");
            for (int i = 0; i < expectedFloats.Length; i++)
            {
                Assert.That(actualFloats[i], Is.EqualTo(expectedFloats[i]), "Should read floats value");
            }

            Assert.That(preferences.GetStrings("stringSliceKey"), Is.EqualTo(new[] { "a", "b", "c" }), "Should read strings value");
            Assert.That(preferences.GetBools("boolSliceKey"), Is.EqualTo(new[] { true, false, true }), "Should read bools value");
            #endregion

            #region 3. 测试读取不存在的文件
            var nonExistentFile = XFile.PathJoin(tmpDir, "nonexistent.json");
            Assert.That(preferences.Read(nonExistentFile), Is.False, "Should fail reading non-existent file");
            #endregion

            #region 4. 测试读取无效的JSON
            var invalidFile = XFile.PathJoin(tmpDir, "invalid.json");
            XFile.SaveText(invalidFile, "invalid json");
            Assert.That(preferences.Read(invalidFile), Is.False, "Should fail reading invalid JSON");
            #endregion

            #region 5. 测试复杂JSON
            var complexData = @"{
                    ""nullValue"": null,
                    ""emptyObject"": {},
                    ""emptyArray"": [],
                    ""nestedObject"": {
                        ""key"": ""value""
                    },
                    ""mixedArray"": [1, ""two"", true, null]
                }";

            var complexFile = XFile.PathJoin(tmpDir, "complex.json");
            XFile.SaveText(complexFile, complexData);

            var complexPrefs = new XPrefs.IBase();
            Assert.That(complexPrefs.Read(complexFile), Is.True, "Should read complex file successfully");

            Assert.That(complexPrefs.Get<object>("nullValue"), Is.Null, "Should get null value");
            Assert.That(complexPrefs.Get<XPrefs.IBase>("emptyObject"), Is.Not.Null, "Should get empty object");
            Assert.That(complexPrefs.Get<object[]>("emptyArray"), Is.Null, "Should get empty array");
            Assert.That(complexPrefs.Get<XPrefs.IBase>("nestedObject"), Is.Not.Null, "Should get nested object");
            Assert.That(complexPrefs.Get<object[]>("mixedArray"), Is.Null);
            #endregion

            #region 6. 测试大文件
            var largePrefs = new XPrefs.IBase();
            for (int i = 0; i < 1000; i++)
            {
                largePrefs.Set($"key{i}", $"value{i}");
            }

            var largeFile = XFile.PathJoin(tmpDir, "large.json");
            largePrefs.File = largeFile;
            Assert.That(largePrefs.Save(), Is.True, "Should save large file successfully");

            var loadedLargePrefs = new XPrefs.IBase();
            Assert.That(loadedLargePrefs.Read(largeFile), Is.True, "Should read large file successfully");
            Assert.That(loadedLargePrefs.GetString("key42"), Is.EqualTo("value42"), "Should get value42");
            #endregion
        }
        finally
        {
            // 清理测试目录
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
    }

    [Test]
    public void Eval()
    {
        #region 1. 基本替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("name", "John");
            pf.Set("greeting", "Hello ${Preferences.name}");

            var result = pf.Eval("${Preferences.greeting}");
            Assert.That(result, Is.EqualTo("Hello John"), "Should evaluate greeting");
        }
        #endregion

        #region 2. 缺失变量测试
        {
            var pf = new XPrefs.IBase();
            var result = pf.Eval("${Preferences.missing}");
            Assert.That(result, Is.EqualTo("${Preferences.missing}(Unknown)"), "Should evaluate missing");
        }
        #endregion

        #region 3. 递归变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("recursive1", "${Preferences.recursive2}");
            pf.Set("recursive2", "${Preferences.recursive1}");

            var result = pf.Eval("${Preferences.recursive1}");
            Assert.That(result, Is.EqualTo("${Preferences.recursive1}(Recursive)"), "Should evaluate recursive");
        }
        #endregion

        #region 4. 嵌套变量测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("outer", "value");

            var result = pf.Eval("${Preferences.outer${Preferences.inner}}");
            Assert.That(result, Is.EqualTo("${Preferences.outer${Preferences.inner}(Nested)}"), "Should evaluate nested");
        }
        #endregion

        #region 5. 多重替换测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("first", "John");
            pf.Set("last", "Doe");

            var child = new XPrefs.IBase();
            child.Set("name", "Mike");
            pf.Set("child", child);

            var result = pf.Eval("${Preferences.first} and ${Preferences.last} has a child named ${Preferences.child.name} age ${Preferences.child.age}");
            Assert.That(result, Is.EqualTo("John and Doe has a child named Mike age ${Preferences.child.age}(Unknown)"), "Should evaluate multiple");
        }
        #endregion

        #region 6. 空值测试
        {
            var pf = new XPrefs.IBase();
            pf.Set("empty", "");

            var result = pf.Eval("test${Preferences.empty}end");
            Assert.That(result, Is.EqualTo("test${Preferences.empty}(Unknown)end"), "Should evaluate empty");
        }
        #endregion
    }

    [Test]
    public void Override()
    {
        #region 1. 准备测试环境
        LogAssert.ignoreFailingMessages = true;
        var tmpDir = XFile.PathJoin(XEnv.LocalPath, "TestXPrefs-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tmpDir)) XFile.CreateDirectory(tmpDir);

        try
        {
            // 准备配置文件
            var configData = @"{
                    ""key1"": ""value1"",
                    ""key2"": 42
                }";

            var assetFile = XFile.PathJoin(tmpDir, "asset.json");
            var localFile = XFile.PathJoin(tmpDir, "local.json");
            var customLocalFile = XFile.PathJoin(tmpDir, "custom_local.json");

            XFile.SaveText(assetFile, configData);
            XFile.SaveText(localFile, configData);
            XFile.SaveText(customLocalFile, @"{
                    ""customKey"": ""customValue""
                }");

            try
            {
                #region 2. 测试Local配置文件路径覆盖
                XEnv.ParseArgs(true, "--Preferences@Local=" + customLocalFile);
                XPrefs.local = null;
                var local = XPrefs.Local;
                Assert.That(local.File, Is.EqualTo(customLocalFile), "Should set local file");
                Assert.That(local.GetString("customKey"), Is.EqualTo("customValue"), "Should get custom value");
                #endregion

                #region 3. 测试Local配置文件不存在时的行为
                XEnv.ParseArgs(true, "--Preferences@Local=nonexistent.json");
                XPrefs.local = null;
                local = XPrefs.Local;
                Assert.That(local.File, Is.EqualTo("nonexistent.json"), "Should set nonexistent file");
                Assert.That(local.Has("key1"), Is.False, "文件不存在时应该是空配置");
                #endregion

                #region 4. 测试Asset配置文件路径覆盖
                if (Application.isEditor)
                {
                    XEnv.ParseArgs(true, "--Preferences@Asset=" + customLocalFile);

                    XPrefs.asset = null;
                    Assert.That(XPrefs.Asset.File, Is.EqualTo(customLocalFile), "Should set custom file");
                    Assert.That(XPrefs.Asset.GetString("customKey"), Is.EqualTo("customValue"), "Should get custom value");
                }
                #endregion

                #region 5. 测试Asset配置文件不存在时的行为
                if (Application.isEditor)
                {
                    XEnv.ParseArgs(true, "--Preferences@Asset=nonexistent.json");
                    XPrefs.asset = null;
                    Assert.That(XPrefs.Asset.File, Is.EqualTo("nonexistent.json"), "Should set nonexistent file");
                    Assert.That(XPrefs.Asset.Has("key1"), Is.False, "文件不存在时应该是空配置");
                }
                #endregion

                #region 6. 测试Asset和Local参数混合
                XEnv.ParseArgs(true,
                    "--Preferences@Asset.key2=100",
                    "--Preferences@Asset.key3=asset value",
                    "--Preferences@Local.key2=200",
                    "--Preferences@Local.key3=local value",
                    "--Preferences@Local=" + localFile
                );
                XPrefs.local = null;

                var asset = new XPrefs.IAsset();
                Assert.That(asset.Read(assetFile), Is.True, "Should read asset file successfully");
                local = XPrefs.Local;

                // 验证Asset结果
                Assert.That(asset.GetString("key1"), Is.EqualTo("value1"), "原值保持不变");
                Assert.That(asset.GetInt("key2"), Is.EqualTo(100), "被Asset命令行参数覆盖");
                Assert.That(asset.GetString("key3"), Is.EqualTo("asset value"), "Asset新增参数");

                // 验证Local结果
                Assert.That(local.GetString("key1"), Is.EqualTo("value1"), "原值保持不变");
                Assert.That(local.GetInt("key2"), Is.EqualTo(200), "被Local命令行参数覆盖");
                Assert.That(local.GetString("key3"), Is.EqualTo("local value"), "Local新增参数");
                #endregion

                #region 7. 测试多级路径覆盖
                XEnv.ParseArgs(true,
                    "--Preferences.Log.Std.Config.Level=Debug",
                    "--Preferences@Asset.UI.Window.Style.Theme=Dark",
                    "--Preferences@Local.Network.Server.Config.Port=8080",
                    "--Preferences@Local=" + localFile
                );

                XPrefs.local = null;
                asset = new XPrefs.IAsset();
                Assert.That(asset.Read(assetFile), Is.True, "Should read asset file successfully");
                local = XPrefs.Local;

                // 验证Asset多级路径
                var logConfig = asset.Get<XPrefs.IBase>("Log")
                                    .Get<XPrefs.IBase>("Std")
                                    .Get<XPrefs.IBase>("Config");
                Assert.That(logConfig.GetString("Level"), Is.EqualTo("Debug"), "Should get debug level");

                var uiConfig = asset.Get<XPrefs.IBase>("UI")
                                   .Get<XPrefs.IBase>("Window")
                                   .Get<XPrefs.IBase>("Style");
                Assert.That(uiConfig.GetString("Theme"), Is.EqualTo("Dark"), "Should get dark theme");

                // 验证Local多级路径
                var networkConfig = local.Get<XPrefs.IBase>("Network")
                                       .Get<XPrefs.IBase>("Server")
                                       .Get<XPrefs.IBase>("Config");
                Assert.That(networkConfig.GetString("Port"), Is.EqualTo("8080"), "Should get port");
                #endregion

                #region 8. 测试多层覆盖优先级
                XEnv.ParseArgs(true,
                    "--Preferences.sharedKey=base value",
                    "--Preferences@Asset.sharedKey=asset value",
                    "--Preferences@Local.sharedKey=local value",
                    "--Preferences@Local=" + localFile
                );

                XPrefs.local = null;
                asset = new XPrefs.IAsset();
                Assert.That(asset.Read(assetFile), Is.True, "Should read asset file successfully");
                local = XPrefs.Local;

                Assert.That(asset.GetString("sharedKey"), Is.EqualTo("asset value"), "Asset特定覆盖优先");
                Assert.That(local.GetString("sharedKey"), Is.EqualTo("local value"), "Local特定覆盖优先");
                #endregion

                #region 9. 测试Local配置文件和参数覆盖的顺序
                var localData = @"{
                        ""orderKey"": ""file value""
                    }";
                XFile.SaveText(localFile, localData);

                XEnv.ParseArgs(true,
                    "--Preferences@Local.orderKey=override value",
                    "--Preferences@Local=" + localFile
                );

                XPrefs.local = null;
                local = XPrefs.Local;
                Assert.That(local.GetString("orderKey"), Is.EqualTo("override value"), "命令行参数应该优先于文件内容");
                #endregion
            }
            finally
            {
                // 重置测试参数
                XEnv.ParseArgs(true);
                XPrefs.asset = null;
                XPrefs.local = null;
            }
        }
        finally
        {
            if (XFile.HasDirectory(tmpDir))
            {
                XFile.DeleteDirectory(tmpDir, true);
            }
        }
        #endregion
    }

    internal class MyPreferencesEditor : XPrefs.IEditor
    {
        string XPrefs.IEditor.Section => "MyPreferences";

        string XPrefs.IEditor.Tooltip => "This is tooltip of MyPreferences.";

        bool XPrefs.IEditor.Foldable => true;

        int XPrefs.IEditor.Priority => 0;

        void XPrefs.IEditor.OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement, XPrefs.IBase context) { }

        void XPrefs.IEditor.OnVisualize(string searchContext, XPrefs.IBase context) { }

        void XPrefs.IEditor.OnDeactivate(XPrefs.IBase context) { }

        internal static bool onSaveCalled;
        bool XPrefs.IEditor.OnSave(XPrefs.IBase context) { onSaveCalled = true; return true; }

        internal static bool onApplyCalled;
        bool XPrefs.IEditor.OnApply(XPrefs.IBase context) { onApplyCalled = true; return true; }

        internal static bool onBuildCalled;
        bool XPrefs.IEditor.OnBuild(XPrefs.IBase context) { onBuildCalled = true; return true; }
    }

    [Test]
    public void Editor()
    {
        LogAssert.ignoreFailingMessages = true;
        var testDir = XFile.PathJoin(XEnv.ProjectPath, "Temp", "TestXPrefs");
        var lastUri = XPrefs.IAsset.Uri;

        try
        {
            // 准备测试数据
            var streamingFile = XFile.PathJoin(XEnv.AssetPath, "Preferences.json");
            if (XFile.HasFile(streamingFile)) XFile.DeleteFile(streamingFile);
            XPrefs.IAsset.Uri = XFile.PathJoin(testDir, "Preferences.json");

            // 模拟保存行为
            XPrefs.Asset.Set("environment_key", "${Environment.ProjectPath}");
            XPrefs.Asset.Set("preferences_key", "${Preferences.environment_key}");
            XPrefs.Asset.Set("const_key@Const", "${Environment.LocalPath}");
            XPrefs.Asset.Set("editor_key@Editor", "editor_value");
            XPrefs.Asset.Save();

            // 模拟构建行为
            (XPrefs.Asset as UnityEditor.Build.IPreprocessBuildWithReport).OnPreprocessBuild(null);

            // 验证变量求值
            Assert.That(MyPreferencesEditor.onSaveCalled, Is.True, "OnSave 应当被调用。");
            Assert.That(MyPreferencesEditor.onApplyCalled, Is.True, "OnApply 应当被调用。");
            Assert.That(MyPreferencesEditor.onBuildCalled, Is.True, "OnBuild 应当被调用。");
            Assert.That(XFile.HasFile(streamingFile), Is.True, "OnBuild 调用后内置配置文件应当存在。");

            var streamingPreferences = new XPrefs.IBase(encrypt: true);
            streamingPreferences.Read(streamingFile);
            Assert.That(streamingPreferences.GetString("environment_key"), Is.EqualTo(XEnv.ProjectPath), "引用环境变量会被求值。");
            Assert.That(streamingPreferences.GetString("preferences_key"), Is.EqualTo(XEnv.ProjectPath), "引用配置变量会被求值。");
            Assert.That(streamingPreferences.GetString("const_key@Const"), Is.EqualTo("${Environment.LocalPath}"), "标记 @Const 的值不会被求值。");
            Assert.That(streamingPreferences.Has("editor_key@Editor"), Is.False, "标记 @Editor 的配置会被移除。");
        }
        finally
        {
            if (XFile.HasDirectory(testDir)) XFile.DeleteDirectory(testDir); // 删除测试目录
            XPrefs.IAsset.Uri = lastUri; // 恢复配置文件
            LogAssert.ignoreFailingMessages = false;
        }
    }
}
