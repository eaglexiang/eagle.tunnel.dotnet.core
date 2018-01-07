# Eagle Tunnel

代理工具，SSR的替代品。基于.NET Core。

[主页](https://www.eaglexiang.org/eagle-tunnel)

## 支持

### 基本功能

- [x] HTTP代理
- [x] HTTPS代理
- [ ] SOCKET5代理
- [ ] GUI

### 支持平台

- [x] Windows
- [x] Linux
- [x] macOS

= =反正就是.NET Core支持的平台。

- [ ] Android

## 使用方法

通用版

需要先安装[.NET Core Runtime](https://www.microsoft.com/net/download/)。

```shell
git clone -b bin https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
# 解压文件
# 以服务端方式启动
dotnet eagle.tunnel.dotnet.core.dll s
# 以客户端方式启动
dotnet eagle.tunnel.dotnet.core.dll c
```

将手机或电脑的代理地址设置为本地服务的LocalIP和LocalHttpPort

## 开源协议

[MIT](./LICENSE)
