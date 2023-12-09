using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class UnitsService
    {
        private readonly IClock _clock;
        private readonly List<Unit> _units;
        public UnitsService(IClock clock)
        {
            _clock = clock;
            clock.CreateTimer(_ => LaunchAllUnitsFight(), null, TimeSpan.Zero, TimeSpan.FromSeconds(6));
        }

        private void LaunchAllUnitsFight()
        {
            foreach (var unit in _units)
            {
                ProcessAttackBis(unit);
            };
        }

        private void ProcessAttackBis(Unit unitThatAttack)
        {
            var unitTypePriority = GetNextTargetsType(unitThatAttack);
            var unitsAround = _units.Where(unit => unitThatAttack.Planet == unit.Planet && unitThatAttack.System == unit.System);
            // ajouter une cond qui vérifie que les vaisseau de la liste unitsAround n'appartient pas à l'user
            var firstTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[0]);
            var secondTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[1]);
            var thirdTarget = unitsAround.Where(unit => unit.Type == unitTypePriority[2]);
            var rightTarget = firstTarget.Count() != 0 ? firstTarget : secondTarget.Count() != 0 ? secondTarget : thirdTarget;
            var orderedRightTarget = rightTarget.OrderBy(unit => unit.Health); // jsplu si c'est ascending ou descending
            var unitToAttack = orderedRightTarget.FirstOrDefault();
            unitToAttack.Health -= GetUnitDamage(unitThatAttack, unitToAttack.Type);
        }

        private int? GetUnitDamage(Unit unitThatAttack, string? type)
       => type switch
       {
           "bomber" => 10,
           "fighter" => 20,
           "cruiser" => 30
       };

        private List<string> GetNextTargetsType(Unit unit)
            => unit.Type switch
            {
                "bomber" => new List<string> { "cruiser", "bomber", "fighter" },
                "fighter" => new List<string> { "bomber", "fighter", "cruiser" },
                "cruiser" => new List<string> { "fighter", "bomber", "cruiser" },
            };

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
                    if (unit.task != null)
                        await unit.task;
                }
                else
                    unit.Planet = null;
            }
        }
    } 
}
