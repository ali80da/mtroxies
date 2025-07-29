#!/bin/bash
echo "Installing dependencies..."
sudo apt-get update
sudo apt-get install -y docker.io docker-compose
sudo systemctl enable docker
sudo systemctl start docker

echo "Creating configuration directories..."
mkdir -p Configs
touch Configs/nginx.conf Configs/mtproto-config Configs/env-config.json

echo "Starting services..."
docker-compose up -d

echo "Setup complete! Access the web interface at http://<your-server-ip> to configure the subdomain and other settings."