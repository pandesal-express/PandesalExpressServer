using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PandesalExpress.Infrastructure.Models;
using Shared.Utils;

namespace PandesalExpress.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<
    Employee, AppRole, Ulid, // User, Role, Key
    IdentityUserClaim<Ulid>, IdentityUserRole<Ulid>, // UserClaim, UserRole
    IdentityUserLogin<Ulid>, IdentityRoleClaim<Ulid>, IdentityUserToken<Ulid> // UserLogin, RoleClaim, UserToken
>(options)
{
    private const string ID_COLUMN_TYPE = "char(26)";
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<SalesLog> SalesLogs { get; set; }
    public DbSet<SalesLogItem> SalesLogItems { get; set; }
    public DbSet<StoreInventory> StoreInventories { get; set; }
    public DbSet<PdndRequest> PdndRequests { get; set; }
    public DbSet<PdndRequestItem> PdndRequestItems { get; set; }
    public DbSet<TransferRequest> TransferRequests { get; set; }
    public DbSet<TransferRequestItem> TransferRequestItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Model Relationships Configuration

        // Attendances -> Employee
        builder.Entity<Attendance>()
               .HasOne(e => e.Employee)
               .WithMany(e => e.Attendances)
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // Departments -> Employees
        builder.Entity<Department>()
               .HasMany(e => e.Employees)
               .WithOne(e => e.Department)
               .HasForeignKey(e => e.DepartmentId)
               .OnDelete(DeleteBehavior.Cascade);

        // Payrolls -> Employee
        builder.Entity<Payroll>()
               .HasOne(e => e.Employee)
               .WithMany(e => e.Payrolls)
               .HasForeignKey(e => e.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // Employees -> Store (optional)
        builder.Entity<Employee>()
               .HasOne(e => e.Store)
               .WithMany(e => e.Employees)
               .HasForeignKey(e => e.StoreId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        // Stores unique index
        builder.Entity<Store>()
               .HasIndex(e => e.StoreKey)
               .IsUnique();

        // SalesLog -> Employee
        builder.Entity<SalesLog>()
               .HasOne(sl => sl.Employee)
               .WithMany(e => e.SalesLogProcessed)
               .HasForeignKey(sl => sl.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // SalesLog -> Store
        builder.Entity<SalesLog>()
               .HasOne(sl => sl.Store)
               .WithMany(s => s.SalesLogs)
               .HasForeignKey(sl => sl.StoreId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // SalesLogItem -> SalesLog and Product (a joint table)
        builder.Entity<SalesLogItem>()
               .HasOne(sli => sli.SalesLog)
               .WithMany(sl => sl.SalesLogItems)
               .HasForeignKey(sli => sli.SalesLogId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<SalesLogItem>()
               .HasOne(sli => sli.Product)
               .WithMany(p => p.SalesLogItems)
               .HasForeignKey(sli => sli.ProductId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // StoreInventory -> Store and Product
        builder.Entity<StoreInventory>()
               .HasOne(si => si.Store)
               .WithMany(s => s.StoreInventories)
               .HasForeignKey(si => si.StoreId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<StoreInventory>()
               .HasOne(si => si.Product)
               .WithMany(p => p.StoreInventories)
               .HasForeignKey(si => si.ProductId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // PdndRequest
        builder.Entity<PdndRequest>()
               .HasOne(e => e.Store)
               .WithMany(e => e.PdndRequests)
               .HasForeignKey(e => e.StoreId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<PdndRequest>()
               .HasOne(e => e.RequestingEmployee)
               .WithMany(e => e.PdndRequests)
               .HasForeignKey(e => e.RequestingEmployeeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<PdndRequest>()
               .HasOne(e => e.Commissary)
               .WithMany()
               .HasForeignKey(e => e.CommissaryId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
        builder.Entity<PdndRequest>()
               .HasOne(e => e.LastUpdatedByEmployee)
               .WithMany()
               .HasForeignKey(e => e.LastUpdatedBy)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);


        // PdndRequestItem -> PdndRequest and Product
        builder.Entity<PdndRequestItem>()
               .HasOne(e => e.PdndRequest)
               .WithMany(e => e.PdndRequestItems)
               .HasForeignKey(e => e.PdndRequestId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<PdndRequestItem>()
               .HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

        // TransferRequest
        builder.Entity<TransferRequest>()
               .HasOne(e => e.SendingStore)
               .WithMany()
               .HasForeignKey(e => e.SendingStoreId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        builder.Entity<TransferRequest>()
               .HasOne(e => e.ReceivingStore)
               .WithMany()
               .HasForeignKey(e => e.ReceivingStoreId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        builder.Entity<TransferRequest>()
               .HasOne(e => e.InitiatingEmployee)
               .WithMany()
               .HasForeignKey(e => e.InitiatingEmployeeId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();
        builder.Entity<TransferRequest>()
               .HasOne(e => e.RespondingEmployee)
               .WithMany()
               .HasForeignKey(e => e.RespondingEmployeeId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        // TransferRequestItem
        builder.Entity<TransferRequestItem>()
               .HasOne(e => e.TransferRequest)
               .WithMany(e => e.Items)
               .HasForeignKey(e => e.TransferRequestId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();
        builder.Entity<TransferRequestItem>()
               .HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

        //  Model Properties Configuration

        // Employee config
        builder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
                entity.Property(e => e.Email).HasMaxLength(254);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(254);
                entity.Property(e => e.DepartmentId).HasColumnType(ID_COLUMN_TYPE).IsRequired();
                entity.Property(e => e.StoreId).HasColumnType(ID_COLUMN_TYPE).IsRequired(false);
                entity.Property(e => e.FirstName).HasColumnType("varchar(70)");
                entity.Property(e => e.LastName).HasColumnType("varchar(70)");
                entity.Property(e => e.Position).HasColumnType("varchar(180)");
                entity.Property(e => e.SssNumber).HasColumnType("varchar(10)").IsRequired(false);
                entity.Property(e => e.TinNumber).HasColumnType("varchar(12)").IsRequired(false);
                entity.Property(e => e.PhilHealthNumber).HasColumnType("varchar(12)").IsRequired(false);
                entity.Property(e => e.PagIbigNumber).HasColumnType("varchar(12)").IsRequired(false);
            }
        );

        builder.Entity<AppRole>(entity => { entity.Property(r => r.Id).HasColumnType(ID_COLUMN_TYPE).ValueGeneratedNever(); }
        );

        // -- PK --
        builder.Entity<Department>().HasKey(e => e.Id);
        builder.Entity<Attendance>().HasKey(e => e.Id);
        builder.Entity<Payroll>().HasKey(e => e.Id);
        builder.Entity<Store>().HasKey(e => e.Id);
        builder.Entity<Product>().HasKey(e => e.Id);
        builder.Entity<SalesLog>().HasKey(e => e.Id);
        builder.Entity<SalesLogItem>().HasKey(e => e.Id);
        builder.Entity<StoreInventory>().HasKey(e => e.Id);
        builder.Entity<PdndRequest>().HasKey(e => e.Id);
        builder.Entity<PdndRequestItem>().HasKey(e => e.Id);
        builder.Entity<TransferRequest>().HasKey(e => e.Id);
        builder.Entity<TransferRequestItem>().HasKey(e => e.Id);

        // Department config
        builder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.Name).HasColumnType("varchar(90)").IsRequired();
            }
        );

        // Attendance config
        builder.Entity<Attendance>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.EmployeeId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.Status)
                      .HasColumnType("varchar(10)")
                      .HasConversion<string>();
            }
        );

        // Payroll config
        builder.Entity<Payroll>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.EmployeeId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.BaseSalary).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.SssDeduction).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PhilHealthDeduction).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.PagIbigDeduction).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.LoanDeduction).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Overtime).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Bonus).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.TotalSalary).HasColumnType("decimal(18, 2)");
            }
        );

        // Store config
        builder.Entity<Store>(entity =>
            {
                entity.Property(s => s.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(s => s.StoreKey).HasColumnType("varchar(50)");
                entity.Property(s => s.Name).HasColumnType("varchar(90)");
                entity.Property(s => s.Address).HasColumnType("text");
                entity.Property(s => s.OpeningTime).HasColumnType("time");
                entity.Property(s => s.ClosingTime).HasColumnType("time");
            }
        );

        // Product config
        builder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(p => p.Name).HasColumnType("varchar(180)");
                entity.Property(p => p.Price).HasColumnType("decimal(18, 2)");
                entity.Property(p => p.Shift).HasColumnType("varchar(4)");
                entity.Property(p => p.Quantity).HasColumnType("int");
                entity.Property(p => p.Description).HasColumnType("text");
            }
        );

        // Sales Log config
        builder.Entity<SalesLog>(entity =>
            {
                entity.Property(sl => sl.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(sl => sl.StoreId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(sl => sl.EmployeeId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(sl => sl.Name).HasColumnType("varchar(180)");
                entity.Property(sl => sl.Shift).HasColumnType("varchar(15)");
                entity.Property(sl => sl.Quantity).HasColumnType("int");
                entity.Property(sl => sl.TotalPrice).HasColumnType("decimal(18, 2)");
            }
        );

        // Sales Log Item config
        builder.Entity<SalesLogItem>(entity =>
            {
                entity.Property(sli => sli.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(sli => sli.SalesLogId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(sli => sli.ProductId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(sli => sli.Quantity).HasColumnType("int");
                entity.Property(sli => sli.PriceAtSale).HasColumnType("decimal(18, 2)");
                entity.Property(sli => sli.Amount).HasColumnType("decimal(18, 2)");
            }
        );

        // Store Inventory config
        builder.Entity<StoreInventory>(entity =>
            {
                entity.Property(si => si.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(si => si.StoreId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(si => si.ProductId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(si => si.Quantity).HasColumnType("int");
                entity.Property(si => si.Price).HasColumnType("decimal(18, 2)");
                entity.Property(si => si.LastVerified).HasColumnType("timestamp with time zone");
            }
        );

        // PDND Request config
        builder.Entity<PdndRequest>(entity =>
            {
                entity.Property(p => p.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(p => p.StoreId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(p => p.RequestingEmployeeId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(p => p.CommissaryId)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .IsRequired(false);
                entity.Property(p => p.RequestDate).HasColumnType("timestamp with time zone");
                entity.Property(p => p.DateNeeded).HasColumnType("timestamp with time zone");
                entity.Property(p => p.Status).HasColumnType("varchar(15)");
                entity.Property(p => p.CommissaryNotes)
                      .HasColumnType("varchar(500)")
                      .IsRequired(false);
                entity.Property(p => p.StatusLastUpdated)
                      .HasColumnType("timestamp with time zone")
                      .IsRequired(false);
                entity.Property(p => p.LastUpdatedBy)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .IsRequired(false);
            }
        );

        // PDND Request Item config
        builder.Entity<PdndRequestItem>(entity =>
            {
                entity.Property(p => p.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(p => p.PdndRequestId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(p => p.ProductId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(p => p.ProductName).HasColumnType("varchar(180)");
                entity.Property(p => p.Quantity).HasColumnType("int");
                entity.Property(p => p.TotalAmount).HasColumnType("decimal(18, 2)");
            }
        );

        // TransferRequest config
        builder.Entity<TransferRequest>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.SendingStoreId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.ReceivingStoreId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.InitiatingEmployeeId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.RespondingEmployeeId).HasColumnType(ID_COLUMN_TYPE).IsRequired(false);
                entity.Property(e => e.Status).HasColumnType("varchar(15)").HasConversion<string>();
                entity.Property(e => e.RequestNotes).HasColumnType("varchar(500)").IsRequired(false);
                entity.Property(e => e.ResponseNotes).HasColumnType("varchar(500)").IsRequired(false);
                entity.Property(e => e.ShippedAt).HasColumnType("timestamp with time zone").IsRequired(false);
                entity.Property(e => e.ReceivedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            }
        );

        // TransferRequestItem config
        builder.Entity<TransferRequestItem>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnType(ID_COLUMN_TYPE)
                      .ValueGeneratedNever();
                entity.Property(e => e.TransferRequestId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.ProductId).HasColumnType(ID_COLUMN_TYPE);
                entity.Property(e => e.QuantityRequested).HasColumnType("int");
            }
        );
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Ulid>().HaveConversion<UlidConverter<string>>();
    }
}
