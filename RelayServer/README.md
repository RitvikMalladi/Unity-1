# Relay Server — Deployment Guide (AWS EC2)

This is the small "switchboard" server that lets your phone (the
Controller app) talk to your game (the WebGL build, or an Android/PC
build) over the internet, instead of over the same Wi-Fi network.

It does not know anything about driving or cars — it just puts two
devices that type in the same **room code** into the same "room" and
forwards messages between them.

You only need to set this up **once**. After that, every time you play,
you (or a friend) just needs a room code — no IP addresses, no
Wi-Fi requirement.

---

## 1. Launch an EC2 instance

1. Go to the [AWS EC2 console](https://console.aws.amazon.com/ec2/) and click **Launch instance**.
2. **Name**: anything, e.g. `car-racing-relay`.
3. **AMI (operating system)**: Ubuntu Server 22.04 LTS (or newer).
4. **Instance type**: `t3.micro` or `t2.micro` is plenty — this server does almost no work.
5. **Key pair**: create a new one and download the `.pem` file. You'll need it to log in over SSH.
6. **Network settings** → Edit → add these inbound rules (besides the default SSH one):
   - Type `HTTP`, port `80`, source `Anywhere`
   - Type `HTTPS`, port `443`, source `Anywhere`
7. Click **Launch instance**.
8. Once it's running, note its **Public IPv4 address**.

## 2. Point a domain name at it

`wss://` (secure WebSocket) requires a real hostname with a valid TLS
certificate — an IP address alone won't work, and your WebGL page
(served over `https://`) will refuse to connect to anything else.

1. If you don't already have a domain, buy a cheap one (Namecheap,
   Route 53, etc.) — even a subdomain of one you already own is fine,
   e.g. `relay.yourdomain.com`.
2. In your domain's DNS settings, add an **A record** pointing
   `relay.yourdomain.com` at the EC2 instance's public IP address.
3. Wait a few minutes for DNS to propagate. You can check with:
   ```bash
   ping relay.yourdomain.com
   ```
   It should resolve to your EC2 instance's IP.

## 3. Connect and install Node.js

SSH into the instance (replace the path/user/IP with your own):

```bash
chmod 400 your-key.pem
ssh -i your-key.pem ubuntu@YOUR_EC2_PUBLIC_IP
```

Install Node.js (v18 LTS) and nginx:

```bash
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs nginx
node -v   # should print v18.x
```

## 4. Upload and start the relay server

From your own computer (not the EC2 instance), copy this folder up:

```bash
scp -i your-key.pem -r RelayServer ubuntu@YOUR_EC2_PUBLIC_IP:~/relay
```

Back on the EC2 instance:

```bash
cd ~/relay
npm install
```

Run it in the background permanently with `pm2` (a process manager that
restarts the server if it crashes or the instance reboots):

```bash
sudo npm install -g pm2
pm2 start server.js --name relay
pm2 save
pm2 startup   # follow the one printed command it gives you, then re-run: pm2 save
```

The server is now listening on `ws://localhost:8080` — but only inside
the EC2 instance. Next we make it reachable securely from the internet.

## 5. Set up HTTPS/WSS with nginx + Let's Encrypt

nginx will sit in front of the Node.js server: it terminates TLS
(handles the `https://`/`wss://` encryption) and forwards plain
`ws://` traffic to `localhost:8080` internally.

Create the nginx site config:

```bash
sudo nano /etc/nginx/sites-available/relay
```

Paste this (replace `relay.yourdomain.com` with your actual domain):

```nginx
server {
    listen 80;
    server_name relay.yourdomain.com;

    location /relay {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_read_timeout 3600s;
    }
}
```

Enable it and reload nginx:

```bash
sudo ln -s /etc/nginx/sites-available/relay /etc/nginx/sites-enabled/
sudo nginx -t          # should say "syntax is ok"
sudo systemctl reload nginx
```

Now get a free TLS certificate with Certbot — it will edit the nginx
config above automatically to add the HTTPS/443 block:

```bash
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d relay.yourdomain.com
```

Follow the prompts (enter your email, agree to the terms). Certbot also
sets up automatic renewal, so you don't need to repeat this.

## 6. Test it

From your own computer:

```bash
curl -I https://relay.yourdomain.com
```

You should get an HTTP response (not a connection error). The actual
WebSocket endpoint your Unity project will use is:

```
wss://relay.yourdomain.com/relay
```

## 7. Point Unity at your relay

Open `Assets/Racing_Game/Scripts/RemoteInput/RelayConfig.cs` and set:

```csharp
public const string ServerUrl = "wss://relay.yourdomain.com/relay";
```

That's the **only** line you need to change in the whole project —
every script (controller and game) reads the server address from here.

---

## How the room codes work

- The game (WebGL build or Android/PC build) generates a random
  4-digit room code the first time it runs and remembers it
  (`PlayerPrefs`) — it shows on-screen in the Garage and on the race
  HUD.
- Type that same code into the phone Controller app and tap **CONNECT**.
- Both devices join the same "room" on the relay server, which then
  forwards driving input and garage commands between them.
- The code doesn't change between sessions on the same device, so once
  you've paired once you can usually reuse the same code — but if it's
  ever taken by someone else's device at the same time, just tap
  **CONNECT** again after checking the code on the game screen.

## Updating the server later

```bash
scp -i your-key.pem -r RelayServer ubuntu@YOUR_EC2_PUBLIC_IP:~/relay
ssh -i your-key.pem ubuntu@YOUR_EC2_PUBLIC_IP
cd ~/relay && npm install && pm2 restart relay
```

## Cost

A `t3.micro`/`t2.micro` instance is within the AWS free tier for the
first 12 months on a new account, and costs only a few dollars a month
after that — this relay uses negligible CPU/bandwidth since it just
forwards small JSON messages.
