# 🏦 SecureBank Transaction & Support System

> A multithreaded client-server banking simulation built in C# .NET — demonstrating TCP, UDP, and concurrent socket programming.

---

## 📌 Overview

**SecureBank** is a comprehensive networking project that simulates a real-world banking communication system. It demonstrates core concepts of socket programming by implementing a unified architecture where a central multithreaded server concurrently manages multiple client connections across three distinct functional modules.

Built as part of the **Data Communication course (2025–2026)** under TA Ragab S. Bakhit.

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────┐
│                  SecureBank Server                  │
│                                                     │
│   TCP Socket (Port 8000)      UDP Socket (Port 8001)│
│          │                           │              │
│    ┌─────▼──────┐             ┌──────▼─────┐        │
│    │ ThreadPool │             │ UDP Thread │        │
│    └─────┬──────┘             └──────┬─────┘        │
│          │                           │              │
│   ┌──────┴──────┐             ┌──────▼─────┐        │
│   │             │             │ Live Ticker│        │
│   ▼             ▼             └────────────┘        │
│ Banking       Chat                                  │
│ Handler       Handler                               │
└─────────────────────────────────────────────────────┘
         ▲                  ▲                ▲
         │                  │                │
    Client (TCP)       Client (TCP)     Client (UDP)
    Banking Mode       Chat Mode        Rates Mode
```

---

## ✨ Features

### Module A — Banking Transactions (TCP)
- Deposit and withdraw funds using structured commands (`DEPOSIT:100`, `WITHDRAW:50`)
- Real-time balance tracking with overdraft protection
- Confirmation or error response for every transaction
- Thread-safe balance updates using `lock`

### Module B — Support Chatbot (TCP)
- Natural language keyword detection (`help`, `hours`, `loan`, `balance`, `transfer`)
- Automated predefined responses over persistent TCP connection
- Graceful session termination with `exit` command

### Module C — Live Exchange Rates (UDP)
- Connectionless rate requests using `GET_RATES`
- Simulated real-time EGP exchange rates (USD, EUR, GBP, SAR)
- No handshake overhead — pure UDP datagram communication

### Concurrency
- Multithreaded server using `ThreadPool.QueueUserWorkItem`
- Main thread never blocks — always ready for new connections
- UDP listener runs on a dedicated background thread
- Thread-safe shared state with `lock`

---

## 🛠️ Tech Stack

| Component | Technology |
|---|---|
| Language | C# (.NET Framework) |
| TCP Communication | `System.Net.Sockets.Socket` — Stream/TCP |
| UDP Communication | `System.Net.Sockets.Socket` — Dgram/UDP |
| Concurrency | `System.Threading.ThreadPool` |
| Encoding | `System.Text.Encoding.ASCII` |

---

## 📁 Project Structure

```
SecureBank/
│
├── SecureBankServer/
│   └── Program.cs          ← Full multithreaded server
│       ├── Main()           ← TCP accept loop + UDP thread startup
│       ├── HandleClient()   ← ThreadPool entry point per client
│       ├── HandleBanking()  ← Module A logic
│       ├── HandleChat()     ← Module B logic
│       ├── StartUDPListener() ← Module C logic
│       ├── ProcessCommand() ← DEPOSIT/WITHDRAW parser
│       ├── GetBotReply()    ← Keyword matching engine
│       └── BuildRates()     ← Simulated exchange rate generator
│
├── SecureBankClient/
│   └── Program.cs          ← Interactive console client
│       ├── Main()           ← Mode selection menu
│       └── GetLiveRates()   ← UDP rate fetcher
│
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) installed
- Any IDE: Visual Studio, VS Code with C# extension

### Setup & Run

**1. Clone the repository**
```bash
git clone https://github.com/yourusername/SecureBank.git
cd SecureBank
```

**2. Start the Server first**
```bash
cd SecureBankServer
dotnet run
```

You should see:
```
[TCP] Server running on port 8000...
[UDP] Listener running on port 8001...
Ready for multiple clients...
```

**3. Start one or more Clients**
```bash
cd SecureBankClient
dotnet run
```

**4. Select a mode from the menu**
```
=== SecureBank Client ===
1 - Banking
2 - Support Chat
3 - Live Exchange Rates
Select mode:
```

---

## 💻 Usage Examples

### Banking Mode
```
Select mode: 1
Connected!
BANKING MODE: Starting balance $1000

Enter command: DEPOSIT:500
Server: SUCCESS: Deposited $500. New balance: $1500

Enter command: WITHDRAW:200
Server: SUCCESS: Withdrew $200. New balance: $1300

Enter command: WITHDRAW:9999
Server: ERROR: Insufficient funds. Balance: $1300

Enter command: exit
Server: Session ended. Final balance: $1300
```

### Support Chat Mode
```
Select mode: 2
Connected!
CHAT MODE: Welcome to SecureBank Support!

You: I need help with my loan
Server: We offer personal loans from 8% interest. Visit any branch.

You: what are your hours
Server: Open Sunday to Thursday, 9AM to 5PM.

You: exit
Server: Goodbye!
```

### Live Rates Mode
```
Select mode: 3
Fetching live exchange rates...

=== LIVE RATES ===
USD: 49.13 EGP
EUR: 52.87 EGP
GBP: 61.74 EGP
SAR: 13.42 EGP
==================
```

---

## 🔑 Key Concepts Demonstrated

| Concept | Implementation |
|---|---|
| TCP three-way handshake | `Connect()` → `Accept()` |
| Persistent connection | `while(true)` Send/Receive loop |
| Connectionless communication | `SendTo()` / `ReceiveFrom()` |
| Non-blocking server | `ThreadPool.QueueUserWorkItem()` |
| Thread safety | `lock(balanceLock)` on shared balance |
| Protocol selection | TCP for reliability, UDP for speed |
| Graceful shutdown | `Shutdown()` then `Close()` |

---

## 🔌 Port Reference

| Port | Protocol | Module |
|---|---|---|
| 8000 | TCP | Banking + Chat |
| 8001 | UDP | Live Ticker |

---

## 📚 Learning Objectives Met

- ✅ C# .NET Socket API implementation
- ✅ TCP vs UDP protocol selection based on requirements
- ✅ Concurrent server architecture with ThreadPool
- ✅ Structured command parsing (`DEPOSIT:100`)
- ✅ Keyword-based natural language processing
- ✅ Thread-safe shared state management

---

## 🔮 Possible Extensions

- [ ] **GUI Client** — WinForms or WPF with async UI updates
- [ ] **Authentication** — Login handshake before transactions
- [ ] **Server Logging** — Timestamped `server_log.txt` audit trail
- [ ] **Overdraft alerts** — Proactive balance warnings
- [ ] **Multiple accounts** — Dictionary-based account management

---

## 👨‍💻 Author

Built for **Data Communication Course — 2025/2026**
Faculty of Computers and Information

---

## 📄 License

This project is for educational purposes.
