namespace quasitekBball.Dtos;

public record class PlayerDto{
    public int Id { get; init; }
    public string Name  { get; init; }
    public string Team { get; init; }
    public int Age  { get; init; }
    public string Height  { get; init; }
    public double Ppg  { get; init; }
    public string ContractSalary { get; set; }

    public PlayerDto(int id, string name, string team, int age, string height, double ppg, decimal salary)
    {
        Id = id;
        Name = name;
        Team = team;
        Age = age;
        Height = height;
        Ppg = ppg;
        ContractSalary=salary.ToString("C0");
    }
}
