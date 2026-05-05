-- Created by Redgate Data Modeler (https://datamodeler.redgate-platform.com)
-- Last modification date: 2026-04-30 03:57:24.092

-- tables
-- Table: Makers
CREATE TABLE Makers (
    Id int  NOT NULL IDENTITY,
    Name varchar(150)  NOT NULL,
    CONSTRAINT Makers_pk PRIMARY KEY  (Id)
);

-- Table: ProductTypes
CREATE TABLE ProductTypes (
    Id int  NOT NULL IDENTITY,
    Name varchar(50)  NOT NULL,
    CONSTRAINT ProductTypes_pk PRIMARY KEY  (Id)
);

-- Table: Products
CREATE TABLE Products (
    Id int  NOT NULL IDENTITY,
    Name varchar(150)  NOT NULL,
    Description varchar(500)  NULL,
    StickerPrice decimal(7,2)  NOT NULL,
    ProductTypeId int  NOT NULL,
    MakerId int  NOT NULL,
    CONSTRAINT Products_pk PRIMARY KEY  (Id)
);

-- Table: VendorProducts
CREATE TABLE VendorProducts (
    ProductId int  NOT NULL,
    VendorCode char(10)  NOT NULL,
    Amount int  NOT NULL,
    PricePerUnit decimal(7,2)  NOT NULL,
    CONSTRAINT VendorProducts_pk PRIMARY KEY  (ProductId,VendorCode)
);

-- Table: Vendors
CREATE TABLE Vendors (
    Code char(10)  NOT NULL,
    Name varchar(100)  NOT NULL,
    CONSTRAINT Vendors_pk PRIMARY KEY  (Code)
);

-- foreign keys
-- Reference: Product_Maker (table: Products)
ALTER TABLE Products ADD CONSTRAINT Product_Maker
    FOREIGN KEY (MakerId)
    REFERENCES Makers (Id);

-- Reference: Product_ProductType (table: Products)
ALTER TABLE Products ADD CONSTRAINT Product_ProductType
    FOREIGN KEY (ProductTypeId)
    REFERENCES ProductTypes (Id);

-- Reference: Table_4_Product (table: VendorProducts)
ALTER TABLE VendorProducts ADD CONSTRAINT Table_4_Product
    FOREIGN KEY (ProductId)
    REFERENCES Products (Id);

-- Reference: VendorProducts_Vendors (table: VendorProducts)
ALTER TABLE VendorProducts ADD CONSTRAINT VendorProducts_Vendors
    FOREIGN KEY (VendorCode)
    REFERENCES Vendors (Code);

-- End of file.


-- Makers
INSERT INTO Makers (Name) VALUES
('Contoso'),
('Fabrikam'),
('Northwind'),
('AdventureWorks');

-- ProductTypes
INSERT INTO ProductTypes (Name) VALUES
('Electronics'),
('Furniture'),
('Appliances'),
('Accessories');

-- Vendors
INSERT INTO Vendors (Code, Name) VALUES
('VEND000001', 'Tech Supplier Ltd'),
('VEND000002', 'Home Goods Inc'),
('VEND000003', 'Global Trade Co'),
('VEND000004', 'Retail Partner');

-- Products
INSERT INTO Products (Name, Description, StickerPrice, ProductTypeId, MakerId) VALUES
('Laptop X100', 'High performance laptop', 4500.00, 1, 1),
('Office Chair Comfort', 'Ergonomic office chair', 850.00, 2, 2),
('Microwave Pro', '800W microwave oven', 600.00, 3, 3),
('Wireless Mouse', 'Bluetooth optical mouse', 120.00, 4, 1),
('Standing Desk', 'Adjustable height desk', 1500.00, 2, 4),
('Smartphone Z', 'Latest generation smartphone', 3200.00, 1, 2);

-- VendorProducts
INSERT INTO VendorProducts (ProductId, VendorCode, Amount, PricePerUnit) VALUES
(1, 'VEND000001', 50, 4200.00),
(1, 'VEND000003', 30, 4300.00),
(2, 'VEND000002', 100, 700.00),
(3, 'VEND000003', 80, 500.00),
(4, 'VEND000001', 200, 90.00),
(4, 'VEND000004', 150, 95.00),
(5, 'VEND000002', 40, 1300.00),
(6, 'VEND000001', 60, 3000.00),
(6, 'VEND000003', 70, 3100.00);
