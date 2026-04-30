USE app;
GO

-- ============================
-- CLEAN UP OLD TABLES
-- ============================
DROP TABLE IF EXISTS 
    MealReceivesFeedback,
    CustomerControlsFeedback,
    AdminManagesMeal,
    AdminManagesMenu,
    AdminManagesRestaurant,
    RestaurantPreparesOrder,
    CustomerViewsMenu,
    AdminManagesCustomer,
    OrderMeal,
    [Order],
    Feedback,
    Meal,
    Menu,
    RestaurantLocation,
    RestaurantPhone,
    Restaurant,
    AdminPhone,
    CustomerPhone,
    Admin,
    Customer;
GO

-- ============================
-- CUSTOMER & ADMIN
-- ============================
CREATE TABLE Customer (
    CID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Password VARCHAR(100) NOT NULL,
    Government VARCHAR(50) NOT NULL,
    City VARCHAR(50) NOT NULL,
    District VARCHAR(50) NOT NULL,
    Street VARCHAR(50) NOT NULL
);

CREATE TABLE CustomerPhone (
    CID INT NOT NULL,
    PhoneNo VARCHAR(11) NOT NULL,
    PRIMARY KEY (CID, PhoneNo),
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE CASCADE
);

CREATE TABLE Admin (
    AID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Password VARCHAR(100) NOT NULL
);

CREATE TABLE AdminPhone (
    AID INT NOT NULL,
    PhoneNo VARCHAR(11) NOT NULL,
    PRIMARY KEY (AID, PhoneNo),
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE CASCADE
);

-- ============================
-- RESTAURANT
-- ============================
CREATE TABLE Restaurant (
    RID INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    AID INT,
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE SET NULL
);

CREATE TABLE RestaurantPhone (
    RID INT NOT NULL,
    PhoneNo VARCHAR(11) NOT NULL,
    PRIMARY KEY (RID, PhoneNo),
    FOREIGN KEY (RID) REFERENCES Restaurant(RID) ON DELETE CASCADE
);

CREATE TABLE RestaurantLocation (
    RID INT PRIMARY KEY,
    Location VARCHAR(100) NOT NULL,
    FOREIGN KEY (RID) REFERENCES Restaurant(RID) ON DELETE CASCADE
);

-- ============================
-- MENU & MEAL
-- ============================
CREATE TABLE Menu (
    MenuID INT IDENTITY(1,1) NOT NULL,
    RID INT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL,
    AID INT,
    PRIMARY KEY (MenuID, RID),
    FOREIGN KEY (RID) REFERENCES Restaurant(RID) ON DELETE CASCADE,
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE SET NULL
);

CREATE TABLE Meal (
    MealID INT IDENTITY(1,1) PRIMARY KEY,
    MenuID INT NOT NULL,
    RID INT NOT NULL,
    AID INT,
    Name VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    Price DECIMAL(10,2) NOT NULL,
    IsAvailable BIT NOT NULL,
    Category VARCHAR(50),
    FOREIGN KEY (MenuID, RID) REFERENCES Menu(MenuID, RID) ON DELETE CASCADE,
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE SET NULL
);

-- ============================
-- FEEDBACK
-- ============================
CREATE TABLE Feedback (
    MealID INT NOT NULL,
    FeedbackID INT IDENTITY(1,1) NOT NULL,
    CID INT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment VARCHAR(MAX),
    PRIMARY KEY (MealID, FeedbackID),
    FOREIGN KEY (MealID) REFERENCES Meal(MealID) ON DELETE CASCADE,
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE SET NULL
);

-- ============================
-- ORDER & ORDER MEAL
-- ============================
CREATE TABLE [Order] (
    OID INT IDENTITY(1,1) PRIMARY KEY,
    CID INT NOT NULL,
    RID INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    TotalPrice DECIMAL(10, 2) NOT NULL,
    TotalItems INT NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE CASCADE,
    FOREIGN KEY (RID) REFERENCES Restaurant(RID) ON DELETE CASCADE
);

-- Avoiding multiple cascade paths by removing cascade from MealID
CREATE TABLE OrderMeal (
    OID INT NOT NULL,
    MealID INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (OID, MealID),
    FOREIGN KEY (OID) REFERENCES [Order](OID) ON DELETE CASCADE,
    FOREIGN KEY (MealID) REFERENCES Meal(MealID)
);

-- ============================
-- RELATIONSHIP TABLES
-- ============================
CREATE TABLE AdminManagesCustomer (
    AID INT NOT NULL,
    CID INT NOT NULL,
    ManagedAt DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (AID, CID),
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE CASCADE,
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE CASCADE
);

