namespace API.DTOs.Agent;

public record AgentDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string LicenseNumber)

{
    public static AgentDto FromModel(Models.Agent agent) => new(
        agent.Id,
        agent.FirstName,
        agent.LastName,
        agent.Email,
        agent.Phone,
        agent.LicenseNumber);
}