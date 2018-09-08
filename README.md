# Eagle Tunnel

![csharp](https://img.shields.io/badge/language-csharp-blue.svg) [![eagle tunnel](https://travis-ci.org/eaglexiang/eagle.tunnel.dotnet.core.svg?branch=master)](https://travis-ci.org/eaglexiang/eagle.tunnel.dotnet.core)

稳定易用的代理工具。

> 出于性能和易于部署的因素，ET已经用Golang重新实现：[eagle.tunnel.go](https://github.com/eaglexiang/eagle.tunnel.go)

## 介绍

### 特性

- [x] HTTPS(S) 代理
- [x] SOCKS 5 代理
- [x] 多路负载均衡
- [x] systemd系统服务
- [x] 用户认证
- [x] 用户限速
- [x] 智能分流
- [x] hosts

### 平台

- [x] [Windows](https://github.com/eaglexiang/eagle.tunnel.dotnet)
- [x] Linux
- [x] macOS

### 依赖

[dotnet core](https://github.com/dotnet/core)

### 安装

Linux/macOS

```shell
mkdir ~/git
cd ~/git
git clone --recursive https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core
./build.sh
sudo ./install.sh
```

Windows/Linux/macOS

[下载](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/releases)

## 其它

基于 [Eagle Tunnel Lib](https://github.com/eaglexiang/eagle.tunnel.dotnet.core.lib) 开发

- [什么是 Eagle Tunnel](https://www.eaglexiang.org/eagle-tunnel)
- [怎么使用 Eagle Tunnel](./doc/guide.md)
- 如果使用中遇到问题，欢迎到[issues](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/issues)提问

## 开源协议

[MIT](./LICENSE)