namespace OrderService.Application.Services;

public interface IHttpClientUtils
{
    Task SendPostRequest(string url, object payload);
}