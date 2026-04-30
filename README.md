# 🍔 Food Ordering System

**Cairo University — Faculty of Computers and Artificial Intelligence**  
**Course:** Database Systems-1 | **Phase:** 2  
**Section:** S5 | **TA:** Nour Ayman | **Program:** Special Zayed

---

## 📌 Project Purpose

This Food Ordering System streamlines the food ordering process for customers while enhancing operational efficiency for restaurants. It allows customers to browse menus, customize orders, and make payments seamlessly. For restaurants, it automates order management, minimizes errors, and optimizes kitchen workflows.

Key features include:
- Identifying customers' favorite dishes and seasonal preferences
- Allowing users to add, edit, or remove feedback
- Managing daily customer data and sales records
- Simplifying the billing process

---

## 🗂️ Project Structure

```
FoodOrderingSystem/
├── database/
│   └── DDL_and_SQL_statements.sql   # Full DDL + sample data + queries
├── diagrams/
│   ├── Power_Designer_ERD.pdm       # PowerDesigner conceptual model
│   └── Food_Ordering_Systems_ERD.pdm# Physical ERD model
├── docs/
│   ├── Report.pdf                   # Phase 2 C# implementation report
│   └── Phase2_ERD_Report.pdf        # Phase 2 ERD documentation
└── src/
    └── (C# Windows Forms source files)
```

---

## 🗄️ Database Schema

The system is built on **Microsoft SQL Server** and includes the following tables:

### Core Entities
| Table | Description |
|-------|-------------|
| `Customer` | Stores customer personal and address info |
| `Admin` | Stores admin credentials |
| `Restaurant` | Restaurant details, linked to managing admin |
| `Menu` | Menus per restaurant (weak entity) |
| `Meal` | Individual meals within a menu |
| `Feedback` | Customer ratings and comments on meals |
| `Order` | Customer orders with payment info |

### Multi-valued Attribute Tables
| Table | Description |
|-------|-------------|
| `CustomerPhone` | Customer phone numbers |
| `AdminPhone` | Admin phone numbers |
| `RestaurantPhone` | Restaurant phone numbers |
| `RestaurantLocation` | Restaurant locations |

### Relationship Tables
| Table | Description |
|-------|-------------|
| `OrderMeal` | Meals within each order (quantity + price) |
| `AdminManagesCustomer` | Admin–Customer management |
| `AdminManagesRestaurant` | Admin–Restaurant management |
| `AdminManagesMenu` | Admin–Menu management |
| `AdminManagesMeal` | Admin–Meal management |
| `CustomerViewsMenu` | Customer menu browsing history |
| `RestaurantPreparesOrder` | Order preparation tracking |
| `CustomerControlsFeedback` | Customer feedback ownership |
| `MealReceivesFeedback` | Meal–Feedback association |

---

## 📊 SQL Queries

The file `database/DDL_and_SQL_statements.sql` includes answers to the following analytical questions:

| # | Question |
|---|----------|
| A | What was the most ordered meal? |
| B | What were the order prices for each customer during the last 3 months? |
| C | What meals were never ordered by any customer? |
| D | Who was the customer with the highest order price this month? |
| E | What meals were ordered more than 5 times in the last 2 months? |
| F | For each customer, retrieve all info and their total number of orders |

---

## 💻 C# Windows Forms Application

The application (Phase 2) was built using **C# Windows Forms** connected to SQL Server via ADO.NET.

### Features Implemented

#### Feedback Management (Form2)
- **Insert** — Add feedback linked to customer email + meal ID
- **Update** — Modify existing feedback (rating/comment)
- **Delete** — Remove feedback by FeedbackID
- **Show Data** — Display all feedbacks for a given email

#### Customer Account Management (Form3)
- **Insert** — Register new customer with 1–2 phone numbers
- **Update** — Update personal info or password (with authentication)
- **Delete** — Delete account using email + password verification
- **Show Data** — Display all customer records

---

## ⚙️ Setup Instructions

### Prerequisites
- Microsoft SQL Server (or SQL Server Express)
- SQL Server Management Studio (SSMS)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/FoodOrderingSystem.git
   cd FoodOrderingSystem
   ```

2. **Set up the database**
   - Open SSMS and connect to your SQL Server instance
   - Open `database/DDL_and_SQL_statements.sql`
   - Execute the script — it will create the `app` database, all tables, and insert sample data

3. **Run the C# application** *(if source is included)*
   - Open the solution in Visual Studio
   - Update the connection string in the forms if needed:
     ```
     Data Source=.\SQLEXPRESS;Initial Catalog=app;Integrated Security=True
     ```
   - Build and run

---

## 🔗 ERD Diagrams

- **Conceptual ERD** — Created in Lucidchart (see docs)
- **Physical ERD** — Created in PowerDesigner (`.pdm` files in `/diagrams`)
- **SQL Diagram** — Available in the Phase 2 report PDF

---

## 📄 License

This project was submitted as an academic assignment for Cairo University, Faculty of Computers and Artificial Intelligence, Database Systems-1 course.
