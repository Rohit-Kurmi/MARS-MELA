using System.Data;
using MARS_MELA.Models;
using Microsoft.Data.SqlClient;

namespace MARS_MELA
{
    public class DataAccessLayer
    {

        // Connection string field
        private readonly string cs;

        // Constructor DI (Dependency Injection)
        public DataAccessLayer(ConnectionString con)
        {
            cs = con.cs; // Store database connection string
        }



        public int AddUser(SignUP Sign)
        {
            // Default values for new user
            int EmailVerified = 0;
            int MobileVerified = 0;
            int Status = 1;

            // Create SQL connection
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Use stored procedure: AddUser
                using (SqlCommand cmd = new SqlCommand("AddUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parameters for stored procedure
                    cmd.Parameters.AddWithValue("@MobileNo", Sign.MobileNo);
                    cmd.Parameters.AddWithValue("@EmailID", Sign.EmailID);
                    cmd.Parameters.AddWithValue("@EmailVerified", EmailVerified);
                    cmd.Parameters.AddWithValue("@MobileVerified", MobileVerified);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@FirstName", Sign.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", Sign.LastName);
                    cmd.Parameters.AddWithValue("@CreatedBy", Sign.CreatedBy);

                    // Open connection
                    conn.Open();

                    // Execute the stored procedure and get return value
                    int result = Convert.ToInt32(cmd.ExecuteScalar());

                    return result;
                }
            }
        }



        //-----------------------------------
        // Purpose: Check login conditions based on MobileNo
        // Returns:
        //  - NEED_VERIFICATION : If mobile/email not verified
        //  - CREATE_PASSWORD   : If user exists but no password created
        //  - LOGIN_ALLOWED     : Verified + password exists
        //  - USER_NOT_FOUND    : No user with given mobile
        //-----------------------------------

