using System;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace Web.Host.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        //public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        //{
        //    // OAuth2 supports the notion of client authentication
        //    // this is not used here
        //    context.Validated();
        //}

        //public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        //{
        //    // validate user credentials (demo!)
        //    // user credentials should be stored securely (salted, iterated, hashed yada)
        //    if (context.UserName != context.Password)
        //    {
        //        context.Rejected();
        //        return;
        //    }

        //    // create identity
        //    var id = new ClaimsIdentity(context.Options.AuthenticationType);
        //    id.AddClaim(new Claim("sub", context.UserName));
        //    id.AddClaim(new Claim("role", "user"));

        //    context.Validated(id);
        //}
        
        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
                else if (context.ClientId == "web")
                {
                    var expectedUri = new Uri(context.Request.Uri, "/");
                    context.Validated(expectedUri.AbsoluteUri);
                }
            }

            return Task.FromResult<object>(null);
        }
    }
}