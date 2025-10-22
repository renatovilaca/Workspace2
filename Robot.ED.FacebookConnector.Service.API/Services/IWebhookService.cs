using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public interface IWebhookService
{
    Task ForwardResultAsync(RpaResultDto result);
}
