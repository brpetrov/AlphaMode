using AlphaMode.Data;
using AlphaMode.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

//Custom services
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();


//Build Admin Role if it doesn't exist

//using (var scope = app.Services.CreateScope())
//{
//    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    var roles = new[] { "Admin" };

//    foreach (var r in roles)
//        if (!await roleMgr.RoleExistsAsync(r))
//            await roleMgr.CreateAsync(new IdentityRole(r));

//    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

//    var adminEmail = "brpetrov@outlook.com";   // ✏️ your account
//    var adminUser = await userMgr.FindByEmailAsync(adminEmail);

//    if (adminUser != null && !await userMgr.IsInRoleAsync(adminUser, "Admin"))
//        await userMgr.AddToRoleAsync(adminUser, "Admin");
//}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

