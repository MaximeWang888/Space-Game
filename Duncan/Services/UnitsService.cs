using Duncan.Model;
using Shard.Shared.Core;

namespace Duncan.Services
{
    public class UnitsService
    {
        private readonly IClock _clock;
        private static User globalUser = new User();
        public UnitsService(IClock clock) {
            this._clock = clock;
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
                    if (unit.task != null)
                        await unit.task;
                }
                else
                    unit.Planet = null;
            }
        }

        public void RunTaskOnUnit(Unit unit, User user)
        {
            unit.task = ProcessAttack(unit, user);
        }

        public async Task ProcessAttack(Unit unit, User user)
        {
            globalUser.FightingUnits?.Add(unit);
            if (globalUser.FightingUnits?.Count == 2 ) {
                await _clock.Delay(6_000); // 6 secondes

                Unit unit1 = globalUser.FightingUnits[0];
                Unit unit2 = globalUser.FightingUnits[1];

                switch (unit1.Type)
                {
                    case "bomber":
                        switch (unit2.Type)
                        {
                            case "bomber":
                                break;
                            case "fighter":
                                break;
                            case "cruiser":
                                break;
                        }
                        break;
                    case "fighter":
                        switch (unit2.Type)
                        {
                            case "bomber":
                                unit1.Health -= 10;
                                break;
                            case "fighter":
                                unit1.Health -= 70;
                                unit2.Health -= 70;
                                break;
                            case "cruiser":
                                unit1.Health -= 10;
                                break;
                        }
                        break;
                    case "cruiser":
                        switch (unit2.Type)
                        {
                            case "bomber":
                                unit1.Health -= 4;
                                break;
                            case "fighter":
                                unit1.Health -= 40;
                                break;
                            case "cruiser":
                                break;
                        }
                        break;
                }

                globalUser.FightingUnits = null;
            }
            //unit.Health = 0;
        }
    }
}
