# Clinic CF

A comprehensive, production-grade **Local Client-Server Medical Desktop Application** built using **C# .NET 8 WPF**, **Entity Framework Core (Code-First)**, and the **CommunityToolkit.Mvvm** framework.

This application is designed specifically for clinic environments operating over a Local Area Network (LAN). It optimizes storage by keeping the database light, writing heavy echocardiogram media files to a central Windows Shared Folder, and logging references dynamically.

---

## 🚀 Key Features

* **Patient Registration Hub:**
  * Fast lookup search by Patient Name or Phone Number.
  * Form registration fields (Name, Age, Gender, Address, Phone).
* **Clinical Examination Timeline:**
  * Log chief complaint, History of Present Illness (HPI), physical exams, diagnosis, and treatment plans.
  * Interactive timeline rendering historic visits in reverse chronological order.
* **Echocardiogram Hub:**
  * Asynchronous background media file upload (`.mp4`, `.avi`, `.jpg`, `.png`, `.dcm`).
  * Unique file renaming (`PatientId_Timestamp_GUID`) to guarantee zero name collisions.
  * Responsive UI which prevents locks during network file operations.
* **Shared State Coordination:**
  * Uses a central singleton state manager. Selecting a patient in the registry immediately updates the global header banner and refreshes the context for clinical exams and echo uploads.

---

## 🛠️ Architecture & Tech Stack

* **Frontend:** WPF (.NET 8) with MVVM architecture.
* **MVVM Framework:** `CommunityToolkit.Mvvm` (using source generators for properties and commands).
* **Database & ORM:** SQL Server with Entity Framework Core (Code-First).
* **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`.
* **Configuration:** JSON settings (`appsettings.json`) for connection strings and shared path.

```
/MedicalApp
├── /Data           - AppDbContext and database mapping configurations
├── /Models         - EF Core database entities (Patient, Visit, EchoRecord)
├── /Services       - Database CRUD services and Shared State manager (Singleton)
├── /ViewModels     - MVVM ViewModels coordinating views and commands
├── /Views          - XAML UserControls, App.xaml and MainWindow shell
├── /Styles         - Theme.xaml defining color palettes and control styles
└── /Converters     - XAML Null-to-Boolean and Null-to-Visibility helpers
```

---

## ⚙️ Setup & Installation

### 1. Database Configuration
Ensure a SQL Server instance is reachable on your local network (e.g. at server IP `192.168.1.100`). The database user must have permissions to create and modify tables.

### 2. Shared File Storage Configuration
1. On your server PC, create a folder for media storage (e.g. `C:\EchoFiles`).
2. Right-click the folder, go to **Properties** -> **Sharing** -> **Advanced Sharing**.
3. Check **Share this folder**, set the share name (e.g. `EchoFiles`), and click **Permissions**.
4. Grant **Read/Write** permissions to network users or clients running the WPF app.
5. Note the network path (UNC) e.g., `\\192.168.1.100\EchoFiles`.

### 3. Application Settings
Open [appsettings.json](MedicalApp/appsettings.json) and update the parameters:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_IP;Database=MedicalDb;User Id=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "FileStorageSettings": {
    "NetworkSharePath": "\\\\YOUR_SERVER_IP\\EchoFiles"
  }
}
```

---

## 💻 Running the Application

Ensure you have the **.NET 8 SDK** installed. Run the following commands in your shell:

```powershell
# Clone the repository
git clone https://github.com/KyuJunior/Clinic_cf.git
cd Clinic_cf

# Build and run the WPF application
cd MedicalApp
dotnet restore
dotnet run
```

*Note: Database migrations will run automatically on startup to deploy the latest schema.*
