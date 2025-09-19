using FastEndpoints;
using Microsoft.AspNetCore.Http;
using MasLazu.AspNet.Framework.Endpoint.EndpointGroups;

namespace MasLazu.AspNet.Authentication.Password.Endpoint.EndpointGroups;

public class AuthEndpointGroup : SubGroup<V1EndpointGroup>
{
    public AuthEndpointGroup()
    {
        Configure("auth", ep => ep.Description(x => x.WithTags("Auth")));
    }
}
