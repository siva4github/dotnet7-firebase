using Firebase.Auth;
using FirebaseLoginAuth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FirebaseLoginAuth.Controllers
{
    public class HomeController : Controller
    {
        private FirebaseAuthProvider _auth;

        public HomeController()
        {
            _auth = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyDriZw1ynbIGAiQqJN-0D3zLQwvmI6-N44"));
        }

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("_UserToken");

            if (token != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("SignIn");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(LoginModel loginModel)
        {
            try
            {
                // create the user

                await _auth.CreateUserWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);

                // log in the new user
                var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = fbAuthLink.FirebaseToken;

                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("_UserToken", token);

                    return RedirectToAction("Index");
                }

            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonSerializer.Deserialize<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(string.Empty, firebaseEx!.Error.Message);
                return View(loginModel);
            }

            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginModel loginModel)
        {
            try
            {
                // log in an exisitng user
                var firebaseLink = await _auth.SignInWithEmailAndPasswordAsync(loginModel.Email, loginModel.Password);
                string token = firebaseLink.FirebaseToken;
                if (!string.IsNullOrEmpty(token))
                {
                    HttpContext.Session.SetString("_UserToken", token);

                    return RedirectToAction("Index");
                }
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonSerializer.Deserialize<FirebaseError>(ex.ResponseData);
                ModelState.AddModelError(string.Empty, firebaseEx!.Error.Message);
                return View(loginModel);
            }
            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("_UserToken");
            return RedirectToAction("SignIn");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}