namespace API.DTOs.Agent;

public record UpdateAgentDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string LicenseNumber)
{
    public Models.Agent ToModel() => new Models.Agent
    {
        FirstName = FirstName,
        LastName = LastName,
        Email = Email,
        Phone = Phone,
        LicenseNumber = LicenseNumber
    };
}
