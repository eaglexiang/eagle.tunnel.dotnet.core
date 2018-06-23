sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[packages-microsoft-com-prod]nname=packages-microsoft-com-prod nbaseurl= https://packages.microsoft.com/yumrepos/microsoft-rhel7.3-prodnenabled=1ngpgcheck=1ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/dotnetdev.repo'
sudo yum -y update
sudo yum -y install libunwind libicu
sudo yum install dotnet-sdk-2.1.4