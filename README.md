# Eagle Tunnel

代理工具，SSR的替代品

[主页](https://www.eaglexiang.org/eagle-tunnel)

## 基本功能

- [x] HTTP代理
- [x] HTTPS代理
- [ ] SOCKET5代理

## 基本原理

![基本原理](./doc/arch.png)

## 使用方法

### 环境准备

需要安装[.NET Core](https://www.microsoft.com/net/learn/get-started)

### 使用

远端服务（服务器端）

```shell
git clone -b bin https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core/eagle.tunnel
# 用文本编辑器编辑同目录下的config.cfg
dotnet eagle.tunnel.dotnet.core.dll s
```

本地服务（客户端）

```shell
git clone -b bin https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core/eagle.tunnel
# 用文本编辑器编辑同目录下的config.cfg
dotnet eagle.tunnel.dotnet.core.dll c
```

将手机或电脑的代理地址设置为本地服务的IP和Port

## 开源协议

[MIT](./LICENSE)
