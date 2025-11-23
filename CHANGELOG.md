# 更新记录

## [1.1.2] - 
### 新增
- 新增 XTime.GetMicrosecond 函数以获取高精度的时间戳

### 优化
- 优化 XTime.GetTimestamp/GetMillisecond 函数的实现

## [1.1.1] - 2025-11-09
### 修复
- 更正 package.json 中的产品名称

## [1.1.0] - 2025-11-09
### 修复
- 修复 XPool 在高并发场景下的缓存池异常
- 修复 XEvent 模块的 puer 适配器错误

### 变更
- 优化 XLog.File 模块 Write 函数的执行时机
- 重构 XEnv 模块的首选项前缀为 XEnv
- 重构 XLog 模块的首选项前缀为 XLog
- 标准化产品名称、项目自述及模块文档
- 移除 XPrefs.IRemote.IHandler 接口回调的 context 参数

## [1.0.0] - 2025-10-26
### 新增
- 首次发布
