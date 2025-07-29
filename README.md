# MTRoxiesAssistorSolution

An open-source, Docker-based **MTProto Proxy Manager** for Telegram with:

- Dynamic Port Allocation  
- Sponsor Channel Support  
- Fake Domain for Anti-Filtering  

Access your proxies easily through a simple web interface.

---

## ✨ Features

- ✅ Manage multiple MTProto proxies in a single container  
- 🔒 Support for **Fake Domains** to bypass filtering  
- 🔐 Automatic **SSL setup** with Let's Encrypt  
- 🌐 **Web UI** for configuration and proxy management  
- 🤖 Integration with [@MTProxybot](https://t.me/MTProxybot) for registration and sponsorship  
- 🎲 Random port selection  
- ⚙️ Fully automated setup & management  

---

## 📦 Prerequisites

- Ubuntu 22.04  
- Docker & Docker Compose  
- A server with a public IP address  
- Telegram Bot Token from [@BotFather](https://t.me/BotFather)  

---

## 🚀 Installation

### 1. Clone the repository

```bash
git clone https://github.com/ali80da/mtroxies.git
cd mtroxies
```

### 2. Run the setup script

```bash
chmod +x setup.sh
./setup.sh
```

---

## 🌐 Web Interface

- After installation, visit:  
  `http://<server-ip>`  
  to configure your:
  - Subdomain  
  - Email  
  - Admin Username  
  - Telegram Bot Token  

- Once configured, access your dashboard at:  
  `https://<your-subdomain>`

---

## ⚙️ Usage

- **Initial Setup:**  
  Enter subdomain, mail, username, and bot token via setup UI.

- **Create a Proxy:**  
  Specify:
  - Port (or choose random)
  - Telegram Channel ID
  - *(Optional)* Fake Domain (e.g. `domain.com`)

- **View Proxies:**  
  List of all active proxies with clickable Telegram links.

- **Delete a Proxy:**  
  Remove any proxy by its port number.

---

## 📁 Project Structure

```
mtroxies/
├── Sources/
│   ├── Roxi.Core/          # Core logic
│   ├── Roxi.Web/           # API layer
│   ├── Roxi.Client/        # Web interface (UI)
├── Configs/
│   ├── nginx.conf
│   ├── mtproto-config
│   ├── env-config.json
├── Docs/
│   ├── README.md
│   ├── CHANGELOG.md
├── docker-compose.yml
└── setup.sh
```

---

## 🤝 Contributing

Contributions are welcome!  
Please feel free to submit a [pull request](https://github.com/ali80da/mtroxies/pulls) or open an [issue](https://github.com/ali80da/mtroxies/issues) on GitHub.

---

## 📄 License

This project is licensed under the **MIT License**.  
See the `LICENSE` file for more details.

---


