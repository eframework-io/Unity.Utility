# XPrefs

[![NPM](https://img.shields.io/npm/v/io.eframework.unity.utility?label=NPM&logo=npm)](https://www.npmjs.com/package/io.eframework.unity.utility)
[![UPM](https://img.shields.io/npm/v/io.eframework.unity.utility?label=UPM&logo=unity&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.eframework.unity.utility)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-io/Unity.Utility)
[![Discord](https://img.shields.io/discord/1422114598835851286?label=Discord&logo=discord)](https://discord.gg/XMPx2wXSz3)

实现了多源化配置的读写，支持可视化编辑、变量求值和命令行参数覆盖等功能，是一个灵活高效的首选项系统。

## 功能特性

- 多源化配置：支持资产、本地、远端首选项的多源化解析，按顺序优先级获取配置项
- 多数据类型：支持基础类型（整数、浮点数、布尔值、字符串）、数组类型及配置实例（IBase）
- 变量求值：支持命令行参数覆盖配置项，使用 ${Preferences.Key} 语法引用其他配置项
- 自定义编辑器：通过自定义编辑器实现可视化编辑，支持在保存、应用和构建流程中注入自定义逻辑

## 使用手册

### 1. 基础操作

#### 1.1 检查配置项
```csharp
// 检查配置项是否存在
var exists = XPrefs.HasKey("configKey");
```

#### 1.2 读写基本类型
```csharp
// 写入配置
XPrefs.Local.Set("intKey", 42);
XPrefs.Local.Set("floatKey", 3.14f);
XPrefs.Local.Set("boolKey", true);
XPrefs.Local.Set("stringKey", "value");

// 读取配置
var intValue = XPrefs.GetInt("intKey", 0);
var floatValue = XPrefs.GetFloat("floatKey", 0f);
var boolValue = XPrefs.GetBool("boolKey", false);
var stringValue = XPrefs.GetString("stringKey", "");
```

#### 1.3 读写数组类型
```csharp
// 写入数组
XPrefs.Local.Set("intArray", new[] { 1, 2, 3 });
XPrefs.Local.Set("stringArray", new[] { "a", "b", "c" });

// 读取数组
var intArray = XPrefs.GetInts("intArray");
var stringArray = XPrefs.GetStrings("stringArray");
```

### 2. 配置源管理

#### 2.1 资产首选项
```csharp
var value = XPrefs.Asset.GetString("key");
```

#### 2.2 本地首选项
```csharp
// 写入本地配置
XPrefs.Local.Set("key", "value");
XPrefs.Local.Save();

// 读取本地配置
var value = XPrefs.Local.GetString("key");
```

#### 2.3 远端首选项
```csharp
// RemoteHandler 是远端首选项的处理器。
public class RemoteHandler : XPrefs.IRemote.IHandler
{
    // Uri 是远端的地址。
    public string Uri => "http://example.com/config";

    // OnStarted 是流程启动的回调。
    public void OnStarted() { }
    
    // OnRequest 是预请求的回调。
    public void OnRequest(UnityWebRequest request) { 
        request.timeout = 10;
    }

    // OnRetry 是错误重试的回调。
    public bool OnRetry(int count, out float pending)
    {
        pending = 1.0f;
        return count < 3;
    }

    // OnSucceeded 是请求成功的回调。
    public void OnSucceeded() { }

    // OnFailed 是请求失败的回调。
    public void OnFailed() { }
}

// 读取远端首选项
RunCoroutine(XPrefs.Remote.Read(new RemoteHandler()));
```

### 3. 变量求值

#### 3.1 基本用法
```csharp
// 设置配置项
XPrefs.Local.Set("name", "John");
XPrefs.Local.Set("greeting", "Hello ${Preferences.name}");

// 解析变量引用
var result = XPrefs.Local.Eval("${Preferences.greeting}"); // 输出: Hello John
```

#### 3.2 多级路径
```csharp
// 设置嵌套配置
XPrefs.Local.Set("user.name", "John");
XPrefs.Local.Set("user.age", 30);

// 使用多级路径引用
var result = XPrefs.Local.Eval("${Preferences.user.name} is ${Preferences.user.age}");
```

#### 3.3 构建处理

支持在构建流程的 `IPreprocessBuildWithReport` 阶段对资产首选项的配置进行变量求值，规则及示例如下：

```json
{
    "environment_key": "${Environment.ProjectPath}/Build", // 引用环境变量会被求值
    "preferences_key": "${Preferences.environment_key}",   // 引用配置变量会被求值
    "const_key@Const": "${Environment.LocalPath}",         // 标记 @Const 的值不会被求值
    "editor_key@Editor": "editor_value"                    // 标记 @Editor 的配置会被移除
}
```

### 4. 命令行参数

#### 4.1 覆盖首选项
```bash
--Preferences@Asset=path/to/asset.json    # 覆盖资产首选项路径
--Preferences@Local=path/to/local.json    # 覆盖本地首选项路径
```

#### 4.2 覆盖配置值
```bash
--Preferences@Asset.key=value             # 覆盖资产首选项配置
--Preferences@Local.key=value             # 覆盖本地首选项配置
--Preferences.key=value                   # 覆盖所有首选项配置
```

### 5. 自定义编辑器

通过自定义编辑器实现可视化编辑，支持在保存、应用和构建流程中注入自定义逻辑：

```csharp
public class MyPreferencesEditor : XPrefs.IEditor
{
    // Section 是配置分组的名称。
    string XPrefs.IEditor.Section => "MyPreferences";
    
    // Tooltip 是配置分组的提示。
    string XPrefs.IEditor.Tooltip => "This is tooltip of MyPreferences.";
    
    // Foldable 表示是否支持折叠。
    bool XPrefs.IEditor.Foldable => true;

    // Priority 获取显示的优先级。
    int XPrefs.IEditor.Priority => 0;

    // OnActivate 在面板激活时调用。
    void XPrefs.IEditor.OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement, XPrefs.IBase context) { }
    
    // OnVisualize 在面板绘制时调用。
    void XPrefs.IEditor.OnVisualize(string searchContext, XPrefs.IBase context) { }

    // OnDeactivate 在面板停用时调用。
    void XPrefs.IEditor.OnDeactivate(XPrefs.IBase context) { }

    // OnSave 在保存配置时调用。
    bool XPrefs.IEditor.OnSave(XPrefs.IBase context) { return true; }

    // OnApply 在应用配置时调用。
    bool XPrefs.IEditor.OnApply(XPrefs.IBase context) { return true; }

    // OnBuild 在项目构建时调用。
    bool XPrefs.IEditor.OnBuild(XPrefs.IBase context) { return true; }
}
```

## 常见问题

### 1. 首选项配置无法保存？
- 检查配置对象是否可写（writable = true）。
- 确认文件路径有效且具有写入权限。
- 验证是否调用了 Save() 方法。

### 2. 首选项变量替换失败？
- 确认变量引用格式正确（${Preferences.key}）。
- 检查引用的配置项是否存在。
- 注意避免循环引用和嵌套引用。

### 3. 远端首选项请求失败？
- 检查网络连接是否正常。
- 确认远端服务器地址正确。
- 验证超时和重试参数设置。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可协议](../LICENSE.md)