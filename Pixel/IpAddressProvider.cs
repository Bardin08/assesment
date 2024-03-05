namespace Pixel;

public class IpAddressProvider
{
    public string GetUserIp(HttpRequest request)
    {
        var requestIpAddress = request.Headers[HeaderNames.ForwardedFrom].FirstOrDefault();
        if (string.IsNullOrEmpty(requestIpAddress))
        {
            requestIpAddress = request.HttpContext.Connection.RemoteIpAddress!.ToString();
        }

        return requestIpAddress;
    }    
}