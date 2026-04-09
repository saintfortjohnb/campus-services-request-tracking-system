# Campus Services Request & Tracking System

**Course:** CIS 4891 – Senior Capstone Project  
**Team:** Capstone Group 2  

---

## 📌 Project Overview
The Campus Services Request & Tracking System is a web-based application designed to improve how campus service requests are submitted, tracked, and managed.

The system replaces informal request methods (email, verbal communication) with a centralized platform that provides visibility, accountability, and structured workflows.

---

## 🎯 Objectives
- Centralize service request submission and tracking
- Improve accountability and visibility across departments
- Standardize request lifecycle management
- Provide basic reporting for operational insights

---

## 👥 User Roles
The system supports role-based access control:

- **Requester**
  - Submit service requests
  - View own requests and status history
  - Add notes to requests

- **Technician**
  - View assigned requests
  - Update request status
  - Add work notes

- **Manager**
  - Assign requests to technicians
  - Monitor request progress
  - Access reporting dashboard

- **Administrator**
  - Manage users, roles, categories, and teams
  - Full system access

---

## 🔄 Request Lifecycle

---

## 🚀 Key Features
- Service request creation with auto-generated tracking numbers
- Category-based routing to service teams
- Technician assignment system
- Status history tracking (automatic logging)
- Notes system for communication and updates
- Role-based UI and access control
- Reporting dashboard with:
  - Request counts (Open, Resolved, Closed)
  - Requests grouped by category and status
  - Filtered CSV export (status, category, date range)

---

## 🧱 Technology Stack
- **Backend:** ASP.NET Core MVC (.NET)
- **Language:** C#
- **Database:** SQL Server / Azure SQL
- **ORM:** Entity Framework Core
- **Frontend:** Razor Views + Bootstrap
- **Version Control:** Git & GitHub

---

## 🔐 Security Features
- Session-based authentication
- Role-based authorization (Requester, Technician, Manager, Admin)
- UI restrictions (users only see allowed actions)
- Controller-level access enforcement

---

## 📊 Reporting
The system includes a reporting dashboard for managers and administrators:
- View request metrics
- Analyze workload by category and status
- Export filtered data to CSV for external analysis

---

## ⚙️ How to Run the Application

1. Clone the repository:
```bash
git clone https://github.com/your-username/campus-services-request-tracking-system.git
