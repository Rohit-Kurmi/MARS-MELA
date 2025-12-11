using System.Diagnostics;
using MARS_MELA.Models;
using Microsoft.AspNetCore.Mvc;

namespace MARS_MELA.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataAccessLayer DAL;

        public HomeController(DataAccessLayer dal)
        {
            DAL = dal;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //========================================================
        // SIGN UP - GET
        //========================================================
        public IActionResult SignUP()
        {
            return View();
        }


        //========================================================
        // SIGN UP - POST
        //========================================================
        [HttpPost]
        public IActionResult SignUP(SignUP sign)
        {
            if (!ModelState.IsValid)
            {
                return View(sign);
            }

            try
            {
                // Call DAL to insert user into database
                // Return values:
                //  -1 = User already exists
                //   1 = User successfully registered
                int res = DAL.AddUser(sign);

                // CASE 1: User already exists
                if (res == -1)
                {
                    TempData["message"] = "User Already Exists";

                    // Redirect user to SignIN page
                    return RedirectToAction("SignIN", "Home");
                }

                // CASE 2: New user successfully created
                else if (res == 1)
                {
                    TempData["message"] = "User Registered Successfully";

                    // Pass mobile and email to next page using TempData
                    TempData["Mobile"] = sign.MobileNo;
                    TempData["Email"] = sign.EmailID;

                    // Redirect user to OTP verification page
                    return RedirectToAction("Verification", "Home");
                }

                // CASE 3: Unexpected result or failure
                return View();
            }
            catch
            {
                // If any exception occurs, stay on SignUP page
                return View();
            }
        }



        public IActionResult SignIN()
        {
            return View();
        }

        //========================================================
        // SIGN IN - POST
        //========================================================
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult SignIN(SignIN signin)
        {
            // ---------------------------------------------------------
            // Step 1: Check user login status (verification/password)
            // Returns one of the following:
            //
            // "NEED_VERIFICATION" → Mobile/Email not verified yet
            // "CREATE_PASSWORD"   → User is verified but has no password set
            // "LOGIN_ALLOWED"     → User is verified and password exists
            // "USER_NOT_FOUND"    → Mobile number does not exist
            // ---------------------------------------------------------


            if (!ModelState.IsValid)
            {
                return View(signin);
            }

            try
            {


                string result = DAL.SignINCheck(signin);


                // CASE 1: User exists but NOT verified → Send to OTP page
                if (result == "NEED_VERIFICATION")
                {
                    TempData["Mobile"] = signin.MobileNo;      // Store mobile temporarily
                    return RedirectToAction("Verification", "Home");
                }


                // CASE 2: User verified but has NO password yet
                // Redirect user to OTP page so that they can verify
                // and then create a new password
                else if (result == "CREATE_PASSWORD")
                {
                    TempData["Mobile"] = signin.MobileNo;
                    return RedirectToAction("Verification", "Home");
                }


                // CASE 3: User verified AND password already exists
                // Take user to EnterPassword page
                else if (result == "LOGIN_ALLOWED")
                {
                    TempData["Mobile"] = signin.MobileNo;
                    return RedirectToAction("EnterPassword");
                }


                // CASE 4: User does not exist in database
                else
                {
                    ViewBag.msg = "User not found!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }




        //========================================================
        // OTP VERIFICATION - GET
        //========================================================
        public IActionResult Verification()
        {
            return View();
        }

        //========================================================
        // OTP VERIFICATION - POST
        //========================================================
        [HttpPost]
        public IActionResult Verification(Verification verify, string actiontype)
        {
            try
            {
                // ===============================
                // CASE 1: SEND OTP
                // ===============================
                if (actiontype == "Send_OTP")
                {
                    // Remove OTP validation for this action
                    ModelState.Remove("OTPCode");

                    // Validate only Mobile + Email
                    if (!ModelState.IsValid)
                    {
                        return View(verify);
                    }

                    // Generate OTP
                    string otp = DAL.GenerateAndSaveOTP(verify);

                    if (otp != "")
                    {
                        TempData["OTPMessage"] = $"Your OTP is: {otp}";
                        TempData["Mobile"] = verify.MobileNo;
                        TempData["Email"] = verify.EmailID;
                    }

                    return RedirectToAction("Verification");
                }

                // ===============================
                // CASE 2: SUBMIT OTP
                // ===============================
                if (actiontype == "Submit")
                {
                    // Full validation including OTP
                    if (!ModelState.IsValid)
                    {
                        return View(verify);
                    }

                    int result = DAL.verification(verify);

                    if (result == 1)
                    {
                        TempData["Success"] = "Verification successful!";
                        TempData["moblieno"] = verify.MobileNo;
                        TempData["emailid"] = verify.EmailID;
                        return RedirectToAction("CreatePassword");
                    }
                    else
                    {
                        TempData["Error"] = "Invalid or expired OTP!";
                        return View(verify);
                    }
                }

                return View(verify);
            }
            catch
            {
                return View(verify);
            }
        }



        //========================================================
        // CREATE PASSWORD - GET
        //========================================================
        public IActionResult CreatePassword()
        {
            return View();
        }

        //========================================================
        // CREATE PASSWORD - POST
        // Purpose: Save the user's password after verification
        // Input: Users model containing MobileNo, EmailID, and PasswordHash
        //========================================================
        [HttpPost]
        public IActionResult CreatePassword(CreatePassword creatpass)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(creatpass);
                }
                // Step 1: Hash the password using SHA256
                // This ensures the password is stored securely in the database
                PasswordHelper passclass = new PasswordHelper();
                string hashedPassword = passclass.ComputeSha512Hash(creatpass.PasswordHash);

                // Step 2: Save the hashed password in the database
                // DAL.SavePassword returns the number of rows affected
                int result = DAL.SavePassword(creatpass.MobileNo, hashedPassword, creatpass.EmailID);

                // Step 3: Check if the password was saved successfully
                if (result > 0)
                {
                    // Password saved successfully
                    TempData["Success1"] = "Password created successfully!";

                    // Redirect user to SignIN page after password creation
                    return RedirectToAction("SignIN", "Home");
                }

                // Step 4: Password save failed
                // Show error message and stay on CreatePassword page
                else
                {
                    TempData["Error1"] = "Something went wrong!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                return View();
            }
        }



        public IActionResult EnterPassword()
        {
            return View();
        }

        //========================================================
        // ENTER PASSWORD - POST
        // Purpose: Validate user's login using mobile number and password
        // Input: Users model containing MobileNo and PasswordHash
        //========================================================
        [HttpPost]
        public IActionResult EnterPassword(EnterPassword enterpass)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(enterpass);
                }
                // Step 1: Hash the entered password using SHA256
                // This ensures we compare hashed passwords with the database
                PasswordHelper passclass = new PasswordHelper();
                string hashedPassword = passclass.ComputeSha512Hash(enterpass.PasswordHash);

                // Step 2: Check login credentials in the database
                // DAL.SignIN returns:
                //   1 → Login success
                //   0 → Login failed (incorrect password or user not found)
                int result = DAL.SignIN(enterpass.MobileNo, hashedPassword);

                if (result == 1)
                {
                    TempData["Success"] = "User Login Successfully!";

                    HttpContext.Session.SetString("session", enterpass.MobileNo);

                    // Redirect to home page after successful login
                    return RedirectToAction("Index", "Home");
                }

                // Step 4: Handle login failure
                TempData["Error"] = "UserId OR Password Wrong";
            }
            catch (Exception ex)
            {
                return View();
            }

            // Stay on EnterPassword page to retry
            return View();
        }




        //========================================================
        // create MyProfile 
        //========================================================


        public IActionResult MyProfile()
        {
            string session = HttpContext.Session.GetString("session");

            // SESSION CHECK
            if (string.IsNullOrEmpty(session))
            {
                return RedirectToAction("SignIN", "Home");
            }

            // DB SE CHECK
            string result = DAL.MyProfile(session);

            if (string.IsNullOrEmpty(result))
            {
                return RedirectToAction("SignIN", "Home");
            }

            // REDIRECT BASED ON CREATEDBY
            if (result == "Citizen")
            {
                return RedirectToAction("CitizenProfile","Home");
            }

            if (result == "Company")
            {
                return RedirectToAction("CompanyProfile","Home");
            }

            return View();
        }


        //========================================================
        // create CitizenProfile 
        //========================================================

        public IActionResult CitizenProfile()
        {

            return View();
        }


        [HttpPost]
        public IActionResult CitizenProfile(string actiontype)
        {
            string session = HttpContext.Session.GetString("session");

            if (actiontype == "Logout")
            {
                if (!string.IsNullOrEmpty(session))
                {
                    DAL.Logout(session);
                }

                HttpContext.Session.Remove("session");

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //========================================================
        // create CompanyProfile 
        //========================================================


        public IActionResult CompanyProfile()
        {

            return View();
        }



        [HttpPost]
        public IActionResult CompanyProfile(string actiontype)
        {
            string session = HttpContext.Session.GetString("session");

            if (actiontype == "Logout")
            {
                if (!string.IsNullOrEmpty(session))
                {
                    DAL.Logout(session);
                }

                HttpContext.Session.Remove("session");

                return RedirectToAction("Index", "Home");
            }

            return View();
        }







        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
