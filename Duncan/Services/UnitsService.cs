using Duncan.Model;
using Duncan.Repositories;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class UnitsService
    {
        private readonly IClock _clock;
        private List<Unit> _units;
        private readonly UsersRepo _usersRepo;

        public UnitsService(IClock clock, UsersRepo usersRepo)
        {
            _clock = clock;
            _usersRepo = usersRepo;
            _units = GetAllUnits();
        }
        public async Task WaitingUnit(Unit unitToMove, Unit currentUnit)
        {
            currentUnit.DestinationPlanet = unitToMove.DestinationPlanet;
            currentUnit.DestinationSystem = unitToMove.DestinationSystem;

            if (unitToMove.DestinationSystem == unitToMove.System)
            {
                if (unitToMove.DestinationPlanet != null)
                    await Waiting(currentUnit, 15);
                else
                    currentUnit.EstimatedTimeOfArrival = _clock.Now;

                currentUnit.Planet = unitToMove.DestinationPlanet;
            }
            else if (unitToMove.DestinationSystem != currentUnit.System)
            {
                await Waiting(currentUnit, 60);
                currentUnit.System = unitToMove.DestinationSystem;

                if (unitToMove.DestinationPlanet != null)
                    await Waiting(currentUnit, 15);

                currentUnit.Planet = unitToMove.DestinationPlanet;
            }
        }

        private async Task Waiting(Unit currentUnit, int time)
        {
            currentUnit.EstimatedTimeOfArrival = _clock.Now.AddSeconds(time);
            await _clock.Delay(time * 1000);
        }

        public async Task VerifyTimeDifference(Unit unit)
        {
            if (unit.EstimatedTimeOfArrival.HasValue)
            {
                var timeOfArrival = (unit.EstimatedTimeOfArrival.Value - _clock.Now).TotalSeconds;

                if (timeOfArrival <= 2)
                {
                    if (unit.Task != null)
                        await unit.Task;
                }
                else
                    unit.Planet = null;
            }
        }

        public void LaunchAllUnitsFight()
        {
            _units = GetAllUnits();
            foreach (Unit unit in _units)
            {
                if (IsCombatUnit(unit.Type))
                {
                    ProcessAttack(unit);
                }
            };

            foreach (Unit unit in _units)
            {
                if (unit.Health <= 0)
                    _usersRepo?.GetUserWithUnitId(unit.Id)?.Units?.Remove(unit);
            };
        }

        private void ProcessAttack(Unit unitThatAttack)
        { 
            User? userOwner = _usersRepo.GetUserWithUnitId(unitThatAttack.Id);
            List<Unit> enemies = userOwner is not null ? GetEnemies(userOwner) : new List<Unit>() ;
            List<Unit> enemiesAtSameLocation = GetEnemiesAtSameLocation(enemies, userOwner);
            List<Unit>? enemiesOrderedByPriorityThenByHealth = GetPriorityByType(unitThatAttack.Type)
                ?.SelectMany(priorityList => enemiesAtSameLocation.Where(unit => priorityList.Contains(unit.Type)))
                .ToList();
            Unit? targetUnit = enemiesOrderedByPriorityThenByHealth?.FirstOrDefault();

            if (targetUnit is null) return;

            targetUnit.Health -= GetUnitDamage(unitThatAttack.Type, targetUnit.Type);
        }
        private static List<Unit> GetEnemiesAtSameLocation(List<Unit> enemies, User userOwner)
        {
            return enemies
            .Where(unitEnemie =>
                userOwner.Units.Any(unit =>
                    (unit.Planet.Equals(unitEnemie.Planet) && unit.System.Equals(unitEnemie.System)) ||
                    (!unit.Planet.Equals(unitEnemie.Planet) && unit.System.Equals(unitEnemie.System))
                )
            )
            .ToList();
        }
        private List<Unit> GetEnemies(User userOwner)
        {
            return _units
                .Where(unit => userOwner.Units is not null && !userOwner.Units.Any(userUnit => userUnit.Equals(unit)))
                .Where(unit => unit.Type != "scout" && unit.Type != "builder")
                .ToList();
        }
        private List<Unit> GetAllUnits()
        {
            List<User>? users = _usersRepo.GetUsers();

            return (users ?? new List<User>())
                .Where(u => u.Units != null) 
                .SelectMany(u => u.Units ?? Enumerable.Empty<Unit>())
                .ToList();
        }
        private static List<string>? GetPriorityByType(string type) => type switch
        {
            "bomber" => new List<string> { "cruiser", "bomber", "fighter" },
            "fighter" => new List<string> { "bomber", "fighter", "cruiser" },
            "cruiser" => new List<string> { "fighter", "cruiser", "bomber" },
            _ => null
        };
        private static bool IsCombatUnit(string unitType)
        {
            return unitType == "cruiser" || unitType == "bomber" || unitType == "fighter";
        }
        private int? GetUnitDamage(string? unitTypeThatAttack, string? targetUnitType)
        {
            if (unitTypeThatAttack == "bomber" && _clock.Now.Second % 60 == 0)
            {
                return 400;
            }

            return (unitTypeThatAttack, targetUnitType) switch
            {
                ("bomber", "cruiser") => 0,
                ("bomber", "fighter") => 0,
                ("bomber", "bomber") => 0,
                ("fighter", _) => 10,
                ("cruiser", "bomber") => 4,
                ("cruiser", _) => 40,
                _ => null
            };
        }
    }
}
