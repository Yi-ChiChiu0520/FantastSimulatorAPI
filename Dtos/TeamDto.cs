namespace quasitekBball.Dtos;

public record class TeamDto
{
    public string TeamName { get; init; }
    public string StartingSalaryCap { get; init; }
    public string CurrentSalaryCapSpace { get; init; }

    public int PlayerAmount {get; set;}

    public TeamDto(string teamName, decimal salaryCap, decimal salaryCapSpace, int playerAmount)
    {
        TeamName = teamName;
        StartingSalaryCap = salaryCap.ToString("C0"); // Formats the salary as currency with no decimal places
        CurrentSalaryCapSpace = salaryCapSpace.ToString("C0"); // Formats the salary as currency with no decimal places
        PlayerAmount = playerAmount;
    }
}
