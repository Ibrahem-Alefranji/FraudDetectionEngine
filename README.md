# 🛡️ Fraud Detection Engine

> A real-time, AI-powered fraud detection system built with .NET 8 and ML.NET  
> Designed to protect financial transactions using intelligent risk scoring and behavioral analysis.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![ML.NET](https://img.shields.io/badge/ML.NET-LightGBM-purple)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20Docker-brightgreen)

---

## 🚀 Project Overview

The **Fraud Detection Engine** is a modular, extensible .NET-based system that detects and classifies fraudulent financial transactions in real time. It uses a custom-trained machine learning model (LightGBM via ML.NET) and evaluates both transaction-level features and historical user behavior to make smart decisions.

This project is ideal for:
- Fintech developers
- E-commerce systems
- Banking APIs
- Academic and graduation projects focused on AI and cybersecurity

---

## 🧠 Key Features

- ✅ **ML.NET-based binary classification model** for predicting fraud
- ✅ **Behavior enrichment engine** (analyzes user spending history)
- ✅ **Three-tier fraud action logic**: Allow, Challenge (OTP), or Block
- ✅ **Real-time prediction via API or RabbitMQ integration**
- ✅ **Clean architecture**: model training, prediction, enrichment, logging
- ✅ Built entirely with **.NET 8** and **C#**

---

## 📊 Tech Stack

| Layer              | Technology            |
|-------------------|------------------------|
| Machine Learning  | ML.NET (LightGBM)      |
| Backend API       | ASP.NET Core (.NET 8)  |
| Messaging         | RabbitMQ (optional)    |
| Data Access       | Dapper + SQL Server    |
| Training Data     | SQL (joined transaction + behavior) |

---

## 💡 Decision Engine

The engine makes fraud decisions based on prediction **probability**:

| Probability Range | Decision   | Description                     |
|-------------------|------------|---------------------------------|
| `>= 0.90`         | 🔥 Block    | High-risk → reject immediately  |
| `>= 0.75`         | 🔐 Challenge | Medium-risk → OTP verification |
| `< 0.75`          | ✅ Allow    | Low-risk → allow transaction   |

---

## 📂 Modules

- `ModelTrainerService` – trains and evaluates your ML model from SQL data  
- `FraudDecisionService` – predicts risk and returns a decision  
- `UserBehaviorService` – enriches transactions with historical behavior  
- `TransactionPrediction` – ML.NET prediction output  
- `RabbitMqListenerService` – (optional) listens to real-time transaction queue  

---

## 📦 Status

✔️ Model training tested with synthetic + real data  
✔️ Fully functional scoring engine  
🔜 Admin dashboard and training UI (planned)

---

## 📄 License

MIT License – free for commercial and academic use.  
Credits required when used in research or publications.
