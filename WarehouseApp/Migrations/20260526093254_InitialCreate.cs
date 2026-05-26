using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Categories",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Categories", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DocumentTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DocumentTypes", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Statuses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Statuses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Suppliers",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Inn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
            //        Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Suppliers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Units",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
            //        ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Units", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            //        FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Points = table.Column<int>(type: "int", nullable: false),
            //        IsSubscribedToAI = table.Column<bool>(type: "bit", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
            //        BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Warehouses",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Warehouses", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Products",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            //        Article = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        MinStock = table.Column<int>(type: "int", nullable: false),
            //        Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        StatusId = table.Column<int>(type: "int", nullable: true),
            //        CategoryId = table.Column<int>(type: "int", nullable: true),
            //        UnitId = table.Column<int>(type: "int", nullable: true),
            //        SupplierId = table.Column<int>(type: "int", nullable: true),
            //        CertificateCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Products", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Products_Categories_CategoryId",
            //            column: x => x.CategoryId,
            //            principalTable: "Categories",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.SetNull);
            //        table.ForeignKey(
            //            name: "FK_Products_Statuses_StatusId",
            //            column: x => x.StatusId,
            //            principalTable: "Statuses",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.SetNull);
            //        table.ForeignKey(
            //            name: "FK_Products_Suppliers_SupplierId",
            //            column: x => x.SupplierId,
            //            principalTable: "Suppliers",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_Products_Units_UnitId",
            //            column: x => x.UnitId,
            //            principalTable: "Units",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Orders",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        UserId = table.Column<int>(type: "int", nullable: false),
            //        OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            //        CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        OrderDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Orders", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_Orders_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserActions",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        UserId = table.Column<int>(type: "int", nullable: true),
            //        ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserActions", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_UserActions_Users_UserId",
            //            column: x => x.UserId,
            //            principalTable: "Users",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "WarehouseDocuments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DocumentTypeId = table.Column<int>(type: "int", nullable: false),
            //        Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        Date = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        WarehouseId = table.Column<int>(type: "int", nullable: true),
            //        SupplierId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_WarehouseDocuments", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_WarehouseDocuments_DocumentTypes_DocumentTypeId",
            //            column: x => x.DocumentTypeId,
            //            principalTable: "DocumentTypes",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_WarehouseDocuments_Suppliers_SupplierId",
            //            column: x => x.SupplierId,
            //            principalTable: "Suppliers",
            //            principalColumn: "Id");
            //        table.ForeignKey(
            //            name: "FK_WarehouseDocuments_Warehouses_WarehouseId",
            //            column: x => x.WarehouseId,
            //            principalTable: "Warehouses",
            //            principalColumn: "Id");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ReplenishmentPlans",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ProductId = table.Column<int>(type: "int", nullable: false),
            //        PlannedQuantity = table.Column<int>(type: "int", nullable: false),
            //        RecommendedQuantity = table.Column<int>(type: "int", nullable: true),
            //        Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            //        Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        PlannedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ReplenishmentPlans", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_ReplenishmentPlans_Products_ProductId",
            //            column: x => x.ProductId,
            //            principalTable: "Products",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "StockItems",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ProductId = table.Column<int>(type: "int", nullable: false),
            //        WarehouseId = table.Column<int>(type: "int", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        ReservedQuantity = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_StockItems", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_StockItems_Products_ProductId",
            //            column: x => x.ProductId,
            //            principalTable: "Products",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_StockItems_Warehouses_WarehouseId",
            //            column: x => x.WarehouseId,
            //            principalTable: "Warehouses",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OrderItems",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        OrderId = table.Column<int>(type: "int", nullable: false),
            //        ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //        Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OrderItems", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_OrderItems_Orders_OrderId",
            //            column: x => x.OrderId,
            //            principalTable: "Orders",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DocumentItems",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        DocumentId = table.Column<int>(type: "int", nullable: false),
            //        ProductId = table.Column<int>(type: "int", nullable: false),
            //        Quantity = table.Column<int>(type: "int", nullable: false),
            //        Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DocumentItems", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_DocumentItems_Products_ProductId",
            //            column: x => x.ProductId,
            //            principalTable: "Products",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_DocumentItems_WarehouseDocuments_DocumentId",
            //            column: x => x.DocumentId,
            //            principalTable: "WarehouseDocuments",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_DocumentItems_DocumentId",
            //    table: "DocumentItems",
            //    column: "DocumentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_DocumentItems_ProductId",
            //    table: "DocumentItems",
            //    column: "ProductId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_OrderItems_OrderId",
            //    table: "OrderItems",
            //    column: "OrderId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Orders_UserId",
            //    table: "Orders",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Products_CategoryId",
            //    table: "Products",
            //    column: "CategoryId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Products_StatusId",
            //    table: "Products",
            //    column: "StatusId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Products_SupplierId",
            //    table: "Products",
            //    column: "SupplierId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Products_UnitId",
            //    table: "Products",
            //    column: "UnitId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ReplenishmentPlans_ProductId",
            //    table: "ReplenishmentPlans",
            //    column: "ProductId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StockItems_ProductId",
            //    table: "StockItems",
            //    column: "ProductId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_StockItems_WarehouseId",
            //    table: "StockItems",
            //    column: "WarehouseId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserActions_UserId",
            //    table: "UserActions",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_WarehouseDocuments_DocumentTypeId",
            //    table: "WarehouseDocuments",
            //    column: "DocumentTypeId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_WarehouseDocuments_SupplierId",
            //    table: "WarehouseDocuments",
            //    column: "SupplierId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_WarehouseDocuments_WarehouseId",
            //    table: "WarehouseDocuments",
            //    column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "DocumentItems");

            //migrationBuilder.DropTable(
            //    name: "OrderItems");

            //migrationBuilder.DropTable(
            //    name: "ReplenishmentPlans");

            //migrationBuilder.DropTable(
            //    name: "StockItems");

            //migrationBuilder.DropTable(
            //    name: "UserActions");

            //migrationBuilder.DropTable(
            //    name: "WarehouseDocuments");

            //migrationBuilder.DropTable(
            //    name: "Orders");

            //migrationBuilder.DropTable(
            //    name: "Products");

            //migrationBuilder.DropTable(
            //    name: "DocumentTypes");

            //migrationBuilder.DropTable(
            //    name: "Warehouses");

            //migrationBuilder.DropTable(
            //    name: "Users");

            //migrationBuilder.DropTable(
            //    name: "Categories");

            //migrationBuilder.DropTable(
            //    name: "Statuses");

            //migrationBuilder.DropTable(
            //    name: "Suppliers");

            //migrationBuilder.DropTable(
            //    name: "Units");
        }
    }
}
