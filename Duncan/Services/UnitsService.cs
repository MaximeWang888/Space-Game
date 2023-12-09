using Duncan.Model;
using Duncan.Repositories;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class UnitsService
    {
        private readonly IClock _clock;
        private readonly List<Unit> _units;
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

        public void LaunchTimer()
        {
            _clock.CreateTimer(_ => LaunchAllUnitsFight(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        private void LaunchAllUnitsFight()
        {
            foreach (var unit in _units)
            {
                if (IsCombatUnit(unit.Type))
                {
                    ProcessAttackBis(unit);
                }
                // warUnit.Fight(_clock, GetEnemyOnLocation);
            };

            foreach (var unit in _units)
            {
                if (unit.Health <= 0)
                    _usersRepo.GetUserWithUnitId(unit.Id).Units.Remove(unit);
            };
        }

        private void ProcessAttackBis(Unit unitThatAttack)

        { 
            User userOwner = _usersRepo.GetUserWithUnitId(unitThatAttack.Id);
            List<Unit> enemies = GetEnemy(userOwner); // User different de warunit;
            List<Unit> enemiesAtSameLocation = GetEnemiesAtSameLocation(enemies, userOwner);
            List<Unit> enemiesOrderedByPriorityThenByHealth = GetPriorityByTypeThenByHealth(unitThatAttack.Type)
                ?.SelectMany(priorityList => enemiesAtSameLocation.Where(unit => priorityList.Contains(unit.Type)))
                .OrderBy(unit => unit.Health)
                .ToList();
            Unit targetUnit = enemiesOrderedByPriorityThenByHealth.FirstOrDefault();
            if (targetUnit is null) return;
            targetUnit.Health -= GetUnitDamage(unitThatAttack.Type, targetUnit.Type); 


            //var unitTypePriority = GetNextTargetsType(unitThatAttack);
            //var unitsAround = _units.Where(unit => unitThatAttack.Planet == unit.Planet && unitThatAttack.System == unit.System);
            //// ajouter une cond qui vérifie que les vaisseau de la liste unitsAround n'appartient pas à l'user
            //var firstTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[0]);
            //var secondTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[1]);
            //var thirdTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[2]);
            //var rightTarget = firstTarget.Count() != 0 ? firstTarget : secondTarget.Count() != 0 ? secondTarget : thirdTarget;
            //var orderedRightTarget = rightTarget.OrderBy(unit => unit.Health); // jsplu si c'est ascending ou descending
            //var unitToAttack = orderedRightTarget.FirstOrDefault();
            //unitToAttack.Health -= GetUnitDamage(unitThatAttack, unitToAttack.Type);
        }

        // Sur les vaisseaux du lieu
        // Même planète d’un système si sur planète
        // Même système si hors planète
        private List<Unit> GetEnemiesAtSameLocation(List<Unit> enemies, User userOwner)
        {
            if (userOwner != null)
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
            return null;
        }
        private List<Unit> GetEnemy(User userOwner)
        {
            if (userOwner != null)
                return _units
                    .Where(unit => !userOwner.Units.Any(userUnit => userUnit.Equals(unit)))
                    .Where(unit => unit.Type != "scout" && unit.Type != "builder")
                    .ToList();
            return null;
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
                    if (unit.task != null)
                        await unit.task;
                }
                else
                    unit.Planet = null;
            }
        }

        private List<Unit>? GetAllUnits()
        {
            List<User> users = _usersRepo.GetUsers();
            return users?.SelectMany(u => u.Units).ToList();
        }
        private List<string>? GetPriorityByTypeThenByHealth(string type) => type switch
        {
            "bomber" => new List<string> { "cruiser", "bomber", "fighter" },
            "fighter" => new List<string> { "bomber", "fighter", "cruiser" },
            "cruiser" => new List<string> { "fighter", "bomber", "cruiser" },
            "builder" => new List<string> { },
            "scout" => new List<string> { },
        };
        private bool IsCombatUnit(string unitType)
        {
            return unitType == "cruiser" || unitType == "bomber" || unitType == "fighter";
        }
        private int? GetUnitDamage(string? unitTypeThatAttack, string? targetUnitType) =>
            (unitTypeThatAttack, targetUnitType) switch
            {
                ("bomber", "bomber") => 400,
                ("bomber", _) => 40,
                ("fighter", _) => 10,
                ("cruiser", _) => 40,
                _ => null
            };


    }
}