CREATE TABLE CustomerViewsMenu (
    CID INT NOT NULL,
    MenuID INT NOT NULL,
    RID INT NOT NULL,
    ViewedAt DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (CID, MenuID, RID),
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE CASCADE,
    FOREIGN KEY (MenuID, RID) REFERENCES Menu(MenuID, RID) ON DELETE CASCADE
);

-- Cascade only from OID to avoid multiple paths
CREATE TABLE RestaurantPreparesOrder (
    RID INT NOT NULL,
    OID INT NOT NULL,
    Status VARCHAR(50),
    PRIMARY KEY (RID, OID),
    FOREIGN KEY (RID) REFERENCES Restaurant(RID),
    FOREIGN KEY (OID) REFERENCES [Order](OID) ON DELETE CASCADE
);

CREATE TABLE AdminManagesRestaurant (
    AID INT NOT NULL,
    RID INT NOT NULL,
    PRIMARY KEY (AID, RID),
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE CASCADE,
    FOREIGN KEY (RID) REFERENCES Restaurant(RID) ON DELETE CASCADE
);

CREATE TABLE AdminManagesMenu (
    AID INT NOT NULL,
    MenuID INT NOT NULL,
    RID INT NOT NULL,
    PRIMARY KEY (AID, MenuID, RID),
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE CASCADE,
    FOREIGN KEY (MenuID, RID) REFERENCES Menu(MenuID, RID) ON DELETE CASCADE
);

CREATE TABLE AdminManagesMeal (
    AID INT NOT NULL,
    MealID INT NOT NULL,
    PRIMARY KEY (AID, MealID),
    FOREIGN KEY (AID) REFERENCES Admin(AID) ON DELETE CASCADE,
    FOREIGN KEY (MealID) REFERENCES Meal(MealID) ON DELETE CASCADE
);

-- ✅ FIXED: Order of columns matches Feedback PK (MealID, FeedbackID)
CREATE TABLE CustomerControlsFeedback (
    CID INT NOT NULL,
    MealID INT NOT NULL,
    FeedbackID INT NOT NULL,
    PRIMARY KEY (CID, MealID, FeedbackID),
    FOREIGN KEY (CID) REFERENCES Customer(CID) ON DELETE CASCADE,
    FOREIGN KEY (MealID, FeedbackID) REFERENCES Feedback(MealID, FeedbackID) ON DELETE CASCADE
);

CREATE TABLE MealReceivesFeedback (
    MealID INT NOT NULL,
    FeedbackID INT NOT NULL,
    PRIMARY KEY (MealID, FeedbackID),
    FOREIGN KEY (MealID, FeedbackID) REFERENCES Feedback(MealID, FeedbackID) ON DELETE CASCADE
);



--questions:

--A)what was the most ordering meal?
SELECT TOP 1 M.MealID, M.Name, COUNT(*) AS OrderCount
FROM OrderMeal OM
JOIN Meal M ON OM.MealID = M.MealID
GROUP BY M.MealID, M.Name
ORDER BY OrderCount DESC;

--B) What were the order prices for each customer during the last three months?
SELECT C.CID, C.FirstName, C.LastName, O.TotalPrice, O.CreatedAt
FROM [Order] O
JOIN Customer C ON O.CID = C.CID
WHERE O.CreatedAt >= DATEADD(MONTH, -3, GETDATE());

--C)What was the list of meals that were not ordered by any customer?
SELECT MealID, Name
FROM Meal
WHERE MealID NOT IN (
    SELECT DISTINCT MealID
    FROM OrderMeal
);

--D) Who was the customer that made the highest order price this month?
SELECT TOP 1 C.CID, C.FirstName, C.LastName, O.TotalPrice
FROM [Order] O
JOIN Customer C ON O.CID = C.CID
WHERE MONTH(O.CreatedAt) = MONTH(GETDATE()) AND YEAR(O.CreatedAt) = YEAR(GETDATE())
ORDER BY O.TotalPrice DESC;

--E) What was the list of meals that were ordered more than five times during the last two months?
--SELECT M.MealID, M.Name, SUM(OM.Quantity) AS TotalOrdered
--FROM OrderMeal OM
--JOIN Meal M ON OM.MealID = M.MealID
--JOIN [Order] O ON OM.OID = O.OID
--WHERE O.CreatedAt >= DATEADD(MONTH, -2, GETDATE())
--GROUP BY M.MealID, M.Name
--HAVING SUM(OM.Quantity) > 5;

--F) For each customer, retrieve all his/her information and the number of orders
SELECT C.*, COUNT(O.OID) AS OrderCount
FROM Customer C
LEFT JOIN [Order] O ON C.CID = O.CID
GROUP BY C.CID, C.FirstName, C.LastName, C.Email, C.Password, C.Government, C.City, C.District, C.Street;




