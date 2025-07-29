MTRoxiesAssistorSolution
An open-source, Docker-based MTProto Proxy Manager for Telegram with Dynamic Port Management, Sponsor Channel Support, Snd Fake Domain for Anti-Filtering.


Features

Manage Multiple MTProto ProxiesUusing Single Container.
Support For Fake Domain to Enhance Anti-Filtering.
Automatic SSL Setup With Let's Encrypt.
Web Interface For Initial Configuration And Proxy Management.
Integration With @MTProxybot For Proxy Registration And Sponsorship.
Random Port Selection For Ease of use.
Fully Automated Setup And Management.


Prerequisites

Ubuntu 22.04
Docker and Docker Compose
A Server with a Public IP
A Telegram Bot Token From @BotFather

Installation

Clone The Repository:git clone https://github.com/ali80da/mtroxies.git
cd mtroxies


Run The Setup Script:chmod +x setup.sh
./setup.sh


Access The Web Interface at http://<server-ip> to Configure The Subdomain, Mail, UserName And Telegram Bot Token.
After Configuration, use The Web Interface at https://<subdomain> to Manage Proxies.

Usage

Initial Setup: Enter Your Subdomain, Mail, UserName And Telegram Bot Token in The Setup Program.
Create a Proxy: Specify a Port (or use random), Telegram Channel ID, And an Optional Fake Domain (e.g., domain.com).
View Proxies: List all active proxies with their Telegram links.
Delete a Proxy: Remove a Proxy by Port.

Project Structure

mtroxies/
├── Sources/
│   ├── Roxi.Core/          # Core LOGIC
│   ├── Roxi.Web/           # API
│   ├── Roxi.Client/        # Web Interface (UI)
├── Configs/
│   ├── nginx.conf
│   ├── mtproto-config
│   ├── env-config.json
├── Docs/
│   ├── README.md
│   ├── CHANGELOG.md
├── docker-compose.yml
└── setup.sh

Contributing
Contributions are Welcome! Please Submit Pull Requests or Issues on GitHub.


License




