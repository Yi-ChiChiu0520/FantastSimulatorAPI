namespace quasitekBball.Dtos;

public record class TradePlayer(
    int Id,
    string Name,
    string Team,
    int Age,
    string Height,
    double Ppg,
    decimal ContractSalary
);
