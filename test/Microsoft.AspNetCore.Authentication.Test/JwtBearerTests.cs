// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.JwtBearer
{
    public class JwtBearerTests
    {
        [Fact]
        public async Task VerifySchemeDefaults()
        {
            var services = new ServiceCollection().AddJwtBearerAuthentication().AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            var sp = services.BuildServiceProvider();
            var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
            Assert.NotNull(scheme);
            Assert.Equal("JwtBearerHandler", scheme.HandlerType.Name);
            Assert.Null(scheme.DisplayName);
        }

        [ConditionalFact(Skip = "Need to remove dependency on AAD since the generated tokens will expire")]
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/179
        public async Task BearerTokenValidation()
        {
            var server = CreateServer(o =>
            {
                o.Authority = "https://login.windows.net/tushartest.onmicrosoft.com";
                o.Audience = "https://TusharTest.onmicrosoft.com/TodoListService-ManualJwt";
                o.TokenValidationParameters.ValidateLifetime = false;
            });

            var newBearerToken = "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImtyaU1QZG1Cdng2OHNrVDgtbVBBQjNCc2VlQSJ9.eyJhdWQiOiJodHRwczovL1R1c2hhclRlc3Qub25taWNyb3NvZnQuY29tL1RvZG9MaXN0U2VydmljZS1NYW51YWxKd3QiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9hZmJlY2UwMy1hZWFhLTRmM2YtODVlNy1jZTA4ZGQyMGNlNTAvIiwiaWF0IjoxNDE4MzMwNjE0LCJuYmYiOjE0MTgzMzA2MTQsImV4cCI6MTQxODMzNDUxNCwidmVyIjoiMS4wIiwidGlkIjoiYWZiZWNlMDMtYWVhYS00ZjNmLTg1ZTctY2UwOGRkMjBjZTUwIiwiYW1yIjpbInB3ZCJdLCJvaWQiOiI1Mzk3OTdjMi00MDE5LTQ2NTktOWRiNS03MmM0Yzc3NzhhMzMiLCJ1cG4iOiJWaWN0b3JAVHVzaGFyVGVzdC5vbm1pY3Jvc29mdC5jb20iLCJ1bmlxdWVfbmFtZSI6IlZpY3RvckBUdXNoYXJUZXN0Lm9ubWljcm9zb2Z0LmNvbSIsInN1YiI6IkQyMm9aMW9VTzEzTUFiQXZrdnFyd2REVE80WXZJdjlzMV9GNWlVOVUwYnciLCJmYW1pbHlfbmFtZSI6Ikd1cHRhIiwiZ2l2ZW5fbmFtZSI6IlZpY3RvciIsImFwcGlkIjoiNjEzYjVhZjgtZjJjMy00MWI2LWExZGMtNDE2Yzk3ODAzMGI3IiwiYXBwaWRhY3IiOiIwIiwic2NwIjoidXNlcl9pbXBlcnNvbmF0aW9uIiwiYWNyIjoiMSJ9.N_Kw1EhoVGrHbE6hOcm7ERdZ7paBQiNdObvp2c6T6n5CE8p0fZqmUd-ya_EqwElcD6SiKSiP7gj0gpNUnOJcBl_H2X8GseaeeMxBrZdsnDL8qecc6_ygHruwlPltnLTdka67s1Ow4fDSHaqhVTEk6lzGmNEcbNAyb0CxQxU6o7Fh0yHRiWoLsT8yqYk8nKzsHXfZBNby4aRo3_hXaa4i0SZLYfDGGYPdttG4vT_u54QGGd4Wzbonv2gjDlllOVGOwoJS6kfl1h8mk0qxdiIaT_ChbDWgkWvTB7bTvBE-EgHgV0XmAo0WtJeSxgjsG3KhhEPsONmqrSjhIUV4IVnF2w";
            var response = await SendAsync(server, "http://example.com/oauth", newBearerToken);
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
        }

        [Fact]
        public async Task SignInThrows()
        {
            var server = CreateServer();
            var transaction = await server.SendAsync("https://example.com/signIn");
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task SignOutThrows()
        {
            var server = CreateServer();
            var transaction = await server.SendAsync("https://example.com/signOut");
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task ThrowAtAuthenticationFailedEvent()
        {
            var server = CreateServer(o =>
            {
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.StatusCode = 401;
                        throw new Exception();
                    },
                    OnMessageReceived = context =>
                    {
                        context.Token = "something";
                        return Task.FromResult(0);
                    }
                };
                o.SecurityTokenValidators.Clear();
                o.SecurityTokenValidators.Insert(0, new InvalidTokenValidator());
            },
            async (context, next) =>
            {
                try
                {
                    await next();
                    Assert.False(true, "Expected exception is not thrown");
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("i got this");
                }
            });

            var transaction = await server.SendAsync("https://example.com/signIn");

            Assert.Equal(HttpStatusCode.Unauthorized, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task CustomHeaderReceived()
        {
            var server = CreateServer(o =>
            {
                o.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "Bob le Magnifique"),
                            new Claim(ClaimTypes.Email, "bob@contoso.com"),
                            new Claim(ClaimsIdentity.DefaultNameClaimType, "bob")
                        };

                        var ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name)),
                            new AuthenticationProperties(), context.Scheme.Name);

                        context.Success(ticket);

                        return Task.FromResult<object>(null);
                    }
                };
            });

            var response = await SendAsync(server, "http://example.com/oauth", "someHeader someblob");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal("Bob le Magnifique", response.ResponseText);
        }

        [Fact]
        public async Task NoHeaderReceived()
        {
            var server = CreateServer();
            var response = await SendAsync(server, "http://example.com/oauth");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
        }

        [Fact]
        public async Task HeaderWithoutBearerReceived()
        {
            var server = CreateServer();
            var response = await SendAsync(server, "http://example.com/oauth", "Token");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
        }

        [Fact]
        public async Task UnrecognizedTokenReceived()
        {
            var server = CreateServer();
            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("", response.ResponseText);
        }

        [Fact]
        public async Task InvalidTokenReceived()
        {
            var server = CreateServer(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new InvalidTokenValidator());
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("Bearer error=\"invalid_token\"", response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Theory]
        [InlineData(typeof(SecurityTokenInvalidAudienceException), "The audience is invalid")]
        [InlineData(typeof(SecurityTokenInvalidIssuerException), "The issuer is invalid")]
        [InlineData(typeof(SecurityTokenNoExpirationException), "The token has no expiration")]
        [InlineData(typeof(SecurityTokenInvalidLifetimeException), "The token lifetime is invalid")]
        [InlineData(typeof(SecurityTokenNotYetValidException), "The token is not valid yet")]
        [InlineData(typeof(SecurityTokenExpiredException), "The token is expired")]
        [InlineData(typeof(SecurityTokenInvalidSignatureException), "The signature is invalid")]
        [InlineData(typeof(SecurityTokenSignatureKeyNotFoundException), "The signature key was not found")]
        public async Task ExceptionReportedInHeaderForAuthenticationFailures(Type errorType, string message)
        {
            var server = CreateServer(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new InvalidTokenValidator(errorType));
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal($"Bearer error=\"invalid_token\", error_description=\"{message}\"", response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Theory]
        [InlineData(typeof(ArgumentException))]
        public async Task ExceptionNotReportedInHeaderForOtherFailures(Type errorType)
        {
            var server = CreateServer(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new InvalidTokenValidator(errorType));
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("Bearer error=\"invalid_token\"", response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Fact]
        public async Task ExceptionsReportedInHeaderForMultipleAuthenticationFailures()
        {
            var server = CreateServer(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new InvalidTokenValidator(typeof(SecurityTokenInvalidAudienceException)));
                options.SecurityTokenValidators.Add(new InvalidTokenValidator(typeof(SecurityTokenSignatureKeyNotFoundException)));
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("Bearer error=\"invalid_token\", error_description=\"The audience is invalid; The signature key was not found\"",
                response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Theory]
        [InlineData("custom_error", "custom_description", "custom_uri")]
        [InlineData("custom_error", "custom_description", null)]
        [InlineData("custom_error", null, null)]
        [InlineData(null, "custom_description", "custom_uri")]
        [InlineData(null, "custom_description", null)]
        [InlineData(null, null, "custom_uri")]
        public async Task ExceptionsReportedInHeaderExposesUserDefinedError(string error, string description, string uri)
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.Error = error;
                        context.ErrorDescription = description;
                        context.ErrorUri = uri;

                        return Task.FromResult(0);
                    }
                };
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("", response.ResponseText);

            var builder = new StringBuilder(JwtBearerDefaults.AuthenticationScheme);

            if (!string.IsNullOrEmpty(error))
            {
                builder.Append(" error=\"");
                builder.Append(error);
                builder.Append("\"");
            }
            if (!string.IsNullOrEmpty(description))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    builder.Append(",");
                }

                builder.Append(" error_description=\"");
                builder.Append(description);
                builder.Append('\"');
            }
            if (!string.IsNullOrEmpty(uri))
            {
                if (!string.IsNullOrEmpty(error) ||
                    !string.IsNullOrEmpty(description))
                {
                    builder.Append(",");
                }

                builder.Append(" error_uri=\"");
                builder.Append(uri);
                builder.Append('\"');
            }

            Assert.Equal(builder.ToString(), response.Response.Headers.WwwAuthenticate.First().ToString());
        }

        [Fact]
        public async Task ExceptionNotReportedInHeaderWhenIncludeErrorDetailsIsFalse()
        {
            var server = CreateServer(o =>
            {
                o.IncludeErrorDetails = false;
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("Bearer", response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Fact]
        public async Task ExceptionNotReportedInHeaderWhenTokenWasMissing()
        {
            var server = CreateServer();

            var response = await SendAsync(server, "http://example.com/oauth");
            Assert.Equal(HttpStatusCode.Unauthorized, response.Response.StatusCode);
            Assert.Equal("Bearer", response.Response.Headers.WwwAuthenticate.First().ToString());
            Assert.Equal("", response.ResponseText);
        }

        [Fact]
        public async Task CustomTokenValidated()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        // Retrieve the NameIdentifier claim from the identity
                        // returned by the custom security token validator.
                        var identity = (ClaimsIdentity)context.Ticket.Principal.Identity;
                        var identifier = identity.FindFirst(ClaimTypes.NameIdentifier);

                        Assert.Equal("Bob le Tout Puissant", identifier.Value);

                        // Remove the existing NameIdentifier claim and replace it
                        // with a new one containing a different value.
                        identity.RemoveClaim(identifier);
                        // Make sure to use a different name identifier
                        // than the one defined by BlobTokenValidator.
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "Bob le Magnifique"));

                        return Task.FromResult<object>(null);
                    }
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator(JwtBearerDefaults.AuthenticationScheme));
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer someblob");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal("Bob le Magnifique", response.ResponseText);
        }

        [Fact]
        public async Task RetrievingTokenFromAlternateLocation()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = "CustomToken";
                        return Task.FromResult<object>(null);
                    }
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator("JWT", token =>
                {
                    Assert.Equal("CustomToken", token);
                }));
            });

            var response = await SendAsync(server, "http://example.com/oauth", "Bearer Token");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal("Bob le Tout Puissant", response.ResponseText);
        }

        [Fact]
        public async Task EventOnMessageReceivedSkip_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.None();
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        throw new NotImplementedException();
                    },
                    OnAuthenticationFailed = context =>
                    {
                        throw new NotImplementedException(context.Exception.ToString());
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
            });

            var response = await SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal(string.Empty, response.ResponseText);
        }

        [Fact]
        public async Task EventOnMessageReceivedReject_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Fail("Authentication was aborted from user code.");
                        context.Response.StatusCode = StatusCodes.Status202Accepted;
                        return Task.FromResult(0);
                    },
                    OnTokenValidated = context =>
                    {
                        throw new NotImplementedException();
                    },
                    OnAuthenticationFailed = context =>
                    {
                        throw new NotImplementedException(context.Exception.ToString());
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
            });

            var exception = await Assert.ThrowsAsync<Exception>(delegate
            {
                return SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            });

            Assert.Equal("Authentication was aborted from user code.", exception.InnerException.Message);
        }

        [Fact]
        public async Task EventOnTokenValidatedSkip_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        context.None();
                        return Task.FromResult(0);
                    },
                    OnAuthenticationFailed = context =>
                    {
                        throw new NotImplementedException(context.Exception.ToString());
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator("JWT"));
            });

            var response = await SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal(string.Empty, response.ResponseText);
        }

        [Fact]
        public async Task EventOnTokenValidatedReject_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        context.Fail("Authentication was aborted from user code.");
                        context.Response.StatusCode = StatusCodes.Status202Accepted;
                        return Task.FromResult(0);
                    },
                    OnAuthenticationFailed = context =>
                    {
                        throw new NotImplementedException(context.Exception.ToString());
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator("JWT"));
            });

            var exception = await Assert.ThrowsAsync<Exception>(delegate
            {
                return SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            });

            Assert.Equal("Authentication was aborted from user code.", exception.InnerException.Message);
        }

        [Fact]
        public async Task EventOnAuthenticationFailedSkip_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        throw new Exception("Test Exception");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.None();
                        return Task.FromResult(0);
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator("JWT"));
            });

            var response = await SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Equal(string.Empty, response.ResponseText);
        }

        [Fact]
        public async Task EventOnAuthenticationFailedReject_NoMoreEventsExecuted()
        {
            var server = CreateServer(options =>
            {
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        throw new Exception("Test Exception");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Fail("Authentication was aborted from user code.");
                        context.Response.StatusCode = StatusCodes.Status202Accepted;
                        return Task.FromResult(0);
                    },
                    OnChallenge = context =>
                    {
                        throw new NotImplementedException();
                    },
                };
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new BlobTokenValidator("JWT"));
            });

            var exception = await Assert.ThrowsAsync<Exception>(delegate
            {
                return SendAsync(server, "http://example.com/checkforerrors", "Bearer Token");
            });

            Assert.Equal("Authentication was aborted from user code.", exception.InnerException.Message);
        }

        [Fact]
        public async Task EventOnChallengeSkip_ResponseNotModified()
        {
            var server = CreateServer(o =>
            {
                o.Events = new JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        context.Skip();
                        return Task.FromResult(0);
                    },
                };
            });

            var response = await SendAsync(server, "http://example.com/unauthorized", "Bearer Token");
            Assert.Equal(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.Empty(response.Response.Headers.WwwAuthenticate);
            Assert.Equal(string.Empty, response.ResponseText);
        }

        class InvalidTokenValidator : ISecurityTokenValidator
        {
            public InvalidTokenValidator()
            {
                ExceptionType = typeof(SecurityTokenException);
            }

            public InvalidTokenValidator(Type exceptionType)
            {
                ExceptionType = exceptionType;
            }

            public Type ExceptionType { get; set; }

            public bool CanValidateToken => true;

            public int MaximumTokenSizeInBytes
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool CanReadToken(string securityToken) => true;

            public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
            {
                var constructor = ExceptionType.GetTypeInfo().GetConstructor(new[] { typeof(string) });
                var exception = (Exception)constructor.Invoke(new[] { ExceptionType.Name });
                throw exception;
            }
        }

        class BlobTokenValidator : ISecurityTokenValidator
        {
            private Action<string> _tokenValidator;

            public BlobTokenValidator(string authenticationScheme)
            {
                AuthenticationScheme = authenticationScheme;

            }
            public BlobTokenValidator(string authenticationScheme, Action<string> tokenValidator)
            {
                AuthenticationScheme = authenticationScheme;
                _tokenValidator = tokenValidator;
            }

            public string AuthenticationScheme { get; }

            public bool CanValidateToken => true;

            public int MaximumTokenSizeInBytes
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public bool CanReadToken(string securityToken) => true;

            public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
            {
                validatedToken = null;
                _tokenValidator?.Invoke(securityToken);

                var claims = new[]
                {
                    // Make sure to use a different name identifier
                    // than the one defined by CustomTokenValidated.
                    new Claim(ClaimTypes.NameIdentifier, "Bob le Tout Puissant"),
                    new Claim(ClaimTypes.Email, "bob@contoso.com"),
                    new Claim(ClaimsIdentity.DefaultNameClaimType, "bob"),
                };

                return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthenticationScheme));
            }
        }

        private static TestServer CreateServer(Action<JwtBearerOptions> options = null, Func<HttpContext, Func<Task>, Task> handlerBeforeAuth = null)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    if (handlerBeforeAuth != null)
                    {
                        app.Use(handlerBeforeAuth);
                    }

                    app.UseAuthentication();
                    app.Use(async (context, next) =>
                    {
                        if (context.Request.Path == new PathString("/checkforerrors"))
                        {
                            var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme); // this used to be "Automatic"
                            if (result.Failure != null)
                            {
                                throw new Exception("Failed to authenticate", result.Failure);
                            }
                            return;
                        }
                        else if (context.Request.Path == new PathString("/oauth"))
                        {
                            if (context.User == null ||
                                context.User.Identity == null ||
                                !context.User.Identity.IsAuthenticated)
                            {
                                context.Response.StatusCode = 401;
                                // REVIEW: no more automatic challenge
                                await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);
                                return;
                            }

                            var identifier = context.User.FindFirst(ClaimTypes.NameIdentifier);
                            if (identifier == null)
                            {
                                context.Response.StatusCode = 500;
                                return;
                            }

                            await context.Response.WriteAsync(identifier.Value);
                        }
                        else if (context.Request.Path == new PathString("/unauthorized"))
                        {
                            // Simulate Authorization failure 
                            var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                            await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);
                        }
                        else if (context.Request.Path == new PathString("/signIn"))
                        {
                            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SignInAsync(JwtBearerDefaults.AuthenticationScheme, new ClaimsPrincipal()));
                        }
                        else if (context.Request.Path == new PathString("/signOut"))
                        {
                            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SignOutAsync(JwtBearerDefaults.AuthenticationScheme));
                        }
                        else
                        {
                            await next();
                        }
                    });
                })
                .ConfigureServices(services => services.AddJwtBearerAuthentication(options));

            return new TestServer(builder);
        }

        // TODO: see if we can share the TestExtensions SendAsync method (only diff is auth header)
        private static async Task<Transaction> SendAsync(TestServer server, string uri, string authorizationHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                request.Headers.Add("Authorization", authorizationHeader);
            }

            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };

            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }

            return transaction;
        }
    }
}
