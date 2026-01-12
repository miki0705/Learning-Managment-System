# 🎓 Learning Management System (LMS)

A robust, enterprise-grade desktop solution for private tutoring and language schools. Built with **.NET 9** and **WPF**, this application streamlines student management, scheduling, and automated financial settlements.

![Main Dashboard Placeholder](https://via.placeholder.com/800x450.png?text=LMS+Application+Dashboard+Screenshot)
> *Placeholder: Add a screenshot of your main dashboard here to show off your UI/UX skills.*

## 🏗 Architecture & Design Patterns
This project isn't just a simple CRUD app. It was built with scalability and maintainability in mind:
* **Pattern:** Strict **MVVM (Model-View-ViewModel)** for clean separation of concerns.
* **Business Logic:** Decoupled into a dedicated **Service Layer** (Dependency Injection).
* **Data Access:** **Entity Framework Core** with a Code-First approach.
* **Concurrency:** Fully **Asynchronous** (async/await) to ensure a butter-smooth UI experience.

## 🌟 Key Features

### 📅 Intelligent Scheduling System
* **Smart Sync:** Automatic recalculation of future lessons when a group's schedule changes.
* **Historical Integrity:** System preserves past attendance records while regenerating future "Scheduled" slots.
* **Recurring Logic:** Handles complex recurring patterns for tutoring sessions.

### 💰 Automated Billing & Wallet Engine
* **Virtual Wallet:** Real-time balance tracking for every student.
* **Automated Charging:** Lesson attendance triggers an automatic debit based on `BaseRate` or `IndividualRate` overrides.
* **Transaction Audit Trail:** Full traceability of every financial movement (Payments, Charges, Refunds).

### 👥 Advanced Group Management
* **Member Reconciliation:** Custom logic for synchronizing group memberships, preventing Primary Key conflicts and data duplication during complex UI updates.

---

## 🛠 Tech Stack
* **Framework:** .NET 8/9 (WPF)
* **ORM:** Entity Framework Core
* **Database:** SQL Server / SQLite
* **Tools:** Community Toolkit MVVM, MS Dependency Injection

---

## 📸 Deep Dive: Technical Implementation

### 🛡️ Financial Integrity
Every charge is wrapped in a DB transaction. Here is how the "Automated Charging" logic is handled:

```csharp
// Example of the logic used in BillingService
public async Task ProcessLessonAttendanceAsync(int studentId, int lessonId) {
    // Logic for IndividualRate overrides and Wallet balance updates
}