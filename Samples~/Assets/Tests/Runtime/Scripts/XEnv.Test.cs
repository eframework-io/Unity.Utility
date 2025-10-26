// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;
using EFramework.Unity.Utility;

/// <summary>
/// TestXEnv 是 XEnv 的单元测试。
/// </summary>
public class TestXEnv
{
    /// <summary>
    /// 清理测试环境。
    /// </summary>
    [OneTimeTearDown]
    public void Cleanup() { XEnv.ParseArgs(reset: true); }

    /// <summary>
    /// 测试初始化。
    /// </summary>
    [Test]
    public void OnInitialize()
    {
        var envFile = XFile.PathJoin(XEnv.ProjectPath, ".env");
        var tempEnvFile = XFile.PathJoin(XEnv.ProjectPath, ".env.temp");
        try
        {
            if (XFile.HasFile(envFile))
            {
                File.Move(envFile, tempEnvFile);
                XFile.DeleteFile(envFile);
            }
            File.AppendAllLines(envFile, new string[] { " key1 = value1 ", "key2=", "key3", "# key4=value4" });
            XEnv.OnInitialize();

            Assert.AreEqual("value1", Environment.GetEnvironmentVariable("key1"), "环境变量应正确解析");
            Assert.IsNull(Environment.GetEnvironmentVariable("key2"), "未赋值的环境变量应当解析为空");
            Assert.IsNull(Environment.GetEnvironmentVariable("key3"), "非法的变量不应当被解析");
            Assert.IsNull(Environment.GetEnvironmentVariable("key4"), "注释的变量不应当被解析");
        }
        catch (Exception e) { throw e; }
        finally
        {
            if (XFile.HasFile(envFile)) XFile.DeleteFile(envFile);
            if (XFile.HasFile(tempEnvFile)) File.Move(tempEnvFile, envFile);
            Environment.SetEnvironmentVariable("key1", string.Empty);
            XEnv.OnInitialize();
        }
    }

