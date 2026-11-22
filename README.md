# agentive-invoice-triage
Full automation of invoice sorting using an agent-based pipeline: visual extraction, business decision-making, and automated action execution. This project demonstrates how to combine .NET, Vision Transformer, LLMs, Docker, and automation into a coherent, industrializable, ROI-oriented workflow.

---

### 📚 **Table of Contents**

1. 🎯 POC Objective
2. 🏗️ Architecture
3. ✨ Key Features
4. 🖼️ Demonstration
5. 🧩 Tech Stack
6. 🛠️ Installation & Execution
7. ⚙️ Configuration
8. 📂 Project Structure
9. 📈 Improvement / Production Axes
10. 📞 Contact

---

### 🎯 **POC Objective**

This POC aims to **replace manual document sorting** with an automated chain based on:

- Visual extraction (OCR + Vision Transformer model)
- An LLM agent capable of making business decisions
- Automated workflows triggered via .NET API or Playwright/Selenium actions

**Demonstrated case**: PDF invoice classification → decision → automated action → logging and traceability

---

### 🏗️ **Architecture**

**Pipeline:** PDF Upload → Vision/OCR Extraction → LLM Agent → Analysis & Decision → Automated Workflow → Logging → Blazor Dashboard

**Components:**

- **.NET 8 API**: ingestion, orchestration, rules, REST exposure
- **Vision Transformer / OCR**: text + metadata extraction
- **LLM Decision Agent**: structured prompt, JSON-formatted responses
- **Automation**: .NET actions or Playwright/Selenium
- **Blazor Dashboard**: UI, workflow monitoring, logs
- **Docker**: isolated containers ready for Kubernetes

---

### ✨ **Key Features**

- 🧠 **Autonomous agent** making business decisions (type, amount, destination)
- 📄 **PDF management**: text + business key extraction
- 🔗 **Automated workflows** (action execution via API)
- 🌐 **Blazor Dashboard UI**: real-time status, logs, document details
- 📦 **Full containerization** (docker-compose)
- 🧪 **Industrialization-ready structure** (optional Helm chart)

---

### 🖼️ **Demonstration**

👉 **Demo video (6–8 minutes)**  
_(Lien YouTube une fois publiée)_

👉 **Presentation slides**  
_(Lien ou dossier /docs)_

---

### 🧩 **Tech Stack**

- **C# / .NET 8**
- **Blazor Server / WASM**
- **Vision Transformer (integrated OCR)**
- **LLM API (OpenAI / Local model / other adapter)**
- **Docker / docker-compose**
- **GitHub Actions** (CI/CD)
- **Kubernetes-ready** (minimal manifest provided)

---

# 🛠️ Installation & execution

### 1) **Cloner the project**

```bash
git clone https://github.com/mermazd/agentive-invoice-triage 
cd agentive-invoice-triage
```
### 2) **Create and fill `.env` file**

```bash
LLM_API_KEY=xxx 
VISION_API_KEY=xxx
```

### 3) **Run locally (Docker)**

```bash
docker-compose up --build
```

### 4) **Interface access**

- **API .NET** : http://localhost:5000
- **Dashboard Blazor** : http://localhost:5100

---

# ⚙️ Configuration

### **Agent Configuration**

```json
{
  "decisionMode": "strict-json",
  "confidenceThreshold": 0.7,
  "actions": ["classify", "route", "validate"]
}
```

### **Vision Configuration**

```json
{
  "ocr": "fast",
  "maxPages": 1,
  "extractFields": ["supplier", "amount", "date", "tax"]
}
```
# 📂 Project Structure

```bash
/src
   /Api            → API .NET 8 (ingestion, orchestration)  
   /AgentEngine    → Agentive code, prompts, decision handlers
   /VisionService  → OCR + Vision Transformer model     
   /Dashboard      → UI Blazor 
/docker         
	docker-compose.yml
/docs
    architecture-diagram.png
    slides.pdf 
/tests
    ...
```

### 📈 **Improvement / Production Axes**

For an enterprise version:
- Agent version management (Agent Registry)
- Observability (Prometheus + Grafana)
- Security (RBAC, anonymization, IAM)
- OCR/vision fine-tuning
- Deployment on AKS / EKS / On-prem K8s
- Complex workflows (ERP, KYC, finance, compliance)

---
