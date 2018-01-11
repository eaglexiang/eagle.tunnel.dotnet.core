# Eagle Tunnel

![](https://img.shields.io/badge/language-csharp-blue.svg) [![](https://travis-ci.org/eaglexiang/eagle.tunnel.dotnet.core.svg?branch=master)](https://travis-ci.org/eaglexiang/eagle.tunnel.dotnet.core)

代理工具，SSR的替代品。

[主页](https://www.eaglexiang.org/eagle-tunnel)

## 支持

### 基本功能

- [x] HTTP(S)代理
- [x] SOCKS5代理

### 支持平台

- [x] Windows
- [x] Linux
- [x] macOS

## 使用方法

### 通用版

#### 下载

需要先安装[.NET Core Runtime](https://www.microsoft.com/net/download/)。

[点击下载](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/raw/bin/eagle-tunnel.zip)后再次执行上述指令即可 （请在配置文件末尾保留一行空行）

#### 参数表

可选参数 | 作用
--- | ---
httpserver | 启动HTTP远端服务
hs | 启动HTTP远端服务（同httpserver）
 |
socksserver | 启动Socks5远端服务
ss | 启动Socks5远端服务（同socksserver）
 |
httpclient | 启动HTTP本地服务
hc | 启动HTTP本地服务（同httpclient）
 |
socksclient | 启动Socks5本地服务
sc | 启动Socks5本地服务（同socksclient）

#### 开启步骤

1. 远端服务请运行于网络通畅的计算机上，本地服务运行于需要使用代理服务的计算机上，若将`Local IP`填为局域网IP，则可为局域网计算机（包括手机）提供服务。启动命令见下一小节。
1. 将手机或电脑的代理地址设置为本地服务的LocalIP和LocalHttpPort

#### 启动明林

```shell
dotnet eagle.tunnel.dotnet.core.dll [参数]
# 可选参数见上表
# 例如启动远端HTTP服务和远端Socks5服务：
dotnet eagle.tunnel.dotnet.core.dll hs ss
# 第一次启动四种服务之一，会在当前目录生成默认配置文件
# 请用文本编辑器编辑后（文件末尾请留一行空行）
# 再次执行启动命令
```

#### 守护进程

对于CentOS、Archlinux等使用systemd的Linux发行版，可使用[supervisor](http://supervisord.org/)实现守护进程。

### Windows版（图形界面）

[eagle.tunnel.dotnet](https://github.com/eaglexiang/eagle.tunnel.dotnet)（只支持HTTP(S)代理）

[下载](https://github.com/eaglexiang/eagle.tunnel.dotnet/raw/bin/Eagle%20Tunnel.exe)

## 开源协议

[MIT](./LICENSE)