INSERT INTO Customer (FirstName, LastName, Email, Password, Government, City, District, Street) VALUES
('Ahmed', 'Mohamed', 'ahmed.mohamed@email.com', 'pass123', 'Cairo', 'Nasr City', 'First District', 'Makram Ebeid'),
('Sara', 'Ali', 'sara.ali@email.com', 'pass456', 'Giza', 'Dokki', 'Tahrir', 'Nile St'),
('Omar', 'Khaled', 'omar.khaled@email.com', 'pass789', 'Alexandria', 'Smouha', 'Sporting', 'Victor St'),
('Laila', 'Hassan', 'laila.hassan@email.com', 'pass101', 'Cairo', 'Maadi', 'Degla', 'St 200'),
('Mona', 'Youssef', 'mona.youssef@email.com', 'pass202', 'Giza', 'Sheikh Zayed', 'Beverly Hills', 'Palm St'),
('Khaled', 'Ibrahim', 'khaled.ibrahim@email.com', 'pass303', 'Cairo', 'Heliopolis', 'Roxy', 'Ibn Sina St'),
('Nour', 'Adel', 'nour.adel@email.com', 'pass404', 'Alexandria', 'Miami', 'Seafront', 'Beach Rd'),
('Yassin', 'Tarek', 'yassin.tarek@email.com', 'pass505', 'Cairo', 'Zamalek', 'Nile View', 'Gezira St'),
('Fatima', 'Sayed', 'fatima.sayed@email.com', 'pass606', 'Giza', 'Haram', 'Faisal', 'King Faisal St'),
('Amr', 'Nasser', 'amr.nasser@email.com', 'pass707', 'Cairo', 'New Cairo', 'Fifth Settlement', 'South 90 St');
INSERT INTO CustomerPhone (CID, PhoneNo) VALUES
(1, '01012345678'),
(2, '01123456789'),
(3, '01234567890'),
(4, '01556789012'),
(5, '01098765432'),
(6, '01187654321'),
(7, '01276543210'),
(8, '01543210987'),
(9, '01032109876'),
(10, '01110987654');
INSERT INTO Admin (FirstName, LastName, Email, Password) VALUES
('Hassan', 'Eid', 'hassan.eid@admin.com', 'admin123'),
('Rania', 'Mostafa', 'rania.mostafa@admin.com', 'admin456'),
('Tamer', 'Salem', 'tamer.salem@admin.com', 'admin789'),
('Dina', 'Fathy', 'dina.fathy@admin.com', 'admin101'),
('Mahmoud', 'Gamal', 'mahmoud.gamal@admin.com', 'admin202'),
('Aya', 'Zaki', 'aya.zaki@admin.com', 'admin303'),
('Sami', 'Hany', 'sami.hany@admin.com', 'admin404'),
('Lina', 'Kamal', 'lina.kamal@admin.com', 'admin505'),
('Karim', 'Nabil', 'karim.nabil@admin.com', 'admin606'),
('Hoda', 'Essam', 'hoda.essam@admin.com', 'admin707');
INSERT INTO AdminPhone (AID, PhoneNo) VALUES
(1, '01011122233'),
(2, '01122233344'),
(3, '01233344455'),
(4, '01544455566'),
(5, '01055566677'),
(6, '01166677788'),
(7, '01277788899'),
(8, '01588899900'),
(9, '01099900011'),
(10, '01100011122');
INSERT INTO Restaurant (Name, AID) VALUES
('Pizza Palace', 1),
('Burger Bonanza', 2),
('Sushi Spot', 3),
('Taco Town', 4),
('Pasta Place', 5),
('Grill House', 6),
('Falafel Factory', 7),
('Seafood Shack', 8),
('Curry Corner', 9),
('Shawarma Stop', 10);
INSERT INTO RestaurantPhone (RID, PhoneNo) VALUES
(1, '01012344321'),
(2, '01123455432'),
(3, '01234566543'),
(4, '01545677654'),
(5, '01056788765'),
(6, '01167899876'),
(7, '01278900987'),
(8, '01589011098'),
(9, '01090122109'),
(10, '01101233210');
INSERT INTO RestaurantLocation (RID, Location) VALUES
(1, '123 Makram Ebeid, Nasr City, Cairo'),
(2, '45 Nile St, Dokki, Giza'),
(3, '78 Victor St, Smouha, Alexandria'),
(4, '12 St 200, Maadi, Cairo'),
(5, '9 Palm St, Sheikh Zayed, Giza'),
(6, '33 Ibn Sina St, Heliopolis, Cairo'),
(7, '22 Beach Rd, Miami, Alexandria'),
(8, '15 Gezira St, Zamalek, Cairo'),
(9, '88 King Faisal St, Haram, Giza'),
(10, '50 South 90 St, New Cairo, Cairo');
INSERT INTO Menu (RID, Type, IsActive, AID) VALUES
(1, 'Italian', 1, 1),
(2, 'American', 1, 2),
(3, 'Japanese', 1, 3),
(4, 'Mexican', 1, 4),
(5, 'Italian', 1, 5),
(6, 'BBQ', 1, 6),
(7, 'Egyptian', 1, 7),
(8, 'Seafood', 1, 8),
(9, 'Indian',1,7);





