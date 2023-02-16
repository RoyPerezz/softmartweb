using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SoftmartWeb.Datos;
using SoftmartWeb.Servicios;

var builder = WebApplication.CreateBuilder(args);

//Configuramos la conexion a Sql Server
builder.Services.AddDbContext<ApplicationDbContext>(optiones =>
optiones.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql"))
);

//Agregar servicio de identity a la aplicacion
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//Esta linea es para la url de retorno de acceder
builder.Services.ConfigureApplicationCookie(options =>
{
    //Sobre escribimos la ruta que necesitamos qu ese usa para el acceso y que no use la por default en identity
    options.LoginPath = new PathString("/Cuentas/Acceso");
    options.AccessDeniedPath = new PathString("/Cuentas/Bloqueado");

});

//Estas son opciones de configuracion del identity
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 5; //Cantidad minima de caracteres para el password
    options.Password.RequireLowercase = true; //El password debe contener minusculas
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);//cuanto tiempo bloquear a un usuario
    options.Lockout.MaxFailedAccessAttempts = 3; //numero de intentos antes de bloquear al usuario
});

//Agregar autenticación de Facebook
builder.Services.AddAuthentication().AddFacebook(options=>
{
    options.AppId = "569615431699657";
    options.AppSecret = "4205cb47de3a0a8e95b9264ba2f3830c";
});

//Agregar autenticacion de Google
builder.Services.AddAuthentication().AddGoogle(options =>
{
    options.ClientId = "385106575888-78rnpn3qovn450rtj7l4ggmtuc8vv032.apps.googleusercontent.com";
    options.ClientSecret = "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"; 
});


//Agregar servicio IEmailSender
builder.Services.AddTransient<IEmailSender, MailJetEmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//se agrega la autenticacion
app.UseAuthentication();
app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
