using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security.Infrastructure;
using WebAPI_Test.Domain;
using WebAPI_Test.EntityFramework;

namespace WebAPI_Test.Providers
{
    public class hPayRefreshTokenProvider : IAuthenticationTokenProvider
    {

        private hPayDomain _hPayDomain;
        public hPayRefreshTokenProvider()
        {
            _hPayDomain = new hPayDomain();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            
            var clientId = context.Ticket.Properties.Dictionary[Constants.ClientId];

            if (string.IsNullOrEmpty(clientId))
            {
                return;
            }

            //Generate refresh token
            var refreshTokenId = Guid.NewGuid().ToString("n");

            //set refresh token life time from owin context which was set in CxcAuthorizationServerProvider
            var refreshTokenLifeTime = context.OwinContext.Get<string>(Constants.ClientRefreshTokenLifeTime);

            var token = new RefreshToken()
            {
                Id = refreshTokenId,
                ClientId = clientId,
                Subject = context.Ticket.Identity.Name,
                IssuedUtc = DateTime.Now,
                ExpiresUtc = DateTime.Now.AddMinutes(Convert.ToDouble(refreshTokenLifeTime))
            };

            context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
            context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;

            //Protected Ticket column contains signed string which contains a serialized representation for the ticket for specific user
            //In other words it contains all the claims and ticket properties for this user. 
            //The Owin middle-ware will use this string to build the new access token auto-magically
            token.ProtectedTicket = context.SerializeTicket();

            var result = _hPayDomain.AddRefreshToken(token);

            if (result)
            {
                //Set refresh token information into AuthenticationTicket
                context.SetToken(refreshTokenId);
            }

        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            Receive(context);
        }



        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            var refreshToken = _hPayDomain.GetRefreshToken(context.Token);

            if (refreshToken == null)
            {
                // Return user with status code 400, if refresh token is empty
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                context.Response.ReasonPhrase = "Please login again to gain application access.";
                return;
            }
            else
            {
                //AuthenticationTokenReceiveContext DeserializeTicket data and assign back to AuthenticationTicket
                //Protected Ticket column which contains signed string which contains a serialized representation for the ticket for specific user
                //In other words it contains all the claims and ticket properties for this user. 
                context.DeserializeTicket(refreshToken.ProtectedTicket);
                var result = _hPayDomain.DeleteRefreshTokenByRefreshTokenId(context.Token);

            }


        }
    }

}