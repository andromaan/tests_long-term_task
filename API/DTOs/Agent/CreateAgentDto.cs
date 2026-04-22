namespace API.DTOs.Agent;

public record CreateAgentDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string LicenseNumber)
{
    public static CreateAgentDto FromModel(Models.Agent agent)
        => new(agent.FirstName, agent.LastName,
            agent.Email, agent.Phone, agent.LicenseNumber);

    public Models.Agent ToModel() => new Models.Agent
    {
        FirstName = FirstName,
        LastName = LastName,
        Email = Email,
        Phone = Phone,
        LicenseNumber = LicenseNumber
    };
}