        public string SignINCheck(SignIN sign)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Query to get verification flags and password hash for given MobileNo
                SqlCommand cmd = new SqlCommand(
                    "SELECT EmailVerified, MobileVerified, PasswordHash FROM Users WHERE MobileNo = @MobileNo",
                    conn
                );

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MobileNo", sign.MobileNo);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // If user exists
                if (dr.Read())
                {
                    bool mobileVerified = Convert.ToBoolean(dr["MobileVerified"]);
                    bool emailVerified = Convert.ToBoolean(dr["EmailVerified"]);
                    bool hasPassword = dr["PasswordHash"] != DBNull.Value;

                    // Step 1: Mobile OR Email not verified
                    if (!mobileVerified || !emailVerified)
                    {
                        return "NEED_VERIFICATION";
                    }

                    // Step 2: Password is not created yet
                    if (!hasPassword)
                    {
                        return "CREATE_PASSWORD";
                    }

                    // Step 3: All good → allow login
                    return "LOGIN_ALLOWED";
                }
                else
                {
                    // No user found with this MobileNo
                    return "USER_NOT_FOUND";
                }
            }
        }



        //-----------------------------------
        // Purpose: Generate a 6-digit OTP and save into DB
        // Returns: OTP (string) if updated, otherwise empty string
        //-----------------------------------

        public string GenerateAndSaveOTP(Verification verify)
        {
            // Generate 6-digit random OTP
            string otp = new Random().Next(100000, 999999).ToString();

            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Updating OTP and OTP timestamp (converted to IST: UTC + 330 minutes)
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
              SET OTPCode = @otp, 
                  OTPGeneratedAt = @OTPGeneratedAt
              WHERE MobileNo = @MobileNo AND EmailID = @EmailID",
                    conn
                );

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@MobileNo", verify.MobileNo);
                cmd.Parameters.AddWithValue("@EmailID", verify.EmailID);
                cmd.Parameters.AddWithValue("@otp", otp);
                cmd.Parameters.AddWithValue("@OTPGeneratedAt", DateTime.Now);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                // If update was successful → return OTP
                if (rowsAffected > 0)
                {
                    return otp;
                }

                // No user matched the Mobile + Email
                return "";
            }
        }


        //-----------------------------------------
        // Purpose: Verify OTP and activate user
        // Returns:
        //   1 => Verification successful
        //   0 => Verification failed (wrong OTP or expired)
        //-----------------------------------------
        public int verification(Verification verify)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Step 1: Fetch stored OTP and timestamp for given mobile + email
                SqlCommand cmd = new SqlCommand(
                    "SELECT OTPCode, OTPGeneratedAt FROM Users WHERE MobileNo=@MobileNo AND EmailID=@EmailID",
                    conn
                );

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@MobileNo", verify.MobileNo);
                cmd.Parameters.AddWithValue("@EmailID", verify.EmailID);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // If record found
                if (dr.Read())
                {
                    string storedOtp = dr["OTPCode"]?.ToString() ?? "";
                    DateTime generatedTime = Convert.ToDateTime(dr["OTPGeneratedAt"]);

                    // Step 2: OTP mismatch
                    if (storedOtp != verify.OTPCode)
                    {
                        return 0;
                    }

                    // Step 3: OTP expired (valid only for 2 minutes)
                    if ((DateTime.Now - generatedTime).TotalMinutes > 2)
                    {
                        return 0;
                    }

                    // Close DataReader before running UPDATE
                    dr.Close();

                    // Step 4: Update verification fields
                    SqlCommand updateCmd = new SqlCommand(
                        @"UPDATE Users 
                  SET EmailVerified = 1, MobileVerified = 1 
                  WHERE MobileNo = @MobileNo AND EmailID = @EmailID",
                        conn
                    );

                    updateCmd.Parameters.AddWithValue("@MobileNo", verify.MobileNo);
                    updateCmd.Parameters.AddWithValue("@EmailID", verify.EmailID);

                    int rowsAffected = updateCmd.ExecuteNonQuery();

                    return rowsAffected > 0 ? 1 : 0;
                }

                // No record found → verification failed
                return 0;
            }
        }


        //------------------------------------------------------
        //-----------------------------------------------------------
        // Purpose: Save/Update user's hashed password
        // Parameters:
        //    mobileNo     → User's mobile number
        //    passwordHash → SHA-256 hashed password
        //    emailid      → User's email ID
        // Returns:
        //    Number of rows updated (1 = success, 0 = failed)
        //-----------------------------------------------------------
        public int SavePassword(string mobileNo, string passwordHash, string emailid)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // SQL command to update user's password
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
              SET PasswordHash = @pwd 
              WHERE MobileNo = @MobileNo AND EmailID = @EmailID",
                    conn
                );

                // Adding parameters safely
                cmd.Parameters.AddWithValue("@pwd", passwordHash);
                cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
                cmd.Parameters.AddWithValue("@EmailID", emailid);

                conn.Open();

                // Execute update and return affected rows
                return cmd.ExecuteNonQuery();
            }
        }



        //------------------------------------------------------
        // Purpose : Validate user login using hashed password
        // Input   : mobile (string), password (already SHA256 hash)
        // Output  : 1 = Login success, 0 = Failed
        //------------------------------------------------------
        public int SignIN(string mobile, string password)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                // Step 1: Get the stored password hash for the given mobile number

                SqlCommand cmd = new SqlCommand(
                    "SELECT PasswordHash FROM Users WHERE MobileNo = @MobileNo",
                    conn
                );

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@MobileNo", mobile);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                // Check if user exists
                if (dr.Read())
                {
                    string storedHash = dr["PasswordHash"]?.ToString() ?? "";

                    // Step 2: Compare stored hash with entered hash
                    if (storedHash == password)
                    {
                        dr.Close();
                        SqlCommand updateCmd = new SqlCommand("UPDATE Users SET LastLoginAt =@LastLoginAt , Status=1 WHERE MobileNo = @MobileNo", conn);
                        updateCmd.Parameters.AddWithValue("@MobileNo", mobile);
                        updateCmd.Parameters.AddWithValue("@LastLoginAt", DateTime.Now);
                        updateCmd.ExecuteNonQuery();

                        return 1; // Login successful 
                    }

                    else
                    {
                        return 0;  // Password mismatch
                    }

                }

                // User does not exist
                return 0;
            }
        }



        //------------------------------------------
        //For MyProfile
        //------------------------------------------

        public string MyProfile(string moblie)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("SELECT CreatedBy FROM Users WHERE MobileNo=@MobileNo", conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@MobileNo", moblie);

                    conn.Open();

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {

                        string result = dr["CreatedBy"].ToString() ?? "";

                        return result;

                    }

                    return null;
                }


            }

            catch (Exception ex)
            {
                return null;
            }

        }



        //------------------------------------------------------


        public void Logout(string mobile)
        {
            try
            {

                using (SqlConnection conn = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Users set Status=0 WHERE MobileNo=@MobileNo", conn);
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@MobileNo", mobile);
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            catch (Exception ex)
            {

            }

        }



        //------------------------------------------





    }
}
