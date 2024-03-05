using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Pixel.Tests;

[TestSubject(typeof(IpAddressProvider))]
public class IpAddressProviderTest
{
    private readonly IpAddressProvider _sut = new();

    [Fact]
    public void GetUserIp_WithForwardedForHeader_ReturnsCorrectIpAddress()
    {
        // Arrange
        const string expectedIpAddress = "192.168.1.1";
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-For", expectedIpAddress }
        };
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(r => r.Headers).Returns(headers);

        // Act
        var actualIpAddress = _sut.GetUserIp(requestMock.Object);

        // Assert
        Assert.Equal(expectedIpAddress, actualIpAddress);
    }

    [Fact]
    public void GetUserIp_WithoutForwardedForHeader_ReturnsRemoteIpAddress()
    {
        const string expectedIpAddress = "10.0.0.1";
        var connectionMock = new Mock<ConnectionInfo>();
        connectionMock.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse(expectedIpAddress));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(c => c.Connection).Returns(connectionMock.Object);

        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(r => r.HttpContext).Returns(httpContextMock.Object);
        requestMock.SetupGet(r => r.Headers).Returns(new HeaderDictionary());

        var actualIpAddress = _sut.GetUserIp(requestMock.Object);

        Assert.Equal(expectedIpAddress, actualIpAddress);
    }
}