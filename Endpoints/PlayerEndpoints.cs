using quasitekBball.Dtos;

namespace quasitekBball.Endpoints;

public static class PlayerEndpoints
{
    const string Endpoint="getName";
    const string TeamName="Los Angeles Lakers";
    const int playerMinAmount=5;
    private static readonly decimal StartingSalaryCap = 205;

    private static decimal CurrentSalaryCapSpace = StartingSalaryCap;
    private static readonly List<TeamDto> teamInfo =
    [   
        new(TeamName,StartingSalaryCap,CurrentSalaryCapSpace,0),
    ];

    private static readonly List<PlayerDto> players=
    [
        
    ];  

    public static RouteGroupBuilder MapPlayerEndpoints (this WebApplication app){
    
        var group=app.MapGroup("players");

        // View current salary cap
        group.MapGet("/view/team-info", () => teamInfo);

        // View all players
        group.MapGet("/view", () => players);

        // View single player at id
        group.MapGet("/view/{id}", (int id) => players.Find(player=>player.Id==id)).WithName(Endpoint);

        // Sign player
        group.MapPost("/sign", (SignPlayer signPlayer) => {

            if (CurrentSalaryCapSpace < signPlayer.ContractSalary)
            {
                return Results.BadRequest($"Not enough salary cap space. Your current cap space is {CurrentSalaryCapSpace:C0}. {signPlayer.Name} is asking for {signPlayer.ContractSalary:C0}.");
            }

            PlayerDto player=new (
                players.Count+1,
                signPlayer.Name,
                TeamName,
                signPlayer.Age,
                signPlayer.Height,
                signPlayer.Ppg,
                signPlayer.ContractSalary
            );

            players.Add(player);

            decimal previousSalaryCapSpace = CurrentSalaryCapSpace;

            CurrentSalaryCapSpace -= signPlayer.ContractSalary;

            // Update team info
            teamInfo[0] = new TeamDto(TeamName, StartingSalaryCap, CurrentSalaryCapSpace,players.Count);
            if(players.Count==1){
                return Results.Accepted($"You signed {signPlayer.Name}(ID {players.Count}) with {signPlayer.ContractSalary:C0}. Your salary cap space decreased from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0}. Your team went from {players.Count-1} players to {players.Count} player");
            }else if (players.Count==2){
                return Results.Accepted($"You signed {signPlayer.Name}(ID {players.Count}) with {signPlayer.ContractSalary:C0}. Your salary cap space decreased from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0}. Your team went from {players.Count-1} player to {players.Count} players");
            }else{
                return Results.Accepted($"You signed {signPlayer.Name}(ID {players.Count}) with {signPlayer.ContractSalary:C0}. Your salary cap space decreased from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0}. Your team went from {players.Count-1} players to {players.Count} players");
            }
        });

        // Single Trade
        group.MapPut("/trade/{id}", (int id, TradePlayer trade)=>{
            var player = players.FirstOrDefault(player => player.Id == id);

            if (player == null)
            {
                return Results.NotFound($"Player with ID {id} not found.");
            }

            var index=players.FindIndex(player=>player.Id==id);
            
            decimal previousSalaryCapSpace = CurrentSalaryCapSpace;

            string formattedSalary = player.ContractSalary; 

            formattedSalary = formattedSalary.Substring(1);

            formattedSalary = formattedSalary.Replace(",", "");

            decimal salaryValue;
            if (decimal.TryParse(formattedSalary, out salaryValue))
            {
                // Update the salary cap space
                CurrentSalaryCapSpace += salaryValue;
            }

            players[index]=new PlayerDto(
                id,
                trade.Name,
                TeamName,
                trade.Age,
                trade.Height,
                trade.Ppg,
                trade.ContractSalary
            );    
            if(CurrentSalaryCapSpace<trade.ContractSalary){
                return Results.BadRequest($"Not enough salary cap space. You need more than {previousSalaryCapSpace:C0} because {player.Name}'s salary is {player.ContractSalary:C0}, so your current salary cap becomes {CurrentSalaryCapSpace:C0}, but {trade.Name} is asking for {trade.ContractSalary:C0}");
            }

            CurrentSalaryCapSpace-=trade.ContractSalary;
            teamInfo[0] = new TeamDto(TeamName, StartingSalaryCap, CurrentSalaryCapSpace,players.Count);
            // Trade the player
            return Results.Accepted($"You traded {player.Name} for {trade.Name}. Salary cap space went from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0} because {player.Name}'s salary was {player.ContractSalary} and {trade.Name} asked for {trade.ContractSalary:C0}");
        });

        // Trade Package
        group.MapPut("/trade/{idOne}+{idTwo}", (int idOne, int idTwo, TradePlayer trade)=>{
            // traded player should replace player with smaller index
            if (CurrentSalaryCapSpace < trade.ContractSalary)
            {
                return Results.BadRequest($"Not enough salary cap space. Your current cap space is {CurrentSalaryCapSpace:C0}");
            }

            if(players.Count<=playerMinAmount){
                return Results.BadRequest($"You need to have {playerMinAmount} plus players for a trade package.");
            }
            if(idOne>idTwo){
                int temp=idOne;
                idOne=idTwo;
                idTwo=temp;
            }

            // Find playerOne 
            var playerOne = players.FirstOrDefault(player => player.Id == idOne);
            if (playerOne == null)
            {
                return Results.NotFound($"Player with ID {idOne} not found.");
            }
            
            // Find playerTwo
            var playerTwo = players.FirstOrDefault(player => player.Id == idTwo);
            if (playerTwo == null)
            {
                return Results.NotFound($"Player with ID {idTwo} not found.");
            }
            decimal previousSalaryCapSpace = CurrentSalaryCapSpace;

            string formattedSalaryOne = playerOne.ContractSalary; 

            formattedSalaryOne = formattedSalaryOne.Substring(1);

            formattedSalaryOne = formattedSalaryOne.Replace(",", "");

            decimal salaryValueOne;
            if (decimal.TryParse(formattedSalaryOne, out salaryValueOne))
            {
                // Update the salary cap space
                CurrentSalaryCapSpace += salaryValueOne;

                string formattedSalaryTwo = playerTwo.ContractSalary; 

                formattedSalaryTwo = formattedSalaryTwo.Substring(1);

                formattedSalaryTwo = formattedSalaryTwo.Replace(",", "");

                decimal salaryValueTwo;
                if (decimal.TryParse(formattedSalaryTwo, out salaryValueTwo)){
                    CurrentSalaryCapSpace += salaryValueTwo;
                }
            }

            players.Remove(playerTwo); // Release player two

            var index=players.FindIndex(player=>player.Id==idOne); // Replace player one with trade
            players[index]=new PlayerDto(
                idOne,
                trade.Name,
                TeamName,
                trade.Age,
                trade.Height,
                trade.Ppg,
                trade.ContractSalary
            );    
            if(CurrentSalaryCapSpace<trade.ContractSalary){
                return Results.BadRequest($"Not enough salary cap space. You need more than {previousSalaryCapSpace:C0} because {playerOne.Name}'s salary is {playerOne.ContractSalary:C0} and {playerTwo.Name}'s salary is {playerTwo.ContractSalary}, so your current salary cap becomes {CurrentSalaryCapSpace:C0}, but {trade.Name} is asking for {trade.ContractSalary:C0}");
            }

            CurrentSalaryCapSpace-=trade.ContractSalary;
            teamInfo[0] = new TeamDto(TeamName, StartingSalaryCap, CurrentSalaryCapSpace,players.Count);

            // Trade the player
            return Results.Accepted($"You traded {playerOne.Name} and {playerTwo.Name} for {trade.Name}. Salary cap space went from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0} because {playerOne.Name}'s salary was {playerOne.ContractSalary}, {playerTwo.Name}'s salary was {playerTwo.ContractSalary}, and {trade.Name} asked for {trade.ContractSalary:C0}. Your team went from ");
        });

        // Release player
        group.MapDelete("/release/{id}", (int id)=>{

            var player = players.FirstOrDefault(player => player.Id == id);

            if (player == null)
            {
                return Results.NotFound($"Player with ID {id} not found.");
            }
            
            if(players.Count<=playerMinAmount){
                if(players.Count==1){
                    return Results.BadRequest($"Your team needs to have a minimum of {playerMinAmount} players. You currently have {players.Count} player.");
                }
                return Results.BadRequest($"Your team needs to have a minimum of {playerMinAmount} players. You currently have {players.Count} players.");
            }

            decimal previousSalaryCapSpace = CurrentSalaryCapSpace;

            string formattedSalary = player.ContractSalary; 

            formattedSalary = formattedSalary.Substring(1);

            formattedSalary = formattedSalary.Replace(",", "");

            decimal salaryValue;
            if (decimal.TryParse(formattedSalary, out salaryValue))
            {
                // Update the salary cap space
                CurrentSalaryCapSpace += salaryValue;
            }

            teamInfo[0] = new TeamDto(TeamName, StartingSalaryCap, CurrentSalaryCapSpace,players.Count-1);

            // Remove the player
            players.Remove(player);

            // Return success message
            return Results.Accepted($"You released {player.Name}. Salary cap space increase from {previousSalaryCapSpace:C0} to {CurrentSalaryCapSpace:C0}. Your team went from {players.Count+1} players to {players.Count} players");
        });

        return group;
    }
}
