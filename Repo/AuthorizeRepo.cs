using System.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dapper;
using ReppadL.Common;
using ReppadL.IRepo;
using ReppadL.Model;
using ReppadL.Model.Data;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using ReppadL.Model.PostModel;
using ReppadL.Model.Response;

namespace ReppadL.Repo
{
	public class AuthorizeRepo : IAuthorizeRepo
	{
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;

        public AuthorizeRepo(DBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<APIResponse<TokenResponse>> Login(PostUserLogin userLogin)
        {
            try
            {
                if (string.IsNullOrEmpty(userLogin.username) || string.IsNullOrEmpty(userLogin.password))
                {
                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Username and password are required."
                    };
                }

                using (IDbConnection connection = _context.CreateConnection())
                {
                    string query = "SELECT * FROM Users WHERE Username = @Username";
                    var parameters = new DynamicParameters();
                    parameters.Add("@Username", userLogin.username, DbType.String);

                    var user = await connection.QueryFirstOrDefaultAsync<User>(query, parameters);

                    if (user == null)
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 404,
                            Message = "Not found!"
                        };
                    }

                    if (string.IsNullOrEmpty(user.hashed_password) || string.IsNullOrEmpty(user.salt))
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Password not set up properly."
                        };
                    }

                    if (!VerifyPassword(userLogin.password, user.hashed_password, user.salt))
                    {
                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 400,
                            Message = "Incorrect password."
                        };
                    }

                    string queryToken = "SELECT * FROM token_info WHERE userId = @UserId";
                    var token = await connection.QueryFirstOrDefaultAsync<TokenInfo>(queryToken, new { UserId = user.id });

                    if (token != null)
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var oldToken = handler.ReadJwtToken(token.accessToken);

                        if (oldToken.ValidTo >= DateTime.UtcNow)
                        {
                            var result = new TokenResponse
                            {
                                accessToken = token.accessToken,
                                refreshToken = token.refreshToken
                            };

                            return new APIResponse<TokenResponse>
                            {
                                ResponseCode = 200,
                                Message = "Success!",
                                Result = result
                            };
                        }
                    }

                    var accessToken = GenerateAccessToken(user.id);
                    var refreshToken = GenerateRefreshToken();

                    var tokenInfo = new TokenInfo
                    {
                        userId = user.id,
                        accessToken = accessToken,
                        refreshToken = refreshToken,
                        accessTokenExpiration = DateTime.UtcNow.AddHours(24),
                        refreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                    };

                    var isTokenSaved = await SaveTokenInfo(tokenInfo);

                    if (isTokenSaved)
                    {
                        var result = new TokenResponse
                        {
                            accessToken = accessToken,
                            refreshToken = refreshToken
                        };

                        return new APIResponse<TokenResponse>
                        {
                            ResponseCode = 200,
                            Message = "Success!",
                            Result = result
                        };
                    }

                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Failed!",
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIResponse<TokenResponse>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<APIResponse<TokenResponse>> Register(PostUserRegister userRegister)
        {
            try
            {
                var existingUser = await GetUserByUsername(userRegister.username);

                if (existingUser != null)
                {
                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Username already exists."
                    };
                }

                User _user = new User
                {
                    username = userRegister.username,
                    email = userRegister.email,
                    phone = userRegister.phone,
                    password = userRegister.password
                }; ;

                var (hashedPassword, salt) = HashPassword(userRegister.password);
                _user.hashed_password = hashedPassword;
                _user.salt = salt;

                var newUser = await AddUserToDatabase(_user);
                if (newUser == null)
                {
                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 400,
                        Message = "Failed!",
                    };
                }

                var accessToken = GenerateAccessToken(newUser.id);
                var refreshToken = GenerateRefreshToken();

                var tokenInfo = new TokenInfo
                {
                    userId = newUser.id,
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                    accessTokenExpiration = DateTime.UtcNow.AddHours(24),
                    refreshTokenExpiration = DateTime.UtcNow.AddDays(30),
                };

                var isTokenSaved = await SaveTokenInfo(tokenInfo);

                if (isTokenSaved)
                {
                    var result = new TokenResponse
                    {
                        accessToken = accessToken,
                        refreshToken = refreshToken
                    };

                    return new APIResponse<TokenResponse>
                    {
                        ResponseCode = 200,
                        Message = "Success!",
                        Result = result
                    };
                }

                return new APIResponse<TokenResponse>
                {
                    ResponseCode = 400,
                    Message = "Failed!",
                };
            }
            catch (Exception ex)
            {
                return new APIResponse<TokenResponse>
                {
                    ResponseCode = 500,
                    Message = ex.Message
                };
            }
        }

        public async Task<User> GetUserByUsername(string username)
        {

            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = "SELECT * FROM Users WHERE Username = @Username";
                var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
                return user;
            }
        }

        public async Task<User> AddUserToDatabase(User user)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = @"
                    INSERT INTO users (username, hashed_password, salt, email, phone)
                    VALUES (@Username, @HashedPassword, @Salt, @Email, @Phone);
                    SELECT LAST_INSERT_ID();
                ";
                var parameters = new
                {
                    Username = user.username,
                    HashedPassword = user.hashed_password,
                    Salt = user.salt,
                    Email = user.email,
                    Phone = user.phone
                };

                int userId = await connection.ExecuteScalarAsync<int>(query, parameters);

                user.id = userId;
                return user;
            }
        }

        public static (string hashedPassword, string salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            string salt = Convert.ToBase64String(saltBytes);

            string saltedPassword = password + salt;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string hashedPassword = Convert.ToBase64String(hashBytes);

                return (hashedPassword, salt);
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            // Tạo chuỗi mã băm từ mật khẩu và salt lưu trữ
            string saltedPassword = password + salt;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string inputHashedPassword = Convert.ToBase64String(hashBytes);

                // So sánh chuỗi mã băm từ mật khẩu nhập vào với chuỗi mã băm lưu trữ
                return string.Equals(inputHashedPassword, hashedPassword);
            }
        }

        private string GenerateAccessToken(int userId)
        {
            var secretKey = _configuration["AppSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("SecretKey is missing or empty in appsettings.json.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var accessTokenExpiration = DateTime.UtcNow.AddHours(23);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userId.ToString())
            };

            // generate token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessTokenExpiration,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<Boolean> SaveTokenInfo(TokenInfo tokenInfo)
        {
            using (IDbConnection connection = _context.CreateConnection())
            {
                string query = @"
                    INSERT INTO token_info (userId, accessToken, refreshToken, accessTokenExpiration, refreshTokenExpiration)
                    VALUES (@UserId, @AccessToken, @RefreshToken, @AccessTokenExpiration, @RefreshTokenExpiration)
                ";

                int affectedRows = await connection.ExecuteAsync(query, tokenInfo);
                return affectedRows > 0;
            }
        }
    }
}

