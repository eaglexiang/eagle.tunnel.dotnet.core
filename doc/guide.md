# 使用方法

## 准备工作

1. 首先您需要一台防火墙外的计算机，我使用的是[vultr](https://www.vultr.com/?ref=7357306)的VPS，月付5刀，部分地区有2.5刀的货源。不过安装系统后最好先在国内ping一下是否畅通，最近屏蔽了很多IP。如果不是很熟练Linux的使用，建议给VPS安装CentOS 7，因为下文会以CentOS 7为标准。当然，作为老鸟你也可以使用任意你喜欢的操作系统,推荐 **ArchLinux**。

2. 由于Eagle Tunnel基于.NET Core开发，因此需要安装.NET Core的运行库。**VPS和本机都需要安装**。可到[.NET Core 官网](https://www.microsoft.com/net/download/dotnet-core/runtime-2.1.1)自行下载安装。

## 服务器端配置

简单来说，配置过程为：

1. 安装dotnet
2. 下载并安装 Eagle Tunnel
3. 基本设置
4. 启动服务

此小节默认服务器环境为CentOS 7，并且会使用最新的master分支（而非编译好的字节码文件，因此会用到git，这样便于用上最新的特性及Bug修复）。其它环境的朋友也可参考。

> Archlinux用户可直接执行下列指令一键安装（ **但仍需执行后续配置步骤**）：

```shell
yaourt -S eagle-tunnel
```

> **提示：** 以下的步骤中，提供安装dotnet、git的命令和脚本只是为了方便部分朋友，情况很可能由于其官方的更新而发生改变。如果安装失败，也许是因为教程未跟上官方的节奏，建议自行搜索安装方法替代。当然，发信让我修复也是欢迎的。

### 安装git（如果已安装可跳过）

打开终端（shell）并执行以下指令

```shell
sudo yum install -y git
```

### 下载Eagle Tunnel

```shell
mkdir ~/git
cd ~/git
git clone --recursive https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core
```

### 安装dotnet sdk（如果已安装可跳过）

```shell
# 安装dotnet core sdk
sudo ./scripts/dotnet/centos.sh
```

### 编译安装Eagle Tunnel

```shell
./build.sh
sudo ./install.sh
```

此时 Eagle Tunnel 已经安装完毕。接下来我们需要对其进行配置，默认的配置文件已被安装到`/etc/eagle-tunnel.conf`。

### 基本设置

ET的配置是非常简单的。

```shell
sudo vim /etc/eagle-tunnel.conf # 用你喜欢的编辑器打开配置文件
```

添上或修改以下内容：

```shell
# 如果你不清楚VPS的IP为多少，可通过ifconfig命令进行查询。
Listen = VPS的IP # 例如 8.8.8.8
et = on
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

你也可以尝试执行命令`eagle-tunnel --check`来进行自动的配置检查（测试功能），能帮助你排除一些常见配置错误。

> 如果你的系统存在防火墙，一定要将`Listen`对应的端口打开。你可能需要执行以下指令：

```shell
sudo firewall-cmd --add-port=8080/tcp --permanent
sudo firewall-cmd --reload
```

## 客户端配置

### Windows用户

对于Windows用户，推荐直接使用[Windows版](https://github.com/eaglexiang/eagle.tunnel.dotnet)的Eagle Tunnel。

> **注意：** Windows用户也需要先到[官网](https://www.microsoft.com/net/download/windows)自行下载安装dotnet core运行时。

### 支持systemd的Linux定制版

首先仍然是安装 Eagle Tunnel 及其依赖项（dotnet runtime 和 git）。

```shell
# 安装git
sudo yum install -y git
# 下载ET
git clone --recursive https://github.com/eaglexiang/eagle.tunnel.dotnet.core.git
cd ./eagle.tunnel.dotnet.core
# 安装dotnet
sudo ./scripts/dotnet.centos.sh
# 安装ET
./build.sh
sudo ./install.sh
```

然后同样是编辑配置文件。经历了服务端配置，也许会有细心的朋友发现，`/etc`目录下一共存在两个配置文件：

* `/etc/eagle-tunnel.conf` 供普通模式默认调用
* `/etc/eagle-tunnel.smart.conf` 供智能模式默认调用

在这里先介绍最简单的普通模式。智能模式的介绍会放在后文，您可以使用浏览器的页内搜索功能快速找到它。

```shell
sudo vim /etc/eagle-tunnel.conf # 用你喜欢的编辑器打开配置文件
```

并添上或修改以下选项：

```shell
Relayer=VPS的IP # 与前文中的Listen=相同 如8.8.8.8
Listen=127.0.0.1 # 注意此处，即为本地代理服务的地址，也就是应用程序里填写的代理地址
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

配置文件的规则请参考上一小节。文件为`eagle-tunnel.conf`，位于`publish/etc/`文件夹中。稍有不同的是，我们此时还需要更改`config-dir`属性为`./publish/etc/`。

操作系统 | 脚本名
--- | ---
Linux / macOS | run.sh
Windows | run.bat

## 系统配置

本程序只提供代理服务本身的功能，并不会自动配置您的系统代理，也就是说这需要您手动进行设置。系统的自动配置在带图形界面的版本（即[Windows版本](https://github.com/eaglexiang/eagle.tunnel.dotnet)）中提供支持。假如你是萌新，不知道怎么给系统用上刚刚搭建好的代理，建议百度类似`Windows设置HTTP代理`的关键词。在我们设置系统代理的时候，常会用到`代理地址`和`端口号`这两个东西，代理地址可从配置文件中找到。例如在本文示例配置中的`127.0.0.1`中，`127.0.0.1`就是代理地址；端口号则默认为`8080`。

如果手机端想使用 Eagle Tunnel 提供的服务，可参照下文将服务共享到内网，然后手动设置手机WIFI的HTTP代理。如果 Android 平台一定要使用 SOCKS 代理，可考虑使用[Postern](https://play.google.com/store/apps/details?id=com.tunnelworkshop.postern)这款APP。

## 高级

### 更新

ET并没有自带的更新功能，不过可以通过git来将你的ET更新到最新版本：

```shell
cd ~/git/eagle.tunnel.dotnet.core

git pull # 更新源码
git submodule update # 更新子模块
./build.sh # 编译
sudo ./install # 安装
```

### 共享服务

按照上文示例，代理服务只会被提供给本机。分享服务到内网的方法是有的，只需要简单地将`Listen`参数设置为内网IP（例如192.168.0.2）即可。

### 自定义端口号

ET 的默认端口号为8080，如果你有特殊原因（例如8080端口已被占用），可以通过修改Listen参数的方法修改 ET 的监听端口。例如上文的`Listen`如下：

```shell
Listen = 127.0.0.1
```

我们可以为其添加`:端口号`后缀，来手动设置端口。如下：

```shell
Listen = 127.0.0.1:8080
```

### 负载均衡

如果你拥有多台VPS，想充分利用它们的流量，提高上网的速度，可以考虑开启Eagle-Tunnel的负载均衡功能。开启方法很简单，只需要在上文提到的配置文件中，按照格式换行添加额外的`Relayer`，然后重启服务即可。例如：

```shell
Relayer=1.2.4.8
Relayer=8.8.8.8
```

> 需要注意的是：
>
> 1. 负载均衡只可能提高多个TCP连接并行传输的总速度，并不能提高单个TCP连接的速度。但同一网页经常拥有多个连接，因此启用负载仍然是有好处的。但并不能额外增加在线流媒体（如YouTube）的流畅度。
> 2. 如果你的多台VPS位于不同的国家或地区，可能会造成反效果。这是因为浏览器有可能通过VPS1访问了VPS2所在地区的网站节点，这可能导致性能低下。因此仅建议为同地区的不同VPS启用负载均衡。

### 用户认证

如果你暴露了自己的服务器IP和端口，任何使用Eagle-Tunnel客户端的人都能轻易连上你的服务，从而盗用你的流量。解决方法是为你的代理服务开启用户认证功能。

在服务端中开启用户认证的步骤为：

1. 编辑`/etc/eagle-tunnel.conf`文件，添加参数：`user-check = on`。
2. 编辑`/etc/eagle-tunnel.d/users.list`文件，在其中按照下述规则添加用户：
3. 重启服务

添加规则：

```shell
用户名1:用户密码1
用户名2:用户密码2
```

每一行代表一组用户名和密码，例如一行`abc:def`便代表一个用户名为abc的账户，密码为def。

在本地服务（客户端）中开启用户认证的方法为，编辑`etc/eagle-tunnel.conf`文件，添加或修改`user`参数，为其按照同样的格式填上帐号和密码：

```shell
User = 账户名:密码
```

**服务端与客户端必须同时打开或关闭用户认证功能**，单方面开启或关闭都会造成连接失败。

### 用户限速

如果你需要将代理临时共享给朋友，又担心他的滥用，可以为他分配一个访客账户，并将其限速。限速功能依赖于上一小节的用户认证功能。开启方法为，在`/etc/eagle-tunnel.conf`文件中添加或修改：

```shell
speed-check = on # 打开速度检测
speed-limit = on # 打开速度限制
```

然后在用户列表文件`/etc/eagle-tunnel.d/users.list`中，为对应[用户民:密码]组添加限速后缀[:数值]（单位为KB/s），如下：

```shell
id:key:500
# 这一行的含义是，一个ID为id的账户，密码为key，限速为500KB/s。
```

### 智能分流（智能模式）

普通模式下，ET的所有网络操作都是通过Relayer间接完成的，这样做的好处是稳定的服务，坏处是通过代理连接到国内的网络服务（如爱奇艺，B站等），往往会增大网络的延迟，降低上下行的网络速度，还可能被告知类似“您所处的地区无法观看该版权作品”的信息。

因此ET提供了一个可选的智能分流功能，它由参数`proxy-status`控制：

Proxy-Status 的赋值 | 含义
--- | ---
enable | （默认）普通模式
disable | 禁用模式
smart | 智能模式

ET在三种模式下的表现分别是这样的：

模式 | DNS解析的表现 | IP转发的表现
--- | --- | ---
普通模式 | 全部使用代理加密通道 | 全部使用代理加密通道
禁用模式 | 全部使用本地DNS解析 | 全部使用本地直连
智能模式 | `whitelist_domain.txt`文件中声明的域名使用代理，其余使用本地解析 | 使用[ip2c](https://ip2c.org/)提供的公开服务，在线判断IP所在地是否为中国大陆。

> 请注意，为避免对ip2c服务的浪费，IP所在地判断操作被放置在指定队列统一操作，这可能会造成延迟(通常为5~10秒）。所以如果在智能模式下发现国内网站打开缓慢，不妨等待十秒再刷新试试。

ET自带一个配置文件为`eagle-tunnel.smart.conf`，本质和格式上它和`eagle-tunnel.conf`没有任何却别，仅仅默认多了`proxy-status = smart`参数。该文件供智能模式的systemd服务（`eagle-tunnel-smart.service`）默认调用，如果该文件得到配置，就能很方便地启动独立的、智能的`eagle-tunnel-smart`服务。

```shell
sudo systemctl enable eagle-tunnel-smart.service
sudo systemctl start eagle-tunnel-smart.service
```

启动智能模式后，可百度关键词`IP`，首先应该会显示VPS的IP，等待十秒到一分钟之后刷新网页，应该会显示本机IP，此时智能模式启用成功。

> **警告** 如果同时启用`eagle-tunnel.service`与`eagle-tunnel-smart.service`，需要手动配置至少其中一个的监听端口不为8080，否则会产生监听冲突，导致两个服务只能正常启用一个。

### 超时

为了防止异常连接长时间占用资源,ET提供了可选的超时参数。单位为ms（毫秒），默认值为0（表示不开启超时检测）。

超时参数是有副作用的，对于一些保持长连接却低网络活跃度的网站（例如斗鱼），它是不友好的，极有可能由于误杀连接造成视频中断。所以即便需要开启，一般也只建议在relayer（服务器）处使用TimeOut参数，在本地则不需要。并配合智能模式使用。

### 可用参数一览表

参数名（大小写不敏感） | 类型 | 默认值 | 作用
--- | --- | --- | ---
Relayer | 主机地址 | 无 | 远端服务地址
Listen | 主机地址 | 无 | 本地服务监听地址
HTTP | 布尔值 | off | HTTP代理开关
SOCKS | 布尔值 | off | SOCKS代理开关
ET | 布尔值 | off | Eagle Tunnel协议开关
Proxy-Status | 可选参数（见后表） | enable | 代理服务的模式
Worker | 整数 | 500 | 受理请求的并发数（值越大，并发能力越强，可能的资源消耗越高）
User | 字符串 | 无 | 本地服务使用的账户密码，凭此与远端进行认证交互，注释状态表示关闭认证功能
User-Check | 布尔值 | off | 是否开启用户认证功能
Speed-Check | 布尔值 | off | 是否开启速度检测特性
Speed-Limit | 布尔值 | off | 基于帐号系统和速度检测的帐号限速功能开关，打开它的前提是`speed-check`也必须打开
TimeOut | 整数 | 0 | 超时时间（ms），0代表关闭超时检测
Config-Dir | 字符串 | /etc/eagle-tunnel.d/ | 配置文件目录路径

Proxy-Status可选的参数

可选参数 | 含义
--- | ---
enable | 全局代理
smart | 智能代理
disable | 禁用代理

### 帮助

除了systemd服务和启动脚本，配合正确的参数，你也可以通过命令手动控制ET的运行，它通常是这样的：

```shell
dotnet eagle.tunnel.dotnet.core.dll [options...]
# 如果执行过安装，可以直接使用 eagle-tunnel 替代 dotnet eagle.tunnel.dotnet.core.dll
# 此时它长这样：
eagle-tunnel [options...]
# 一个典型的例子是帮助指令
eagle-tunnel --help
# 你可以通过它获取其它受支持的参数及其提供的功能
```

## 许可证

[LICENSE](https://github.com/eaglexiang/eagle.tunnel.dotnet.core/blob/master/LICENSE)