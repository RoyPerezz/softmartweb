using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SoftmartWeb.Models;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace SoftmartWeb.Controllers
{

    [Authorize]
    public class CuentasController : Controller
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        public readonly UrlEncoder _urlEncoder;

        public CuentasController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailSender emailSender, UrlEncoder urlEncoder, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _urlEncoder = urlEncoder;
            _roleManager = roleManager;
        }
        [HttpGet]

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
    
        [AllowAnonymous]
        public async Task<IActionResult>Registro(string returnurl= null)
        {
            //Para la creacion de los roles
            if (!await _roleManager.RoleExistsAsync("Administrador"))//si el role no existe lo crea
            {
                //Creacion del rol administrador
                await _roleManager.CreateAsync(new IdentityRole("Administrador"));
            }

            if (!await _roleManager.RoleExistsAsync("Registrado"))//si el role no existe lo crea
            {
                //Creacion del rol registrado
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }

            ViewData["ReturnUrl"] = returnurl;
            RegistroViewModel registroVM = new RegistroViewModel();
            return View(registroVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        [AllowAnonymous]
        public async Task<IActionResult> Registro(RegistroViewModel rgViewModel,string returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl??Url.Content("~/");
            if (ModelState.IsValid)//validar el modelo
            {             
               
                var usuario = new AppUsuario { 
                    UserName = rgViewModel.Email,
                    Email = rgViewModel.Email,
                    Nombre=rgViewModel.Nombre,
                    Url = rgViewModel.Url,
                    CodigoPais=rgViewModel.CodigoPais,
                    Telefono = rgViewModel.Telefono,
                    Pais = rgViewModel.Pais,
                    Ciudad = rgViewModel.Ciudad,
                    Direccion = rgViewModel.Direccion,
                    FechaNacimiento = rgViewModel.FechaNacimiento,
                    Estado = rgViewModel.Estado
                };
                var resultado = await _userManager.CreateAsync(usuario, rgViewModel.Password);

                if (resultado.Succeeded)
                {

                    //Asignar un role al usuario registrado
                    await _userManager.AddToRoleAsync(usuario,"Registrado");


                    //implementacion de comfirmacion de email en el registro
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(usuario); //para generar el token de confirmacion
                    var urlRetorno = Url.Action("ConfirmarEmail", "Cuentas", new { userId = usuario.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await _emailSender.SendEmailAsync(rgViewModel.Email, "Confirmar su cuenta - Osmartweb",
                    "Por favor confirme su cuenta aquí: <a href=\"" + urlRetorno + "\">enlace</a>");

                    await _signInManager.SignInAsync(usuario, isPersistent: false);
                    return LocalRedirect(returnurl);
                }

                ValidarErrores(resultado);

            }
            return View(rgViewModel);
        }

        //Manejador de Errores
        [AllowAnonymous]
        private void ValidarErrores(IdentityResult resultado)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(String.Empty,error.Description);
            }
        }

        //Método mostrar formulario de acceso
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Acceso(string returnurl=null)
        {
            ViewData["ReturnUrl"] = returnurl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Acceso(AccesoViewModel accViewModel,string returnurl=null)
           
        {
            ViewData["ReturnUrl"] = returnurl;
            returnurl = returnurl??Url.Content("~/");
            if (ModelState.IsValid)//validar el modelo
            {

                var resultado = await _signInManager.PasswordSignInAsync(accViewModel.Email, accViewModel.Password, accViewModel.RememberMe,lockoutOnFailure:true);

                if (resultado.Succeeded)
                {

                    // return RedirectToAction("Index", "Home");
                    return LocalRedirect(returnurl);
                }
                if (resultado.IsLockedOut)
                {

                   
                    return View("Bloqueado");
                }

                //para la autenticacion de dos factores
                if (resultado.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(VerificarCodigoAutenticador), new { returnurl = accViewModel.RememberMe });
                }
                else
                {
                    ModelState.AddModelError(String.Empty,"Acceso Inválido");
                    return View(accViewModel);
                }

              

            }
            return View(accViewModel);
        }


        //Salir o cerrar la aplicacion (logout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirAplicacion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //METODO OLVIDO DE CONTRASEña

        [HttpGet]
        [AllowAnonymous]
        public IActionResult OlvidoPassword()
        {

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> OlvidoPassword(OlvidoPasswordViewModel opViewModel)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(opViewModel.Email);
                if (usuario== null)
                {
                    return RedirectToAction("ConfirmacionOlvidoPassword");
                }

                var codigo = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var urlRetorno = Url.Action("ResetPassword","Cuentas", new {userId = usuario.Id,code=codigo }, protocol: HttpContext.Request.Scheme);
                await _emailSender.SendEmailAsync(opViewModel.Email,"Recuperar contraseña - Osmartweb",
                    "Por favor recupere su contraseña dando click aquí: <a href=\""+urlRetorno+"\">enlace</a>");

                return RedirectToAction("ConfirmacionOlvidoPassword");
            }

            return View(opViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionOlvidoPassword()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code= null)
        {
            return code== null ? View("Error"):View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(RecuperaPasswordViewModel rpViewModel )
        {
            if (ModelState.IsValid)
            {
                var usuario = await _userManager.FindByEmailAsync(rpViewModel.Email);
                if (usuario == null)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                var resultado = await _userManager.ResetPasswordAsync(usuario,rpViewModel.Code,rpViewModel.Password);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("ConfirmacionRecuperaPassword");
                }

                ValidarErrores(resultado);
            }

            return View(rpViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmacionRecuperaPassword()
        {
            return View();
        }


        //Metodo para confirmacion de email en el registro
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail(string userId, string code)
        {

            if (userId == null || code == null)
            {
                return View("Error");
            }

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                return View("Error");
            }

            var resultado = await _userManager.ConfirmEmailAsync(usuario,code); 
            return View(resultado.Succeeded ? "ConfirmarEmail":"Error");
        }

        //configuracion de acceso externo
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult AccesoExterno(string proveedor, string returnUrl = null)
        {
            //este metodo se auttentica con faceobook y devuelve un url
            var urlRedireccion = Url.Action("AccesoExternoCallback", "Cuentas", new { ReturnUrl=returnUrl});
            var propiedades = _signInManager.ConfigureExternalAuthenticationProperties(proveedor, urlRedireccion);

            return Challenge(propiedades,proveedor);//challenge detecta el tipo de autenticacion
        }

        [HttpGet]
        [AllowAnonymous]

        public async Task<IActionResult> AccesoExternoCallback(string returnurl = null, string error = null)
        {
            returnurl = returnurl ?? Url.Content("~/");
            if (error!=null)
            {
                ModelState.AddModelError(String.Empty,$"Error en el acceso externo {error}");
                return View(nameof(Acceso));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info==null)
            {
                return RedirectToAction(nameof(Acceso));
            }

            //Acceder con el usuario en el proveedor externo
            var resultado = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,info.ProviderKey,isPersistent:false);
            if (resultado.Succeeded)
            {
                //actualizar los tokes de acceso
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return LocalRedirect(returnurl);
            }
            //para la autenticacion de dos factores
            if(resultado.RequiresTwoFactor)
            {
                return RedirectToAction("VerificarCodigoAutenticador",new { returnurl = returnurl});
            }
            else
            {
                //si el usuario no tiene cuenta, pregunta si quiere crear una
                ViewData["RetunrnUrl"] = returnurl;
                ViewData["NombreAMostrarProveedor"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var nombre = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ConfirmacionAccesoExterno", new ConfirmacionAccesoExternoViewModel { Email = email,Name = nombre});
            }

        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmacionAccesoExterno(ConfirmacionAccesoExternoViewModel caeViewModel, string returnurl = null)
        {
            returnurl = returnurl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                //Obtener informacion del usuario del proveedor externo
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info ==null)
                {
                    return View("Error");
                }

                var usuario = new AppUsuario { UserName = caeViewModel.Email,Email = caeViewModel.Email,Nombre = caeViewModel.Name};
                var resultado = await _userManager.CreateAsync(usuario);
                if (resultado.Succeeded)
                {
                    resultado = await _userManager.AddLoginAsync(usuario,info);
                    if (resultado.Succeeded)
                    {
                        await _signInManager.SignInAsync(usuario,isPersistent:false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return LocalRedirect(returnurl);
                    }
                }
                ValidarErrores(resultado);
            }
            ViewData["ReturnUrl"] = returnurl;
            return View(caeViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ActivarAutenticador()
        {
            string formatoUrlAutenticador = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            var usuario = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            var token = await _userManager.GetAuthenticatorKeyAsync(usuario);
            
            //habilitar codigo QR
            string urlAutenticador = string.Format(formatoUrlAutenticador, _urlEncoder.Encode("SoftmartWeb"), _urlEncoder.Encode(usuario.Email),token);
            var adfModel = new AutenticacionDosFactoresViewModel() { Token = token ,UrlCodigoQR = urlAutenticador};
            return View(adfModel);
        }

        [HttpGet]
        public async Task<IActionResult> EliminarAutenticador()
        {
         

            var usuario = await _userManager.GetUserAsync(User);
            await _userManager.ResetAuthenticatorKeyAsync(usuario);
            await _userManager.SetTwoFactorEnabledAsync(usuario,false); //quita la autenticacion de 2 factores
            

            
            return RedirectToAction(nameof(Index),"Home");
        }

        [HttpPost]
        public async Task<IActionResult> ActivarAutenticador(AutenticacionDosFactoresViewModel adfViewModel)
        {
           if(ModelState.IsValid)
            {
                var usuario = await _userManager.GetUserAsync(User);
                var succeded = await _userManager.VerifyTwoFactorTokenAsync(usuario,_userManager.Options.Tokens.AuthenticatorTokenProvider,adfViewModel.Code);
                if (succeded)
                {
                    await _userManager.SetTwoFactorEnabledAsync(usuario,true);
                }
                else
                {
                    ModelState.AddModelError("Error", "Su autenticación de dos factores no ha sido validada");
                    return View(adfViewModel);
                }
            }

            return RedirectToAction(nameof(ConfirmacionAutenticador));
        }

        [HttpGet]
        public IActionResult ConfirmacionAutenticador()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCodigoAutenticador(bool recordarDatos,string returnurl = null)
        {

            var usuario = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (usuario == null)
            {
                return View("Error");
            }

            ViewData["ReturnUrl"] = returnurl;

            return View(new VerificarAutenticadorViewModel {ReturnUrl = returnurl,RecordarDatos = recordarDatos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCodigoAutenticador(VerificarAutenticadorViewModel vaViewModel)
        {

            vaViewModel.ReturnUrl = vaViewModel.ReturnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(vaViewModel);
            }

            //cambiar rememberClient a true para que no me vuelve a pedir el codifo de autenticacion
            var resultado = await _signInManager.TwoFactorAuthenticatorSignInAsync(vaViewModel.Code,vaViewModel.RecordarDatos,rememberClient:true);

            if (resultado.Succeeded)
            {
                return LocalRedirect(vaViewModel.ReturnUrl);
            }if(resultado.IsLockedOut)
            {
                return View("Bloqueado");
            }
            else
            {
                ModelState.AddModelError(String.Empty,"Código inválido");
                return View(vaViewModel);
            }
        }


    }
}