INSERT INTO [Order] (CID, RID, CreatedAt, TotalPrice, TotalItems, PaymentMethod) VALUES
(1, 1, '2025-05-01 12:30:00', 150.00, 3, 'Credit Card'),
(2, 2, '2025-05-02 14:15:00', 80.50, 2, 'Cash'),
(3, 3, '2025-05-03 19:00:00', 220.75, 4, 'Mobile Payment'),
(4, 4, '2025-05-04 11:45:00', 95.25, 2, 'Credit Card'),
(5, 5, '2025-05-05 20:30:00', 180.00, 3, 'Cash'),
(6, 6, '2025-05-06 13:20:00', 250.00, 5, 'Mobile Payment'),
(7, 7, '2025-05-07 17:10:00', 60.00, 1, 'Credit Card'),
(8, 8, '2025-05-08 21:00:00', 300.50, 6, 'Cash'),
(9, 9, '2025-05-09 15:40:00', 140.25, 3, 'Mobile Payment'),
(10, 10, '2025-05-10 18:25:00', 110.00, 2, 'Credit Card');




INSERT INTO [Order] (CID, RID, CreatedAt, TotalPrice, TotalItems, PaymentMethod) VALUES
(1, 2, '2025-01-15 13:45:00', 120.50, 2, 'Cash'),
(2, 3, '2025-02-03 18:20:00', 190.75, 4, 'Credit Card'),
(3, 4, '2025-02-20 11:10:00', 85.00, 2, 'Mobile Payment'),
(4, 5, '2025-03-07 20:00:00', 210.25, 3, 'Credit Card'),
(5, 6, '2025-03-25 14:30:00', 160.00, 3, 'Cash'),
(6, 7, '2025-04-12 19:15:00', 70.50, 1, 'Mobile Payment'),
(7, 8, '2025-04-28 12:50:00', 280.00, 5, 'Credit Card'),
(8, 9, '2025-05-10 17:40:00', 130.25, 3, 'Cash'),
(9, 10, '2025-05-28 21:05:00', 95.75, 2, 'Mobile Payment'),
(10, 1, '2025-06-05 15:25:00', 175.00, 4, 'Credit Card');



INSERT INTO Meal (MenuID, RID, AID, Name, Description, Price, IsAvailable, Category) VALUES
(1, 1, 1, 'Margherita Pizza', 'Classic pizza with tomato sauce, mozzarella, and basil', 120.00, 1, 'Pizza'),
(1, 1, 1, 'Pepperoni Pizza', 'Pizza with pepperoni, cheese, and tomato sauce', 140.00, 1, 'Pizza'),
(2, 2, 2, 'Classic Burger', 'Beef patty with lettuce, tomato, and mayo', 80.50, 1, 'Burger'),
(2, 2, 2, 'Cheeseburger', 'Beef patty with cheddar cheese and pickles', 90.75, 0, 'Burger'),
(3, 3, 3, 'California Roll', 'Sushi roll with crab, avocado, and cucumber', 150.25, 1, 'Sushi'),
(3, 3, NULL, 'Spicy Tuna Roll', 'Sushi roll with spicy tuna and seaweed', 160.00, 1, 'Sushi'),
(4, 4, 4, 'Beef Taco', 'Soft taco with seasoned beef and salsa', 65.00, 1, 'Taco'),
(5, 5, 5, 'Spaghetti Carbonara', 'Pasta with creamy sauce, bacon, and parmesan', 130.50, 1, 'Pasta'),
(6, 6, NULL, 'Grilled Chicken', 'Marinated chicken breast with BBQ sauce', 110.00, 1, 'Grill'),
(7, 7, 7, 'Falafel Sandwich', 'Falafel with tahini and veggies in pita bread', 35.25, 1, 'Sandwich');



INSERT INTO Feedback (MealID, CID, Rating, Comment) VALUES
(1, 1, 5, 'Best pizza ever, fresh ingredients!'),
(2, 2, 4, 'Pepperoni was great, but a bit spicy.'),
(3, 3, 3, 'Burger was okay, bun was too soggy.'),
(4, 4, 2, NULL),
(5, NULL, 5, 'Amazing sushi, very fresh!'),
(6, 5, 4, 'Tuna roll was delicious, good portion.'),
(7, 6, 3, 'Taco was decent, needs more seasoning.'),
(8, 7, 5, 'Carbonara was creamy and perfect!'),
(9, 8, 4, 'Chicken was juicy, loved the BBQ flavor.'),
(10, NULL, 3, 'Falafel was good but a bit dry.');