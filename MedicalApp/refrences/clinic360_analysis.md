# Clinic360: Features & Frontend UI/UX Analysis Report

Clinic360 (hosted at [web.health-360.co](https://web.health-360.co/)) is a sophisticated, highly customizable medical and dental clinic management software (SaaS) designed to streamline private clinic operations. Tailored primarily for clinics in Iraq (with pricing in Iraqi Dinar - IQD), it offers extensive EMR capabilities, advanced customization settings, real-time collaboration chat, robust offline synchronization, and native desktop helper integrations.

---

## 1. High-Level Technical Architecture

* **Frontend Framework**: React single-page application (SPA).
* **Build System**: Vite (indicated by single-bundle JS/CSS).
* **UI Component Library**: MUI (specifically utilizing **MUI Joy UI** classes such as `MuiListItemButton-root`, `MuiButton-variantPlain`, and Joy's modern layout tokens).
* **Design Pattern**: Responsive, clean layout featuring high density data grids, custom HSL-based color themes, customizable page densities, and collapsible split panels.
* **Offline Capabilities**: Full Progressive Web App (PWA) with client-side IndexedDB caches, storage quota management, and background synchronization queues.

---

## 2. Core Functional Modules

### 2.1 Dashboard (Home)
The Dashboard provides clinicians and receptionists with a centralized clinic health overview. It features modular cards that can be shown or hidden based on user preference.

* **Metric Cards**:
  * **Patients**: Tracks total registered patients, new patients today, and tomorrow's upcoming bookings.
  * **Visits**: Highlights current room queue metrics (pending and unfinished visits).
  * **Operations**: Shows pending, completed, and revenue figures for surgical cases.
  * **Pharmacy**: Lists prescriptions and drug usage statistics.
* **Analytics**:
  * **Patient Gender Split**: A visual breakdown (showing 77% Male / 21% Female in current test workspace data).
  * **Visits Per Month**: Bar or line graph representing client flow.
  * **Top Diagnoses & Top Complaints**: Lists top items (e.g., `DM TYPE 2` for diagnoses and `fever`, `abd pain` for complaints) with occurrence count.
* **Finance**:
  * Displays today's income, outstanding invoices count, and unpaid balance list with time filters (Today, This Week, This Month, etc.).

---

### 2.2 Patients Directory
A dense tabular register of all patients registered in the clinic's database.

* **Headers & Fields**: Patient ID (`#`), Name (Arabic/English), Age, Finance status, Gender, Phone, Province, Date of Return, Total Visits, Last Visit, History, Surgical History, Tags, Blood Group, and Notes.
* **Actions**: Search input, filters, direct navigation to patient records, and "Add Patient" modals.

---

### 2.3 Visits (Kanban Queue Board)
An interactive board representing physical rooms or clinic queue stages. Clinicians drag and drop cards to route patients through their check-in process.

* **Column Queues**: Default columns include `Pending` and `Not Finished`, representing patients currently in the waiting area vs. those currently undergoing consultation/treatment.
* **Queue Cards**: Draggable patient cards detailing patient name, queue number (`#`), age, check-in time, performing doctor, and payment status label (e.g., `Paid`, `Free Return`).
* **Interactions**: Drag-to-reorder columns and drag-to-sort cards. Uses fully accessible keyboard fallback controls ("Press space to pick up, arrow keys to move, space again to drop").
* **Visits Column Manager (Add Table)**: Clinicians can partition the visits board by creating custom filtered columns based on visit source (Pending, Not Finished) and labels. Useful for multi-department clinics to separate queues.

---

### 2.4 Appointments
A large calendar grid showing doctor schedules.

* **Layout**: Standard month-to-view layout showing SUN through SAT.
* **Features**: Time navigation, search input, quick appointment creation modal, and layout switching (Month, Week views).
* **Appointment Creator Modal**: Triggers form to input title, date, start/end times, patient linking, room assignment, and an "Add Visit" checkbox to automatically queue them in the Visits board upon scheduling.

---

### 2.5 Operations Tracking
Specifically designed for surgical clinics, this screen tracks operations assigned to patients.

* **Fields**: Patient Name, Age, Surgery Type, Scheduled Date, and Operation Status (e.g., Pending, Completed).
* **Actions**: Quick-filters, search input, and "+ Add Operation" form.

---

### 2.6 Drugs (Pharmacy Directory)
Manages the drug database utilized inside the EMR prescription module.

* **Fields**: Drug Name, Scientific Name, and Usage Count.
* **Filters**: Timeframe ranges (This Week, This Month, This Year) to analyze prescription patterns.
* **Add Drug Modal**: Input fields for Trade Name and autocomplete link to a Scientific Drug name. Ranks drugs by prescription frequency (e.g. `Panadol 500mg tab` shows usage count 14). Allows typing in Arabic for local instructions.

---

### 2.7 Finance Overview, Invoices, Reports & Inventory
A multi-tab module that manages clinic revenue, inventory stock, and doctor payout shares.

* **Overview**: Total Income, outstanding balance (e.g., `1,500,519 IQD` across 25 invoices), daily paid invoices count, and invoice table.
* **Invoices Tab**: Large list containing Patient Name, Phone, Total Invoiced, Outstanding, Invoices Count, and Last Invoice date.
* **Reports Tab**: Extremely detailed financial dashboard summarizing:
  * *Net Profit & Margin*: Profit/Cost/Sales breakdowns.
  * *Payment Status*: Total Paid vs. Outstanding balance.
  * *Income Breakdown*: Visit fees, procedures revenue (categorized into Visit, Special, General, and Orthodontic treatments), and procedures containing inventory items.
  * *Doctor Performance*: Tracks individual revenue contributions.
  * *Payment Methods*: Summarizes cash, card, or outstanding notes.
* **Expenses Tab & Add Expense Modal**: Logs clinic outlays (Subject, Cost in IQD, State: Paid/Unpaid, and Date).
* **Inventory Tab & Add Item Modal**: Tracks clinic-owned medical/dental consumables (Item Name, Cost, Sales Price, Profit Margin, In Stock qty, Sold qty, Expire Date, and Status). Expired items are automatically flagged in red as `Expired` relative to system local time (e.g. dental filling composite `كمبوزت` and amalgam `املكم`). Calculates overall inventory margin, profit, cost, and sales.

---

### 2.8 Collaboration Chat Drawer
A real-time workspace messaging client that overlays as a modal dialog, enhancing internal communication between receptionists, nurses, and doctors.

* **Workspace Group Chats**: General workspace group channel for team-wide broadcasts.
* **Direct Messages**: Allows private DMs between workspace members.
* **Features**: Full text search in chat, attachments upload, and voice message recording.

---

## 3. The Electronic Medical Record (EMR) Clinician Screen

The EMR screen is the core interface where doctors document visits. It uses a **split layout** consisting of a collapsible left navigation bar, a central scrollable EMR card form, and a resizable right patient profile sidebar.

### 3.1 EMR Navigation Actions
* **Add Operation Modal**: Triggers form to schedule/record surgery (Surgery name, Diagnosis, Operation Status: Pending/Completed, Date of Operation, Cost in IQD, Anesthetist, Anesthesia type: Local/General, Assistant, Place of surgery, Finished Date, Summary notes, and Scanned Documents).
* **Previous Visits Modal**: Displays a detailed history matrix of past patient visits detailing Date, Chief Complaint, Complaint Note (e.g., `Patient presented with fever`), HPI, Investigations ordered, Positive Signs (e.g. `PALLOR`), and Performed Procedures (e.g. `suture removal` or `30 min session`).
* **More Print Options Dropdown**: Supports printing specific sub-sections or print bundles: `prescription`, `report`, `visit` (entire summary log), `procedures`, `investigations`, and `imaging`.

### 3.2 Central EMR Cards & Fields
1. **Complaints**: Autocomplete combobox to add chief complaints. Multiline note areas for specific complaints and **History of Present Illness (HPI)**. Support for speech-to-text voice input.
2. **Vital Signs**: Inputs for Heart Rate (bpm), SBP/DBP (mmHg), Respiratory Rate (/min), SPO2 (%), and Temperature (°C), with a "Stable" toggle switch.
3. **Examination**: Positive clinical signs, relative negative signs, and freeform examination notes (supports voice typing).
4. **Past History**: Multiline past medical conditions, allergies, drug history, past surgeries, and family history. Includes a smoking pack-years calculator (cig/day × years) and alcohol tracking.
5. **Procedures**: List of procedures performed during the visit (e.g., `جلسة 30 دقيقة`). Clinicians can assign the performing doctor, adjust price (in IQD), add notes, select consumable inventory items used during the procedure, and define doctor revenue split shares. Contains invoice status and print buttons.
6. **Prescription**: Autocomplete fields for Diagnosis, recent prescriptions templates, drug lookup, return date calendar picker, and medical instructions (e.g., `تعليمات مرضى السكري`). Includes:
   * **Medscape Drug Interaction Integration**: The EMR prescription module reads Medscape IDs mapped to trade/scientific drugs to check for harmful drug-drug interactions.
7. **Investigation & Imaging**: Fast order forms for laboratory tests and medical imaging scans.
8. **Reports & Referrals**: Generates patient referral letters or medical reports using pre-configured templates.
9. **Note Board**: A digital whiteboard supporting handwriting/drawings across multiple canvas pages.

### 3.3 Right Patient Sidebar
* **Demographics**: Patient name initials icon, ID, Gender, Date of Birth, Age, Current Visit Date, and number of children.
* **Medical Info**: Displays weight (kg) and height (cm).
* **Files & Attachments**: Patient-uploaded documents, x-rays, or scan files.
* **Invoice History & Billing Ledger**: Logs past invoices with date, status (Paid, Partial, Unpaid), amount paid, and outstanding balance. Clicking "Pay Invoice" opens a formal invoice modal showing items, payment history ledger with "Refund", "Print", "Export", and "Edit Visit Fee" action triggers.

---

## 4. Customization & Settings

Clinic360 provides granular control over how the app is structured, styled, and accessed:

* **Appearance & Theme**:
  * *Quick Presets*: default, Vision-Friendly, Minimalist, High Density, Dense + Readable, Long Session, Spa Calm.
  * *Theme Modes*: Light, Dark, Auto (System sync).
  * *Color Themes*: 12 custom design accents including *Clinic Blue*, *Eucalyptus Calm*, *Bloom*, *Burnt Sienna*, *Ocean Deep*, *Steel Blue*, *Indigo Premium*, *Mercury*, *Paper White*, *Lavender Calm*, *Vermilion Sport*, and *Peach Comfort*.
  * *Font Size*: Small, Default, Large, Huge.
  * *Density*: Comfortable, Default, Compact, Dense.
  * *Reduce Clicks*: A toggle to show common row actions directly as inline icons in tables.
* **Language (Right-To-Left Support)**:
  * Supports Display Languages in **English** (LTR) and **Arabic** (العربية - RTL).
  * The user interface layout flips dynamically when switching languages, while clinical visit fields and print templates stay in English for medical standardization.
* **Workspaces & Permissions (Role-Based Access Control - RBAC)**:
  * Switch workspaces, manage team members, and configure detailed workspace security permissions.
  * **Edit Member Permissions Dialog**: Sets roles (Admin, Doctor, Secretary, Nurse, Accountant, Custom) with granular toggles:
    * *Patients (7)*: View, Add, Edit, Delete, Export, Set Return Visit, Filter.
    * *Visits (26)*: Manage Queue, Finish All, toggle visibility for clinical sections (e.g. block a secretary from seeing Complaints, Prescriptions, or Vitals), and restrict visit actions (Quick Edit, End Visit, Send to Lab/Pharmacy, Delete).
    * *Finance & Inventory (11)*: Restrict access to Take Payment, Refund, Edit Visit Fee, and toggle the "Full Date Range" switch (if off, staff can only view today's financial transactions to protect clinic cash flow history).
* **Rooms Layout**:
  * Setup physical rooms (e.g., Room 1, Room 2, Room 3) to route queue cards. Each room has a menu to Rename, Manage members, or Delete.
* **Print Templates & WYSIWYG Print Designer**:
  * Dedicated layout configurators for Visits, Prescriptions, Reports, Procedures, Ocular Rx (eye prescriptions), Operations, Lab/imaging orders, Invoices, and QR cards.
  * **Edit Print Template Modal**: Offers tabbed options (General, Header, Content) with a live preview panel (Name: Ahmed Hassan, sample drugs list, zoom in/out, landscape, background upload):
    * *Header tab*: Spacing, Font Size, Bold attributes, column layout (One vs Two columns), and a drag-and-drop sortable interface to drag metadata fields (name, age, date, weight, height, identifier) between left and right columns.
    * *Content tab*: Selectively hides/shows and formats font sizes/weights for Diagnosis, Medications (numeric vs bullet markers), Notes, and Plan.
* **Visit Form Fields & Layouts (Visit Settings)**:
  * Toggles default patient registration fields (Job, Weight, Height, Marital status, Smoking, Allergies, Note) and Custom Patient Fields.
  * **Colored section accents**: Switch to toggle colored borders and card headers on the EMR form.
  * **Section Drag-to-Reorder**: Lets doctors sort card placement order (e.g., placing Prescriptions above Vital Signs).
  * **Section Settings Modal**: Lets clinicians set layouts (Half width or Full width grid space), assign custom color overrides, and toggle visibility of sub-fields (e.g., hiding complaints voice inputs or HPI textareas).
  * **Manage Sections Modal**: Allows showing/hiding main EMR modules, including the default hidden sections: **Charts**, **System Review**, **Documents**, and **PACS**.
  * **Create Custom Sections**: Allows creating new visit forms, adding fields, and triggering the **Add Custom Field** modal (Field type: `Text`, `Yes/No`, `Date`, `Number`, `Files`, `Text Area`, `Creatable Select`, or `Multi-Select Creatable`). Toggling the checkbox `Permanent across patient visits` preserves custom values for a patient across all future visits.
* **Templates & Directories**:
  * "Show built-in default lists" switch hides/shows standard suggestions.
  * **Chief Complaints**: Massive pre-loaded library categorized into general, neuro, speech, GI, respiratory, cardiac, and urology complaints (e.g. `loin pain and hematuria`, `Diarrhoea`, `Palpitations`).
  * **Medical Instructions**: Detailed rich text instructions editor with formatting toolbar (Bold, lists, headings, links, text alignments, table insertion).
  * **Prescriptions**: Bundles trade and scientific drugs (Dose, Type, Times, and Note) into selectable templates (e.g. `COMMON COLD` containing Panadol and Loratidine).
* **Modules (Extra Features)**:
  * Track surgical cases (*Operations*), DICOM studies and reports (*PACS*), send direct *WhatsApp* alerts, enable public *Online booking*, and share a live *Waiting list* queue page.
* **Doctor Contacts & External Contacts**:
  * *Doctor Contacts*: Manage doctor access tokens to share clinical files and DICOM scans.
  * *External Contacts*: Register referring doctors, sample collectors, and lab partners referenced by PACS, patient forms, and procedure revenue shares.
* **Support**:
  * *Remote Support*: A button generates a one-time remote access code sent to the Clinic360 support team to let them control the client machine.
  * Direct Call/Chat triggers linking to support phone numbers (+964 prefix for Iraq).

---

## 5. Offline Capabilities & Desktop Integrations

### 5.1 Offline Mode & Sync Engine
Clinic360 is built with a local-first paradigm. If the clinic loses internet connectivity:
* **Master Switch**: Clinicians can enable offline mode to prevent save failures when offline.
* **Storage Metrics**: Displays browser quota usage (e.g., 25.7 MB / 10.03 GB) with cache size details (Operation log, Visits cache, and Lookups cache).
* **Operation Log**: Tracks unsynced offline writes ("intents"). These intents represent local edits that are queued to sync back to the cloud database when internet returns.
* **Cache Management**: Tools to Refresh, Run eviction pass, and Clear offline data.

### 5.2 Desktop Helper Integration
A small native Windows tray application (**Clinic360 Helper**), installable via `Clinic360HelperSetup.exe` (does not require admin rights). Once active, it bridges the web application to physical clinic devices:
* **DICOM Studies**: Opens and drives imaging studies directly in **RadiAnt DICOM viewer**.
* **Scanner Control**: Triggers physical document/image scanners.
* **Silent Printing**: Bypasses the browser print preview popup to print invoices/prescriptions instantly.
* **Word Files**: Integrates and compiles MS Word report templates.

---

## 6. Frontend UI/UX Assessment

| UI/UX Dimension | Performance & Implementation Details |
| :--- | :--- |
| **Visual Styling** | Consistent and polished. Joy UI's flat design variables create smooth layouts, premium border highlights, and elegant color systems. The dark mode matches modern SaaS styles, avoiding pure black in favor of dark gray slate. |
| **Responsiveness** | Sidebars utilize slide drawer animations on mobile viewports. Layout widths are adjustable via draggable separators (e.g., resizable patient details sidebar from 336px to 576px). |
| **Efficiency (Fewer Clicks)** | Auto-saving alerts ("Saved"), autocomplete comboboxes for drug/complaint libraries, and the "Reduce Clicks" setting minimize user interaction costs during busy clinical hours. |
| **Accessibility & Keyboard Navigation** | High accessibility with interactive aria roles, focused elements (focused buttons and dialogs), and keyboard navigation fallbacks for drag-and-drop actions. |
| **Medical Usability** | Mixed Arabic-English support is handled well. Features like Speech-to-Text voice inputs and templates/recent list shortcuts cater to doctors typing records rapidly. |
