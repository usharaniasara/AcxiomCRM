using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roles = new[] { "Admin", "Manager", "SalesExecutive" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@axciomcrm.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Manager User
            var managerEmail = "manager@axciomcrm.com";
            var managerUser = await userManager.FindByEmailAsync(managerEmail);
            if (managerUser == null)
            {
                managerUser = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    FullName = "Sales Manager",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(managerUser, "Manager@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // 4. Seed Sales Executive User
            var salesEmail = "sales@axciomcrm.com";
            var salesUser = await userManager.FindByEmailAsync(salesEmail);
            if (salesUser == null)
            {
                salesUser = new ApplicationUser
                {
                    UserName = salesEmail,
                    Email = salesEmail,
                    FullName = "Alex SalesRep",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(salesUser, "Sales@12345");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(salesUser, "SalesExecutive");
                }
            }

            // Reload user IDs after creation
            adminUser = await userManager.FindByEmailAsync(adminEmail);
            managerUser = await userManager.FindByEmailAsync(managerEmail);
            salesUser = await userManager.FindByEmailAsync(salesEmail);

            // 5. Seed Customers
            if (!context.Customers.Any())
            {
                var customers = new List<Customer>
                {
                    new Customer
                    {
                        CustomerCode = "CUST-1001",
                        CustomerName = "Acme Global Solutions",
                        Email = "contact@acmeglobal.com",
                        Phone = "+1-555-0101",
                        CompanyName = "Acme Corp",
                        Address = "100 Innovation Way",
                        City = "New York",
                        State = "NY",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow.AddDays(-60),
                        CreatedBy = "admin@axciomcrm.com"
                    },
                    new Customer
                    {
                        CustomerCode = "CUST-1002",
                        CustomerName = "Apex Logistics Inc",
                        Email = "info@apexlogistics.com",
                        Phone = "+1-555-0102",
                        CompanyName = "Apex Logistics",
                        Address = "250 Supply Chain Blvd",
                        City = "Chicago",
                        State = "IL",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow.AddDays(-45),
                        CreatedBy = "manager@axciomcrm.com"
                    },
                    new Customer
                    {
                        CustomerCode = "CUST-1003",
                        CustomerName = "Starlight Media",
                        Email = "hello@starlight.com",
                        Phone = "+1-555-0103",
                        CompanyName = "Starlight Network",
                        Address = "50 Hollywood Lane",
                        City = "Los Angeles",
                        State = "CA",
                        Status = "Active",
                        CreatedDate = DateTime.UtcNow.AddDays(-30),
                        CreatedBy = "sales@axciomcrm.com"
                    }
                };

                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
            }

            // 6. Seed Leads
            if (!context.Leads.Any())
            {
                var leads = new List<Lead>
                {
                    new Lead
                    {
                        LeadCode = "LEAD-1001",
                        LeadName = "Tech Dynamics",
                        Email = "inquiries@techdynamics.io",
                        Phone = "+1-555-0201",
                        CompanyName = "Tech Dynamics LLC",
                        Source = "Website",
                        Status = "New",
                        ExpectedValue = 45000,
                        CreatedDate = DateTime.UtcNow.AddDays(-15),
                        AssignedToUserId = salesUser?.Id
                    },
                    new Lead
                    {
                        LeadCode = "LEAD-1002",
                        LeadName = "BioHealth Labs",
                        Email = "bizdev@biohealth.org",
                        Phone = "+1-555-0202",
                        CompanyName = "BioHealth Systems",
                        Source = "Referral",
                        Status = "Contacted",
                        ExpectedValue = 75000,
                        CreatedDate = DateTime.UtcNow.AddDays(-10),
                        AssignedToUserId = salesUser?.Id
                    },
                    new Lead
                    {
                        LeadCode = "LEAD-1003",
                        LeadName = "Horizon Energy",
                        Email = "lead@horizonenergy.com",
                        Phone = "+1-555-0203",
                        CompanyName = "Horizon Green Energy",
                        Source = "Trade Show",
                        Status = "Qualified",
                        ExpectedValue = 120000,
                        CreatedDate = DateTime.UtcNow.AddDays(-5),
                        AssignedToUserId = managerUser?.Id
                    }
                };

                await context.Leads.AddRangeAsync(leads);
                await context.SaveChangesAsync();
            }

            // 7. Seed Opportunities
            if (!context.Opportunities.Any())
            {
                var cust1 = context.Customers.FirstOrDefault(c => c.CustomerCode == "CUST-1001");
                var cust2 = context.Customers.FirstOrDefault(c => c.CustomerCode == "CUST-1002");

                var opportunities = new List<Opportunity>
                {
                    new Opportunity
                    {
                        OpportunityName = "Acme Cloud ERP Upgrade",
                        CustomerId = cust1?.CustomerId,
                        Amount = 150000,
                        Stage = "Proposal",
                        Probability = 60,
                        ExpectedCloseDate = DateTime.UtcNow.AddDays(20),
                        Status = "Open",
                        CreatedDate = DateTime.UtcNow.AddDays(-25),
                        AssignedToUserId = salesUser?.Id,
                        Notes = "Client requested customized proposal for enterprise license."
                    },
                    new Opportunity
                    {
                        OpportunityName = "Apex Fleet Tracking Integration",
                        CustomerId = cust2?.CustomerId,
                        Amount = 85000,
                        Stage = "Negotiation",
                        Probability = 80,
                        ExpectedCloseDate = DateTime.UtcNow.AddDays(10),
                        Status = "Open",
                        CreatedDate = DateTime.UtcNow.AddDays(-18),
                        AssignedToUserId = salesUser?.Id,
                        Notes = "Negotiating payment terms for annual subscription."
                    },
                    new Opportunity
                    {
                        OpportunityName = "Starlight Content Management System",
                        CustomerId = cust1?.CustomerId,
                        Amount = 50000,
                        Stage = "Won",
                        Probability = 100,
                        ExpectedCloseDate = DateTime.UtcNow.AddDays(-2),
                        Status = "Won",
                        CreatedDate = DateTime.UtcNow.AddDays(-40),
                        AssignedToUserId = managerUser?.Id,
                        Notes = "Contract signed successfully."
                    }
                };

                await context.Opportunities.AddRangeAsync(opportunities);
                await context.SaveChangesAsync();
            }

            // 8. Seed FollowUps
            if (!context.FollowUps.Any())
            {
                var cust1 = context.Customers.FirstOrDefault(c => c.CustomerCode == "CUST-1001");
                var lead1 = context.Leads.FirstOrDefault(l => l.LeadCode == "LEAD-1001");

                var followUps = new List<FollowUp>
                {
                    new FollowUp
                    {
                        CustomerId = cust1?.CustomerId,
                        FollowUpDate = DateTime.UtcNow.AddDays(2),
                        FollowUpType = "Meeting",
                        Remarks = "Discuss final pricing and scope of work.",
                        Status = "Planned",
                        AssignedToUserId = salesUser?.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-2)
                    },
                    new FollowUp
                    {
                        LeadId = lead1?.LeadId,
                        FollowUpDate = DateTime.UtcNow.AddDays(5),
                        FollowUpType = "Call",
                        Remarks = "Introductory call to understand software requirements.",
                        Status = "Planned",
                        AssignedToUserId = salesUser?.Id,
                        CreatedDate = DateTime.UtcNow.AddDays(-1)
                    }
                };

                await context.FollowUps.AddRangeAsync(followUps);
                await context.SaveChangesAsync();
            }

            // 9. Seed Activities
            if (!context.Activities.Any())
            {
                var cust1 = context.Customers.FirstOrDefault(c => c.CustomerCode == "CUST-1001");
                var lead2 = context.Leads.FirstOrDefault(l => l.LeadCode == "LEAD-1002");

                var activities = new List<Activity>
                {
                    new Activity
                    {
                        ActivityType = "Email",
                        Subject = "Sent product brochure and pricing sheet",
                        Description = "Emailed detailed specification sheet to procurement team.",
                        ActivityDate = DateTime.UtcNow.AddDays(-3),
                        CustomerId = cust1?.CustomerId,
                        AssignedToUserId = salesUser?.Id,
                        Status = "Completed",
                        CreatedDate = DateTime.UtcNow.AddDays(-3)
                    },
                    new Activity
                    {
                        ActivityType = "Call",
                        Subject = "Discovery call with VP of Engineering",
                        Description = "Discussed infrastructure requirements and timeline.",
                        ActivityDate = DateTime.UtcNow.AddDays(-1),
                        LeadId = lead2?.LeadId,
                        AssignedToUserId = salesUser?.Id,
                        Status = "Completed",
                        CreatedDate = DateTime.UtcNow.AddDays(-1)
                    }
                };

                await context.Activities.AddRangeAsync(activities);
                await context.SaveChangesAsync();
            }

            // 10. Seed Audit Logs
            if (!context.AuditLogs.Any())
            {
                var auditLog = new AuditLog
                {
                    UserId = adminUser?.Id ?? "SYSTEM",
                    UserName = adminEmail,
                    Action = "SystemInitialize",
                    EntityName = "Database",
                    RecordId = "0",
                    OldValue = null,
                    NewValue = "Initial database seed completed successfully.",
                    CreatedDate = DateTime.UtcNow,
                    IpAddress = "127.0.0.1"
                };

                await context.AuditLogs.AddAsync(auditLog);
                await context.SaveChangesAsync();
            }
        }
    }
}
