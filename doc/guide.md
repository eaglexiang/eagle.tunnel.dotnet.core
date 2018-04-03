# 备用食用方法

Eagle-Tunnel是[SOCKS 5](/socks5-chinese)协议的非标准实现。其存在目的见于协议的`介绍`一节。

## 支持

### 特性

* HTTP(S) 代理
* SOCKS 5 代理（暂不提供关于UDP的支持）
* 负载均衡
* 用户登录
* 用户限速

### 平台

* Windows
* Linux
* macOS

## 下载

[下载地址](/eagle-tunnel-latest)

## 准备工作

1. 首先您需要一台防火墙外的计算机，我使用的是[vultr](https://www.vultr.com/?ref=7357306)的VPS，月付5刀，部分地区有2.5刀的货源。不过安装系统后最好先在国内ping一下是否畅通，最近屏蔽了很多IP。如果不是很熟练Linux的使用，建议给VPS安装CentOS 7，因为下文会以CentOS 7为标准。当然，理论上你可以使用任意你喜欢的操作系统。

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

## 服务器端配置

此小节默认服务器环境为CentOS 7，其它环境的朋友也可参考。

将上文下载的压缩包解压到一个文件夹中，进入该文件夹。使用root权限执行安装脚本`install.sh`。添加守护进程并开启相应的防火墙权限。

```shell
sudo ./install.sh
sudo systemctl enable eagle-tunnel-server.service
# 如果系统没有防火墙，下面几条指令不会执行成功，不过没关系
sudo firewall-cmd --zone=public --add-port=8080/tcp --permanent
sudo firewall-cmd --zone=public --add-port=8081/tcp --permanent
sudo firewall-cmd --reload
```

此时默认的配置文件已被安装到`/etc/eagle-tunnel.conf`，我们需要编辑它。在编辑之前它应该长这个样子：

> Remote HTTP Address:0.0.0.0:8080
> Remote SOCKS Address:0.0.0.0:8081
> Local HTTP Address:0.0.0.0:8080
> Local SOCKS Address:0.0.0.0:8081

将`Remote HTTP Address`和`Remote SOCKS Address`的`0.0.0.0`更改为VPS的外网IP。假如你的VPS的IP为`1.2.4.8`，则更改后的配置文件应该是下面这个样子：

> Remote HTTP Address:`1.2.4.8`:8080
> Remote SOCKS Address:`1.2.4.8`:8081
> Local HTTP Address:0.0.0.0:8080
> Local SOCKS Address:0.0.0.0:8081

最后立即开启代理服务。

```shell
sudo systemctl start eagle-tunnel-server.service
```

此时，如果不出意外，服务端已经搭建完成。可用下面命令查看服务状态，正常情况应该会显示`Active: active (running)`

```shell
sudo systemctl status eagle-tunnel-server.service
```

如果你的系统不支持systemd，也可以考虑使用[supervisor](https://www.baidu.com/s?wd=supervisor+dotnet+core)来实现服务的后台运行。

## 客户端配置

### Linux with systemd

首先仍然是安装。

```shell
sudo ./install.sh
sudo systemctl enable eagle-tunnel-client.service
```

然后同样是编辑配置文件`/etc/eagle-tunnel.conf`。编辑前应该是这样：

> Remote HTTP Address:0.0.0.0:8080
> Remote SOCKS Address:0.0.0.0:8081
> Local HTTP Address:0.0.0.0:8080
> Local SOCKS Address:0.0.0.0:8081

我们首先将上文配置的VPS的IP（此处示例为`1.2.4.8`）填入其中：

> Remote HTTP Address:`1.2.4.8`:8080
> Remote SOCKS Address:`1.2.4.8`:8081
> Local HTTP Address:0.0.0.0:8080
> Local SOCKS Address:0.0.0.0:8081

然后将本地IP填入其中：

> Remote HTTP Address:1.2.4.8:8080
> Remote SOCKS Address:1.2.4.8:8081
> Local HTTP Address:`127.0.0.1`:8080
> Local SOCKS Address:`127.0.0.1`:8081

> \# 提示：将`127.0.0.1`替换为内网IP即可将代理服务共享到内网。

最后马上启动代理。

```shell
sudo systemctl start eagle-tunnel-client.service
```

完成。

### 通用使用方法

并不是每个操作系统都支持systemd守护进程。所以有必要提供手动启动服务的方式。简单来说就是先编辑配置文件，再执行运行脚本。

1. 编辑配置文件

配置文件的规则请参考上一小节。文件为`eagle-tunnel.conf`，位于`scripts`文件夹中。

2. 运行脚本

操作系统 | 脚本名
--- | ---
Linux / macOS | run.sh
Windows | run.bat

## 系统配置

本程序只提供代理服务本身的功能，不会自动配置您的系统代理，需要您手动进行设置。假如你是个萌新，不知道怎么给系统用上刚刚搭建好的代理，建议百度类似`Windows设置HTTP代理`的关键词。在我们设置系统代理的时候，常会用到`代理地址`和`端口号`这两个东西，可从上面的配置中找到。例如`127.0.0.1:8080`中，`127.0.0.1`就是代理地址，`8080`就是端口号。

此外，如果有防火墙内外自动分流的需求，可考虑搭配[SwitchyOmega](https://chrome.google.com/webstore/detail/proxy-switchyomega/padekgcemlokbadohgkifijomclgjgif)一类浏览器插件。

如果Android平台想使用共享到内网的代理服务，可考虑使用[Postern](https://play.google.com/store/apps/details?id=com.tunnelworkshop.postern)这款APP。

## 负载均衡

如果你拥有多台VPS，想充分利用它们的流量，提高上网的速度，可以考虑开启Eagle-Tunnel的负载均衡功能。开启方法很简单，只需要在上文提到的配置文件中，按照格式换行添加`Remote HTTP Address`或者`Remote SOCKS Address`，然后重启服务即可。

需要注意的是：

1. 负载均衡只可能提高多个TCP连接并行传输的总速度，并不能提高单个TCP连接的速度。
2. 如果你的多台VPS位于不同的国家或地区，可能会造成反效果。因为同一个网页的不同服务请求了位于不同国家的服务器，这可能导致冲突。

## 用户认证

如果你暴露了自己的服务端IP和端口，任何使用Eagle-Tunnel客户端的人都能连上你的服务。解决方法是为你的代理服务启用用户认证功能。

以下步骤为启用方法，服务端和客户端`都需要`进行以下步骤，否则会导致验证失败：

1. 在你喜欢的地方新建一个用户文件（例如user.conf），使用任意喜欢的文本编辑工具（例如文本文档）在其中按照下述规则添加用户：

> 用户名1:用户密码1
> 用户名2:用户密码2

2. 编辑配置文件`eagle-tunnel.conf`，添加一行下述参数：

> users:[刚才user.conf文件的路径]

3. 重启服务

提示：客户端使用的用户名与密码以文件中出现的第一组为准。

## 用户限速

如果你需要将代理共享给朋友，又担心他的滥用，可以为他分配一个访客账户，并将其限速。限速功能依赖于上一小节的用户认证功能。开启方法为，在用户文件（user.conf）中，为对应[用户民:密码]组添加限速后缀[:数值]（单位为KB/s），如下：

> eagle1:eagle2`:500`
> \# 这一行的含义是，一个ID为eagle1的账户，密码为eagle2，限速为`500`KB/s。

## 其它

如果有其它特殊需求，也可自行编写程序启动的命令。运行命令的格式为：

```shell
dotnet [eagle.tunnel.dotnet.core.dll文件的路径] c=[配置文件的路径] [选项]
```

请将`c=`参数紧跟在`eagle.tunnel.dotnet.core.dll`的后面。

可供选择的选项有：

选项 | 释义
--- | ---
ss | SOCKS服务器
sc | SOCKS客户端
hs | HTTP(S)服务端
hc | HTTP(S)客户端

四个选项可自由组合。例如：

```shell
dotnet eagle.tunnel.dotnet.core.dll c=/etc/eagle-tunnel.conf hc sc #启动HTTP(S)代理与SOCKS5代理的本地服务，配置文件为`/etc/eagle-tunnel.conf`
```

## 源码

[repo owner="eaglexiang" name="eagle.tunnel.dotnet.core"]

## 许可证

[LICENSE](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/blob/master/LICENSE)