# MgTx - 猛攻通讯 第三方声明

## 项目概述

MgTx 是一个完全独立开发的工业通信框架，旨在提供现代化的工业自动化通信解决方案。

## 第三方依赖

本项目使用以下开源组件：

### 运行时框架

- **.NET 10 SDK**
  - License: MIT
  - Source: https://github.com/dotnet/runtime

### 核心库

- **Serilog.AspNetCore**
  - License: Apache-2.0
  - Source: https://github.com/serilog/serilog-aspnetcore

- **Serilog.Sinks.Console**
  - License: Apache-2.0
  - Source: https://github.com/serilog/serilog-sinks-console

- **Serilog.Sinks.File**
  - License: Apache-2.0
  - Source: https://github.com/serilog/serilog-sinks-file

### 脚本引擎

- **Jint**
  - License: BSD-2-Clause
  - Source: https://github.com/sebastienros/jint

- **IronPython**
  - License: Apache-2.0
  - Source: https://github.com/IronLanguages/ironpython3

### 通信协议

本项目实现以下工业标准协议，这些协议是公开的行业标准：

- **Modbus TCP** - IEC 61158 标准
- **Siemens S7** - 西门子公开通信规范
- **Mitsubishi MC** - 三菱公开通信协议
- **Omron FINS** - 欧姆龙公开通信协议

## 独立性声明

本项目：
1. 不包含任何其他工业通信库的源代码
2. 不复制任何第三方库的实现细节
3. 所有代码均为独立开发和实现
4. 仅遵循公开的协议规范和标准

## 免责声明

本项目提供"按原样"的软件，不提供任何形式的担保，包括但不限于：
- 对特定用途的适用性
- 非侵权性
- 性能保证

## 联系方式

如有任何疑问，请联系 MgTx 项目团队。

---
最后更新: 2026-05-03