    /// <summary>
    /// 测试环境元数据的有效性。
    /// </summary>
    [Test]
    public void Metas()
    {
        Assert.IsTrue(XEnv.Platform != XEnv.PlatformType.Unknown, "平台类型不应为未知");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.DeviceID), "设备标识符不应为空");
        // Assert.IsFalse(string.IsNullOrEmpty(XEnv.MacAddr), "MAC 地址不应为空"); // 注意：Ubuntu、iOS等平台下有可能获取不到，这里不进行测试
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Solution), "解决方案名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Project), "项目名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Product), "产品名称不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Channel), "发布渠道不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Version), "版本号不应为空");
        Assert.IsFalse(string.IsNullOrEmpty(XEnv.Author), "作者名称不应为空");
    }

    /// <summary>
    /// 测试环境配置的默认值。
    /// </summary>
    [Test]
    public void Preferences()
    {
        Assert.IsNotNull(XEnv.Preferences.AppDefault, "Environment/App 应用类型默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.ModeDefault, "Environment/Mode 运行模式默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.SolutionDefault, "Environment/Solution 解决方案名称默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.ProjectDefault, "Environment/Project 项目名称默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.ProductDefault, "Environment/Product 产品名称默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.ChannelDefault, "Environment/Channel 发布渠道默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.VersionDefault, "Environment/Version 版本号默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.AuthorDefault, "Environment/Author 作者名称默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.SecretDefault, "Environment/Secret 应用密钥默认值不应为空");
        Assert.IsNotNull(XEnv.Preferences.RemoteDefault, "Environment/Remote 远程配置地址默认值不应为空");
    }

    /// <summary>
    /// 测试路径管理功能。
    /// </summary>
    [Test]
    public void Paths()
    {
        var tempDir = XFile.PathJoin(XEnv.LocalPath, "TestXEnv-" + XTime.GetMillisecond());
        if (!XFile.HasDirectory(tempDir)) XFile.DeleteDirectory(tempDir);

        try
        {
            // 验证 ProjectPath 是否存在
            if (Application.isEditor) Assert.That(XFile.HasDirectory(XEnv.ProjectPath), Is.True);

            // 验证 AssetPath 是否正确
            Assert.AreEqual(XEnv.AssetPath, Application.streamingAssetsPath);

            // 验证 LocalPath 是否创建
            Assert.IsTrue(XFile.HasDirectory(XEnv.LocalPath));

            // 测试自定义LocalPath
            var customLocal = XFile.PathJoin(tempDir, "CustomLocal");

            // 重置参数缓存并设置自定义路径
            XEnv.ParseArgs(true, "-LocalPath", customLocal);
            XEnv.localPath = null;

            // 验证自定义值
            Assert.AreEqual(Path.GetFullPath(customLocal), Path.GetFullPath(XEnv.LocalPath));

            // 验证本地目录已创建
            Assert.IsTrue(XFile.HasDirectory(XEnv.LocalPath));
        }
        finally
        {
            XFile.DeleteDirectory(tempDir);
            XEnv.ParseArgs(true);
            XEnv.localPath = null;
        }
    }

    /// <summary>
    /// 测试命令行参数解析功能。
    /// </summary>
    [Test]
    public void Args()
    {
        try
        {
            // 测试用例1：基本参数形式
            {
                XEnv.ParseArgs(true, "--test=value");
                Assert.AreEqual("value", XEnv.GetArg("test"), "基本参数形式应正确解析键值对");
            }

            // 测试用例2：多种参数形式
            {
                XEnv.ParseArgs(true,
                    "--key1=value1",          // 双横杠等号
                    "-key2=value2",           // 单横杠等号
                    "--flag1",                // 双横杠无值
                    "-flag2",                 // 单横杠无值
                    "--key3=value3",          // 双横杠等号
                    "-key4", "value4",        // 单横杠空格
                    "--key5", "value5",       // 双横杠空格
                    "-flag3",                 // 单横杠无值
                    "-key6=with=equals",      // 单横杠多等号
                    "--key7=with=equals"      // 双横杠多等号
                );

                Assert.AreEqual("value1", XEnv.GetArg("key1"), "双横杠等号形式的参数应正确解析");
                Assert.AreEqual("value2", XEnv.GetArg("key2"), "单横杠等号形式的参数应正确解析");
                Assert.AreEqual("", XEnv.GetArg("flag1"), "双横杠无值标志应解析为空字符串");
                Assert.AreEqual("", XEnv.GetArg("flag2"), "单横杠无值标志应解析为空字符串");
                Assert.AreEqual("value3", XEnv.GetArg("key3"), "双横杠等号形式的参数应正确解析");
                Assert.AreEqual("value4", XEnv.GetArg("key4"), "单横杠空格分隔的参数应正确解析");
                Assert.AreEqual("value5", XEnv.GetArg("key5"), "双横杠空格分隔的参数应正确解析");
                Assert.AreEqual("", XEnv.GetArg("flag3"), "单横杠无值标志应解析为空字符串");
                Assert.AreEqual("with=equals", XEnv.GetArg("key6"), "含等号的值应完整保留");
                Assert.AreEqual("with=equals", XEnv.GetArg("key7"), "含等号的值应完整保留");
            }

            // 测试用例3：特殊值处理
            {
                XEnv.ParseArgs(true,
                    "--empty=",
                    "--spaces=value with spaces",
                    "--chinese=中文参数",
                    "--symbols=!@#$%^&*()",
                    "--multi=value=with=equals"
                );

                Assert.AreEqual("", XEnv.GetArg("empty"), "空值参数应解析为空字符串");
                Assert.AreEqual("value with spaces", XEnv.GetArg("spaces"), "含空格的值应完整保留");
                Assert.AreEqual("中文参数", XEnv.GetArg("chinese"), "中文参数应正确解析");
                Assert.AreEqual("!@#$%^&*()", XEnv.GetArg("symbols"), "特殊字符应完整保留");
                Assert.AreEqual("value=with=equals", XEnv.GetArg("multi"), "多等号的值应完整保留");
            }

            // 测试用例4：缓存控制
            {
                XEnv.ParseArgs(false, "--newkey=newvalue");
                Assert.AreEqual("中文参数", XEnv.GetArg("chinese"), "不重置缓存时应保留原有参数值");
                Assert.AreEqual("!@#$%^&*()", XEnv.GetArg("symbols"), "不重置缓存时应保留原有参数值");
            }

            // 测试用例5：重置缓存
            {
                XEnv.ParseArgs(true, "--single=value");
                Assert.AreEqual("value", XEnv.GetArg("single"), "重置缓存后新参数应生效");
                Assert.AreEqual("", XEnv.GetArg("chinese"), "重置缓存后原有参数应被清除");
            }

            // 测试用例6：空参数列表
            XEnv.ParseArgs(true);

            // 测试用例7：无效参数形式
            {
                XEnv.ParseArgs(true,
                    "invalid",
                    "--valid=value",
                    "--"
                );
                Assert.AreEqual("value", XEnv.GetArg("valid"), "有效参数应正确解析，忽略无效参数");
            }

            // 测试用例8：参数列表直接访问
            {
                var args = XEnv.ParseArgs(true,
                    "--key1=value1",
                    "--key2=value2"
                );

                var foundKey1 = false;
                var foundKey2 = false;

                foreach (var pair in args)
                {
                    if (pair.Key == "key1")
                    {
                        Assert.AreEqual("value1", pair.Value, "参数列表中的键值对应正确保存");
                        foundKey1 = true;
                    }
                    else if (pair.Key == "key2")
                    {
                        Assert.AreEqual("value2", pair.Value, "参数列表中的键值对应正确保存");
                        foundKey2 = true;
                    }
                }

                Assert.IsTrue(foundKey1, "参数列表应包含第一个测试键值对");
                Assert.IsTrue(foundKey2, "参数列表应包含第二个测试键值对");
            }
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }

    /// <summary>
    /// 测试环境变量解析功能。
    /// </summary>
    [Test]
    public void Eval()
    {
        try
        {
            var testVar = $"TEST_VAR_{XTime.GetMillisecond()}";

            #region 测试命令行参数解析
            {
                XEnv.ParseArgs(true, "-test", "value");
                var result1 = "prefix ${Environment.test} suffix".Eval(XEnv.Instance);
                Assert.AreEqual("prefix value suffix", result1, "应正确解析命令行参数的环境变量引用");
            }
            #endregion

            #region 测试系统环境变量解析
            {
                Environment.SetEnvironmentVariable(testVar, "env_value");
                var result2 = ("prefix ${Environment." + testVar + "} suffix").Eval(XEnv.Instance);
                Assert.AreEqual("prefix env_value suffix", result2, "应正确解析系统环境变量引用");
            }
            #endregion

            #region 测试内置变量解析
            {
                Assert.AreEqual("${Environment.LocalPath}".Eval(XEnv.Instance), XEnv.LocalPath, "${Environment.LocalPath} 解析后应当和 XEnv.LocalPath 相等。");
                Assert.AreEqual("${Environment.ProjectPath}".Eval(XEnv.Instance), XEnv.ProjectPath, "${Environment.ProjectPath} 解析后应当和 XEnv.ProjectPath 相等。");
                Assert.AreEqual("${Environment.AssetPath}".Eval(XEnv.Instance), XEnv.AssetPath, "${Environment.AssetPath} 解析后应当和 XEnv.AssetPath 相等。");
                Assert.AreEqual("${Environment.UserName}".Eval(XEnv.Instance), Environment.UserName, "${Environment.UserName} 解析后应当和 Environment.UserName 相等。");
                Assert.AreEqual("${Environment.Platform}".Eval(XEnv.Instance), XEnv.Platform.ToString(), "${Environment.Platform} 解析后应当和 XEnv.Platform 相等。");
                Assert.AreEqual("${Environment.App}".Eval(XEnv.Instance), XEnv.App.ToString(), "${Environment.App} 解析后应当和 XEnv.App 相等。");
                Assert.AreEqual("${Environment.Mode}".Eval(XEnv.Instance), XEnv.Mode.ToString(), "${Environment.Mode} 解析后应当和 XEnv.Mode 相等。");
                Assert.AreEqual("${Environment.Solution}".Eval(XEnv.Instance), XEnv.Solution, "${Environment.Solution} 解析后应当和 XEnv.Solution 相等。");
                Assert.AreEqual("${Environment.Project}".Eval(XEnv.Instance), XEnv.Project, "${Environment.Project} 解析后应当和 XEnv.Project 相等。");
                Assert.AreEqual("${Environment.Product}".Eval(XEnv.Instance), XEnv.Product, "${Environment.Product} 解析后应当和 XEnv.Product 相等。");
                Assert.AreEqual("${Environment.Channel}".Eval(XEnv.Instance), XEnv.Channel, "${Environment.Channel} 解析后应当和 XEnv.Channel 相等。");
                Assert.AreEqual("${Environment.Version}".Eval(XEnv.Instance), XEnv.Version, "${Environment.Version} 解析后应当和 XEnv.Version 相等。");
                Assert.AreEqual("${Environment.Author}".Eval(XEnv.Instance), XEnv.Author, "${Environment.Author} 解析后应当和 XEnv.Author 相等。");
                Assert.AreEqual("${Environment.Secret}".Eval(XEnv.Instance), XEnv.Secret, "${Environment.Secret} 解析后应当和 XEnv.Secret 相等。");
                Assert.AreEqual("${Environment.NumCPU}".Eval(XEnv.Instance), SystemInfo.processorCount.ToString(), "${Environment.NumCPU} 解析后应当和 SystemInfo.processorCount 相等。");
            }
            #endregion

            #region 测试参数优先级
            {
                XEnv.ParseArgs(true, $"-{testVar}", "arg_value");
                var result3 = ("${Environment." + testVar + "}").Eval(XEnv.Instance);
                Assert.AreEqual("arg_value", result3, "命令行参数应优先于系统环境变量");
            }
            #endregion

            #region 测试缺失变量处理
            {
                XEnv.ParseArgs(true);
                var result4 = "hello ${Environment.missing}".Eval(XEnv.Instance);
                Assert.IsTrue(result4.Contains("(Unknown)"), "未定义的环境变量应标记为未知");
            }
            #endregion

            #region 测试嵌套变量处理
            {
                var result5 = "nested ${Environment.outer${Environment.inner}}".Eval(XEnv.Instance);
                Assert.IsTrue(result5.Contains("(Nested)"), "嵌套的环境变量引用应标记为嵌套");
            }
            #endregion
        }
        finally
        {
            // 重置参数缓存
            XEnv.ParseArgs(true);
        }
    }
}
