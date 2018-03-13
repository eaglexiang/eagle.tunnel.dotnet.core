# 食用方法

最近使用SSR出现了不稳定，分析过代理工具的原理后，试着自己造了一个轮子。功能简单，代码不多，不过实测性能还不错。

## 告知

提供以下功能：

* HTTP(S) 代理
* SOCKS 5 代理（暂不提供关于UDP的支持）

由于.NET Core支持Windows、Linux、macOS，所以理论上本工具也支持上述三个平台。不过需要注意的是，ARM平台暂未找到合适的运行方式，所以树莓派目前（2018年3月）没有办法运行该工具。

## 下载

* **[本地源](/eagle-tunnel/eagle-tunnel-2)**
* **[Github源](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/raw/bin/eagle-tunnel.zip)**

为防止Google Analysis报警存在可执行程序，压缩包设置了解压密码，为本站域名`eaglexiang.org`。

## 准备工作

**1.** 首先您需要一台防火墙外的计算机，我使用的是[vultr](https://www.vultr.com/?ref=7357306)的VPS，月付5刀，部分地区有2.5刀的货源，不过安装系统后最好先在国内ping一下是否畅通，最近屏蔽了很多IP。如果不是很熟练Linux的使用，建议安装[CentOS](https://www.centos.org/)。

**2.** 由于Eagle Tunnel基于.NET Core开发，因此需要安装.NET Core的运行库。**VPS和本机都需要安装**。下面列举一些常见的系统对应的安装方式。

### Windows

* [Windows x86_64](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x64-installer)
* [Windows x86](https://www.microsoft.com/net/download/thank-you/dotnet-runtime-2.0.5-windows-x86-installer)

### Archlinux

```shell
yaourt -S dotnet-runtime
```

### CentOS

```shell
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[packages-microsoft-com-prod]\nname=packages-microsoft-com-prod \nbaseurl= https://packages.microsoft.com/yumrepos/microsoft-rhel7.3-prod\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/dotnetdev.repo'
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

## 安装与运行

### 服务器端

此小节默认服务器环境为CentOS 7，其它环境的朋友也可参考。

将下载的压缩包`eagle-tunnel.zip`解压（密码为`eaglexiang.org`）到一个文件夹中，进入该文件夹。使用任何你喜欢的文本编辑器，打开提供的默认配置模板`eagle-tunnel.conf`，该文件位于文件夹`scripts`中，将被安装到`/etc/`。

该文件应该如下所示：

```shell
Remote HTTP Address:0.0.0.0:8080
Remote SOCKS Address:0.0.0.0:8081
Local HTTP Address:0.0.0.0:8080
Local SOCKS Address:0.0.0.0:8081
```

将`Remote HTTP Address`和`Remote SOCKS Address`的`0.0.0.0`更改为VPS的外网IP。假如你的VPS的IP为`1.2.4.8`，则更改后的配置文件应该是下面这个样子：

```shell
Remote HTTP Address:1.2.4.8:8080
Remote SOCKS Address:1.2.4.8:8081
Local HTTP Address:0.0.0.0:8080
Local SOCKS Address:0.0.0.0:8081
```

如果你遇到了莫名其妙的问题，程序也提供了一个简陋的配置工具，可通过下述命令启动：

```shell
dotnet ./bin/Release/netcoreapp2.0/publish/eagle.tunnel.dotnet.core.dll c=./scripts/eagle-tunnel.conf edit
```

如果没有其它问题，执行下列指令进行后续的安装：

```shell
sudo ./install.sh
# 这两条指令只有支持systemd的系统才能执行成功
sudo systemctl enable eagle-tunnel-server.service
sudo systemctl start eagle-tunnel-server.service
# 如果系统没有防火墙，下面几条指令不会执行成功，不过没关系
sudo firewall-cmd --zone=public --add-port=8080/tcp --permanent
sudo firewall-cmd --zone=public --add-port=8081/tcp --permanent
sudo firewall-cmd --reload
```

此时，如果不出意外，服务端已经搭建完成。如果你的系统不支持systemd，也可以考虑使用[supervisor](https://www.baidu.com/s?wd=supervisor+dotnet+core)来实现服务的后台运行。

### 本地端

客户端的使用与服务端大同小异，首先是配置。不过与服务端不同的是，此时我们除了`Remote HTTP Address`和`Remote SOCKS Address`之外，还需要更改`Local HTTP Address`和`Local SOCKS Address`。简单的例子如下：

```shell
Remote HTTP Address:1.2.4.8:8080
Remote SOCKS Address:1.2.4.8:8081
Local HTTP Address:127.0.0.1:8080
Local SOCKS Address:127.0.0.1:8081
```

> 此处的两个本地IP我都配置为`127.0.0.1`，但如果你需要共享代理到内网，可将其设置为本机的内网IP，例如`192.168.0.1`。

如果你是Windows用户，由于Windows的特殊性，自行编辑可能会造成格式混乱，建议使用脚本进行编辑：双击打开目录中的`config.bat`。

配置完成后，Windows用户可直接双击`start.bat`启动程序。Linux用户也可执行脚本`start.sh`（没用过macOS的土鳖我并不知道macOS脚本怎么写 T_T），如果你喜欢，也可将其设置为系统服务开机启动。

```shell
sudo ./install.sh
# 这两条指令只有支持systemd的系统才能执行成功
sudo systemctl enable eagle-tunnel-client.service
sudo systemctl start eagle-tunnel-client.service
# 如果系统没有防火墙，下面几条指令不会执行成功，不过没关系
sudo firewall-cmd --zone=public --add-port=8080/tcp --permanent
sudo firewall-cmd --zone=public --add-port=8081/tcp --permanent
sudo firewall-cmd --reload
```

完成。

### 系统配置

假如你是个萌新，不知道怎么给系统用上刚刚搭建好的代理，建议百度`Windows设置HTTP代理`。我们设置系统代理的时候，会用到这个东西：

配置项 | 含义
--- | ---
Local HTTP Address | HTTP代理用到的地址，IP和端口号中间以':'间隔

### 其它

如果有其它特殊需求，也可自行编写程序启动的命令。运行命令的格式为：

```shell
dotnet [eagle.tunnel.dotnet.core.dll文件的路径] c=[配置文件的路径] [选项]
```

可供选择的选项有：

选项 | 释义
--- | ---
hs | HTTP服务器
ss | SOCKS服务器
hc | HTTP客户端
sc | SOCKS客户端

**四个选项可自由组合。**例如：

```shell
dotnet eagle.tunnel.dotnet.core.dll c=/etc/eagle-tunnel.conf hc sc #启动HTTP(S)代理与SOCKS5代理的本地服务，配置文件为`/etc/eagle-tunnel.conf`
```

## 源码

[repo owner="eaglexiang" name="eagle.tunnel.dotnet.core"]

## 许可证

[LICENSE](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/blob/master/LICENSE)