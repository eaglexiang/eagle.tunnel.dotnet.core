# 使用方法

## 支持

### 特性

- [x] HTTP(S) 代理
- [x] SOCKS 5 代理
- [x] 负载均衡
- [x] systemd 系统服务
- [x] 用户登录
- [x] 用户限速
- [x] 智能分流

### 平台

- [x] [Windows](https://github.com/eaglexiang/eagle.tunnel.dotnet)
- [x] Linux
- [x] macOS

## 什么是 Eagle Tunnel

简单来说，这就是个代理工具。起初，Eagle Tunnel是一个HTTP代理协议的简单实现。但经过结构的演变，现在它的确更符合`隧道`这个名字。在生活学习工作中，我们时不时会用到各种网络代理协议，Eagle Tunnel 想要做到的，是一个透明的隧道，Web客户端在左边，Web服务器在右边。

![tunnel](https://github.com/eaglexiang/eagle.tunnel.dotnet.core.lib/blob/master/doc/tunnel.png)

Eagle Tunnel 接收各种代理协议，过滤成统一的 Eagle Tunnel 协议格式，再进行加密的流量转发。

![arch](https://github.com/eaglexiang/eagle.tunnel.dotnet.core.lib/blob/master/doc/arch.png)

Eagle Tunnel还在迭代中，它的目标是，部署简单，工作稳定，轻量化。

## 准备工作

1. 首先您需要一台防火墙外的计算机，我使用的是[vultr](https://www.vultr.com/?ref=7357306)的VPS，月付5刀，部分地区有2.5刀的货源。不过安装系统后最好先在国内ping一下是否畅通，最近屏蔽了很多IP。如果不是很熟练Linux的使用，建议给VPS安装CentOS 7，因为下文会以CentOS 7为标准。当然，你也可以使用任意你喜欢的操作系统。

2. 由于Eagle Tunnel基于.NET Core开发，因此需要安装.NET Core的运行库。**VPS和本机都需要安装**。下面列举一些常见的系统对应的安装方式。

### Windows

* [Windows x86_64](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x64-installer)
* [Windows x86](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x86-installer)

### macOS

* [macOS](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-macos-x64-installer)

### Archlinux

```shell
yaourt -S dotnet-runtime
```

### CentOS

```shell
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[packages-microsoft-com-prod]nname=packages-microsoft-com-prod nbaseurl= https://packages.microsoft.com/yumrepos/microsoft-rhel7.3-prodnenabled=1ngpgcheck=1ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/dotnetdev.repo'
sudo yum -y update
sudo yum -y install libunwind libicu
sudo yum install dotnet-sdk-2.1.4
```

### Ubuntu 17.10

```shell
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-artful-prod artful main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1.4
```

### 其它系统

如果是其它操作系统可到[.NET Core 官网](https://www.microsoft.com/net/download/dotnet-core/runtime-2.0.5)自行下载安装。

## 下载 Eagle Tunnel

[稳定版下载地址](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/releases)

## 服务器端配置

简单来说，配置过程为：

1. 安装dotnet和git
2. 下载并安装 Eagle Tunnel
3. 基本设置
4. 启动服务

此小节默认服务器环境为CentOS 7，并且会使用最新的master分支（而非编译好的二进制文件）。其它环境的朋友也可参考。

### 安装dotnet

打开终端（shell）并执行以下指令

```shell
# 安装dotnet
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[packages-microsoft-com-prod]\nname=packages-microsoft-com-prod \nbaseurl= https://packages.microsoft.com/yumrepos/microsoft-rhel7.3-prod\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/dotnetdev.repo'
sudo yum update -y
sudo yum install -y libunwind libicu
sudo yum install -y dotnet-sdk-2.1.105
# 安装git
sudo yum install -y git
```

### 下载并安装 Eagle Tunnel

```shell
git clone --recursive https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core
./build
sudo ./install.sh
```

此时 Eagle Tunnel 已经安装完毕。接下来我们需要对其进行配置，默认的配置文件已被安装到`/etc/eagle-tunnel.conf`。

### 基本设置

```shell
sudo vim /etc/eagle-tunnel.conf
```

在任意位置添上以下内容：

```shell
# 如果你不清楚VPS的IP为多少，可通过ifconfig命令进行查询。
Listen=VPS的IP:8080
eagle tunnel=on
```

### 启动服务

```shell
sudo systemctl start eagle-tunnel.service # 立即启动
sudo systemctl enable eagle-tunnel.service # 开机自启
```

此时，如果不出意外，服务端已经搭建完成。可用下面命令查看服务状态，正常情况应该会显示`Active: active (running)`。如果你的系统不支持systemd，也可以考虑使用[supervisor](https://www.baidu.com/s?wd=supervisor+dotnet+core)来实现服务的后台运行。

```shell
sudo systemctl status eagle-tunnel.service
```

> 如果你的系统存在防火墙，一定要将`Listen`对应的端口打开。你可能需要执行以下指令：

```shell
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload
```

## 客户端配置

### 支持systemd的Linux定制版

首先仍然是安装 Eagle Tunnel 及其依赖项（dotnet runtime 和 git）。

```shell
git clone --recursive https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core
./build
sudo ./install.sh
```

然后同样是编辑配置文件`/etc/eagle-tunnel.conf`。

```shell
sudo vim /etc/eagle-tunnel.conf
```

并添上以下选项：

```shell
Relayer=VPS的IP:8080 # 与前文中的Listen=相同
Listen=127.0.0.1:8080 # 注意此处，即为本地代理服务的地址，也就是应用程序里填写的代理地址
http=on
socks=on
```

最后马上启动代理。

```shell
sudo systemctl start eagle-tunnel.service # 立即启动服务
sudo systemctl enable eagle-tunnel.service # 开机自启
```

> 如果你的系统存在防火墙，一定要将`Listen`对应的端口打开。你可能需要执行以下指令：

```shell
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload
```

### 通用使用方法

并不是每个操作系统都支持systemd守护进程。所以有必要提供手动启动服务的方式。简单来说就是先编辑配置文件，再执行运行脚本。

1. 编辑配置文件
2. 运行脚本

配置文件的规则请参考上一小节。文件为`eagle-tunnel.conf`，位于`publish/etc/`文件夹中。

操作系统 | 脚本名
--- | ---
Linux / macOS | run.sh
Windows | run.bat

## 系统配置

本程序只提供代理服务本身的功能，并不会自动配置您的系统代理，也就是说这需要您手动进行设置。系统的自动配置在带图形界面的版本（即[Windows版本](https://github.com/eaglexiang/eagle.tunnel.dotnet)）中提供支持。假如你是萌新，不知道怎么给系统用上刚刚搭建好的代理，建议百度类似`Windows设置HTTP代理`的关键词。在我们设置系统代理的时候，常会用到`代理地址`和`端口号`这两个东西，可从配置文件中找到。例如在本文示例配置中的`127.0.0.1:8080`中，`127.0.0.1`就是代理地址，`8080`就是端口号。

如果手机端想使用 Eagle Tunnel 提供的服务，可参照下文将服务共享到内网，然后手动设置手机WIFI的HTTP代理。如果 Android 平台一定要使用 SOCKS 代理，可考虑使用[Postern](https://play.google.com/store/apps/details?id=com.tunnelworkshop.postern)这款APP。

## 高级

### 共享服务

按照上文示例，代理服务只会被提供给本机。分享服务到内网的方法是有的，只需要简单地将`Listen`参数设置为内网IP（例如192.168.0.2）即可。

### 负载均衡

如果你拥有多台VPS，想充分利用它们的流量，提高上网的速度，可以考虑开启Eagle-Tunnel的负载均衡功能。开启方法很简单，只需要在上文提到的配置文件中，按照格式换行添加额外的`Relayer`，然后重启服务即可。例如：

```shell
Relayer=1.2.4.8:8080
Relayer=8.8.8.8:8080
```

> 需要注意的是：
>
> 1. 负载均衡只可能提高多个TCP连接并行传输的总速度，并不能提高单个TCP连接的速度。但同一网页通常拥有多个连接，因此是会因为启用负载均衡而受益的。
> 2. 如果你的多台VPS位于不同的国家或地区，可能会造成反效果。这是因为同一网页的不同服务请求了位于不同地区的服务器，这可能导致冲突。

### 用户认证

如果你暴露了自己的服务器IP和端口，任何使用Eagle-Tunnel客户端的人都能轻易连上你的服务，从而盗用你的流量。解决方法是为你的代理服务开启用户认证功能。

在服务端中开启用户认证的步骤为：

1. 编辑`/etc/eagle-tunnel.conf`文件，取消`User-Conf`的注释。
2. 编辑`/etc/eagle-tunnel.d/users.list`文件，在其中按照下述规则添加用户：
3. 重启服务

```shell
用户名1:用户密码1
用户名2:用户密码2
```

在本地服务（客户端）中开启用户认证的方法为，编辑`etc/eagle-tunnel.conf`文件，取消`User`的注释，并为其按照下述格式填上帐号和密码：

```shell
User=账户名:密码
```

### 用户限速

如果你需要将代理临时共享给朋友，又担心他的滥用，可以为他分配一个访客账户，并将其限速。限速功能依赖于上一小节的用户认证功能。开启方法为，在`/etc/eagle-tunnel.conf`文件中添加：

```shell
speed-check=on
```

然后在用户列表文件`/etc/eagle-tunnel.d/users.list`中，为对应[用户民:密码]组添加限速后缀[:数值]（单位为KB/s），如下：

```shell
id:key:500
# 这一行的含义是，一个ID为id的账户，密码为key，限速为500KB/s。
```

### 智能分流

为了保证国内网站的正常使用与高速连接，Eagle Tunnel 提供了一个可选的智能分流功能，它由参数`Proxy Status`控制：

Proxy Status 的赋值 | 效果
--- | ---
enable | 这是默认值。效果为所有流量都会被代理
disable | 禁用代理。所有流量都不会被代理
smart | 智能分流。在DNS解析过程中，`whitelist_domain.txt`中填写的域名，会被强制代理，其余域名会使用本地解析；在接受代理请求后，境外IP会被强制代理，境内IP会由本地进行直连。

### 可用参数一览表

参数名 | 默认值 | 作用
--- | --- | ---
Relayer | | 远端服务地址
Listen | | 本地服务监听地址
http | off | HTTP代理开关
socks | off | SOCKS代理开关
eagle tunnel | off | Eagle Tunnel协议开关
proxy status | enable | 代理服务的分流状态
worker | 200 | 受理请求的并发数（值越大，并发能力越强，可能的资源消耗越高）
User | | 本地服务的账户密码，凭此与远端进行认证交互，注释状态表示关闭认证
User-Conf | /etc/eagle-tunnel.d/users.list | 远端服务用到的用户列表，用于用户认证与用户限速，注释状态表示关闭认证
speed-check | off | 基于帐号系统的帐号限速功能开关

## 许可证

[LICENSE](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/blob/master/LICENSE)