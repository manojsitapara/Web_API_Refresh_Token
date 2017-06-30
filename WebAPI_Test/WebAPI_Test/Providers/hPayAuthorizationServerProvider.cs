using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using WebAPI_Test.Domain;
using WebAPI_Test.EntityFramework;

namespace WebAPI_Test.Providers
{
    public class hPayAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        private readonly hPayDomain _hPayDomain;
        private string _userFullName;
        private string _emailId;

        public hPayAuthorizationServerProvider()
        {
            _hPayDomain = new hPayDomain();
        }



        // ValidateClientAuthentication method is used for validating client app. Meaning, only clients which hold the secret can start an authenticaon flow. 

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;

            context.TryGetFormCredentials(out clientId, out clientSecret);


            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "ClientId should be sent.");
            }

            Client client = _hPayDomain.GetClient(context.ClientId);

            if (client != null)
            {
                if (client.Secret != clientSecret)
                {
                    context.SetError("invalid_clientSecret", "Client secret is invalid.");
                }

                if (!client.Active)
                {
                    context.SetError("invalid_clientId", "Client is inactive.");
                }

                context.OwinContext.Set<string>(Constants.ClientRefreshTokenLifeTime, client.RefreshTokenLifeTime.ToString());

                context.Validated();
            }
            else
            {
                context.SetError("invalid_clientId", $"Client '{context.ClientId}' is not registered in the system.");
            }

        }


 

        //GrantResourceOwnerCredentials() gets Call when a request to the Token endpoint arrives with a "grant_type" of "password"
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            User hPayUser = _hPayDomain.AuthenticateUser(context.UserName, context.Password);

            _userFullName = hPayUser.UserFirstName + " " + hPayUser.UserLastName;
            _emailId = hPayUser.EmailId;

            if (hPayUser !=  null)
            {
                IList<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, "provider"),
                    new Claim(ClaimTypes.Name, _userFullName),
                    new Claim(Constants.Username, _emailId)
                };

                //Claims will be encrypted in token, so they will only be accessed from resource(i.e. client) server
                ClaimsIdentity identity = new ClaimsIdentity(claims, context.Options.AuthenticationType);

                IDictionary<string, string> authenticationPropertiesDictionary = new Dictionary<string, string>();
                authenticationPropertiesDictionary.Add(Constants.ClientId, context.ClientId ?? string.Empty);
                authenticationPropertiesDictionary.Add(Constants.Username, context.UserName);
                authenticationPropertiesDictionary.Add(Constants.UserFullName,
                    hPayUser.UserFirstName + " " + hPayUser.UserLastName);

                //Adds authentication properties, if you want your client to be able to read extended properties
                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertiesDictionary);

                AuthenticationTicket ticket = new AuthenticationTicket(identity, authenticationProperties);

                //The token generation happens behind the scenes when we call  "context.Validated(ticket);"
                context.Validated(ticket);
                _hPayDomain.UpdateUserLoginInFlag(context.UserName, true);
            }
            else
            {
                context.SetError("invalid_grant", "The user name or password is incorrect.");
            }

        }




        //GrantRefreshToken() gets Call when a request to the Token endpoint arrives with a "grant_type" of "refresh_token"

        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary[Constants.ClientId];
            var userName = context.Ticket.Properties.Dictionary[Constants.Username];
            var currentClient = context.ClientId;

            User hPayUser = _hPayDomain.GetUser(userName);

            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);
            var newClaim = newIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (newClaim != null)
            {
                newIdentity.RemoveClaim(newClaim);
            }

            newIdentity.AddClaim(new Claim(ClaimTypes.Name, hPayUser.UserFirstName + " " + hPayUser.UserLastName));

            if (originalClient != currentClient)
            {
                context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
            }

            context.Validated(newIdentity);

        }




        // Add additional parameter to return with response
        public override async Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            context.AdditionalResponseParameters.Add("TestParam1", "Value1");
            context.AdditionalResponseParameters.Add("TestParam2", "Value2");
        }


     
    }






}