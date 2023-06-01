
# Purpose
The purpose of this directory is to get application working in a multi Docker containers through use of configuration over a single or multiple machines, then to deploy and scale it using Docker Swarm or Kubernetes.

# Install Prerequesits
#### Ubuntu: Ensure you have the latest version of Docker-CE by following the install steps *https://docs.docker.com/install/linux/docker-ce/ubuntu/*

    sudo apt-get install -y apt-transport-https ca-certificates curl gnupg-agent software-properties-common
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
    sudo apt-key fingerprint 0EBFCD88
    sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
    sudo apt-get update
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io

#### Ubuntu: Install docker-compose
    sudo apt-get install docker-compose
OR

    sudo curl -L https://github.com/docker/compose/releases/download/1.17.0/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose

#### Set Executable permission to docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    sudo usermod -aG docker ${USER}

#### Verify Docker installation
    docker --version
    docker-compose --version
    sudo docker run hello-world

# Deploy Application 

* Upload docker folder to server 
* Copy "appsettings.json" file from "Aspdotnetcore/src/EMRO.Web.Host/appsettings.json" directory to "docker/Aspdotnetcore/appsettings.json" 
* Copy "App_Data" folder from "Aspdotnetcore/src/EMRO.Web.Host/App_Data" to "docker/Aspdotnetcore/App_Data" directory 
* Copy "wwwroot" folder from "Aspdotnetcore/src/EMRO.Web.Host/wwwroot" to "docker/Aspdotnetcore/wwwroot" directory 
* Change ENV variable for api container in "docker/docker-compose.yml" file


* Change ENV variable for portal container in "docker/docker-compose.yml" file


* Change Nginx configuration for domain pointing

        sed -i -- 's/uatapi.emro.cloud/{your-api-domain}/g' *
        sed -i -- 's/uatportal.emro.cloud/{your-portal-domain}/g' *
        sed -i -- 's/samvaad.emro.cloud/{your-samvaad-domain}/g' *

* Run application containers

    * Command to login docker registry

            docker login registry.emorphis.com -u username -p password

    * Command to launch container from project root directory

            docker-compose up -d

    * Check status of all container

            docker ps -a

    * Check logs of faild container

            docker logs container-id

* configure cron to renew SSL certificate if actual domain

        sudo chmod +x /path-to-project-root/docker/renew-cert.sh
        sudo crontab -e
        # add below line to crontab
        0 12 * * * /path-to-project-root/docker/renew-cert.sh >> /var/log/cron.log 2>&1

* If using self signed domain add host file entry for domain and server IP

## Shutdown application

* Command to shutdown container

        docker-compose down

* Delete all unused containers and images

        docker system prune -af

## Create Docker Images

* Command to login docker registry

        docker login registry.emorphis.com -u username -p password

* Build all application images *Run below commands from project root directory.*

        docker build -f Aspdotnetcore/Dockerfile -t registry.emorphis.com/root/emro/api:latest Aspdotnetcore/.
        docker build -f Angular/Dockerfile -t registry.emorphis.com/root/emro/portal:latest Angular/.
