namespace quasitekBball.Dtos;

public record class SignPlayer(
    string Name,
    string Team,
    int Age,
    string Height,
    double Ppg,
    decimal ContractSalary
);