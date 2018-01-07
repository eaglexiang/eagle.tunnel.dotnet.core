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

Windows x86_64

```shell
git clone -b bin-win64 https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
```

Linux x86_64

```shell
git clone -b bin-linux64 https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
```

通用版

需要安装[.NET Core Runtime](https://www.microsoft.com/net/download/)

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
