using Microsoft.AspNetCore.Mvc;
using PJ_JLIGN.Data;
using PJ_JLIGN.Models;
using Microsoft.EntityFrameworkCore;
using PJ_JLIGN.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PJ_JLIGN.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDbContext;
        public AccesoController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            if(modelo.Clave != modelo.ConfirmarClave)
            {
                ViewData["Mensaje"] = "las contraseñas no coiciden";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                NombreCompleto = modelo.NombreCompleto,
                Correo = modelo.Correo,
                Clave = modelo.Clave,
            };

            await _appDbContext.Usuarios.AddAsync(usuario);
            await _appDbContext.SaveChangesAsync();

            if(usuario.IdUsuario != 0) return RedirectToAction("Login","Acceso");

            ViewData["Mensaje"] = "No se pudo crear el usuario";

            return View();
        }
        [HttpGet]
        public IActionResult Alerta()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if(User.Identity!.IsAuthenticated)return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm modelo)
        {
            Usuario? usuario_encontrado = await _appDbContext.Usuarios
                                            .Where(u => u.Correo == modelo.Correo &&
                                                         u.Clave == modelo.Clave).FirstOrDefaultAsync();

            if(usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias";

                return View();
            }

            List<Claim> claims = new List<Claim>()
            { 
                new Claim(ClaimTypes.Name, usuario_encontrado.NombreCompleto)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),
            properties
            );

            return RedirectToAction("Index", "Home");
        }

        
    }
}
