﻿using Duncan.Model;
using Duncan.Repositories;
using Duncan.Services;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Shard.Shared.Core;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.InteropServices;

namespace Duncan.Controllers
{
    public class UnitsController : ControllerBase
    {
        private readonly MapGeneratorWrapper _map;
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;
        private readonly SystemsRepo _systemsRepo;
        private readonly PlanetRepo _planetRepo;
        private readonly UnitsService _unitsService;

        public UnitsController(MapGeneratorWrapper mapGenerator, UsersRepo usersRepo, UnitsRepo unitsRepo, UnitsService unitsService, SystemsRepo systemsRepo, PlanetRepo planetRepo)
        {
            _map = mapGenerator;
            _unitsRepo = unitsRepo;
            _usersRepo = usersRepo;
            _systemsRepo = systemsRepo;
            _planetRepo = planetRepo;
            _unitsService = unitsService;
        }

        [SwaggerOperation(Summary = "Get unit of a specific user")]
        [HttpGet("users/{userId}/Units")]
        public ActionResult<List<Unit>> GetAllUnit(string userId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (user == null) 
                return NotFound("Not Found user");

            List<Unit> units = user.Units ?? new List<Unit>();

            return units;
        }

        [SwaggerOperation(Summary = "Move Unit By Id")]
        [HttpPut("users/{userId}/units/{unitId}")]
        public async Task<ActionResult<Unit?>> MoveUnitByIdAsync(string userId, string unitId, [FromBody] Unit unitBody)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);

            bool isAdmin = User.IsInRole("admin");

            bool isFakeRemoteUser = User.IsInRole("shard");

            if(unitFound != null) {
                unitFound.Task = _unitsService.WaitingUnit(unitFound, unitBody);
            }

            if (unitFound == null && !isAdmin && !isFakeRemoteUser && unitBody.Type == "cargo") 
            {
                user.Units.Add(unitBody);
                return unitBody; 
            }

            else if (unitFound == null && !isAdmin && !isFakeRemoteUser)
                return Unauthorized("Unauthorized");

            else if (isAdmin)
            {
                user.Units.Add(unitBody);
                unitBody.DestinationPlanet = unitBody.Planet;
                unitBody.DestinationSystem = unitBody.System;
                unitBody.Health = unitBody.Type switch
                {
                    "bomber" => 50,
                    "fighter" => 80,
                    "cruiser" => 400,
                    _ => unitBody.Health // Default case, keep the existing health if the unit type is not recognized
                };
                return unitBody;
            }
            else if (isFakeRemoteUser)
            {
                unitBody.System = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6";
                user.Units.Add(unitBody);
                return unitBody;
            }

            if(_unitsService.NeedToLoadOrUnloadResources(unitFound, unitBody))
            {
                if (unitFound.Type == "cargo")
                {
                    var buildingOnPlanet = user.Buildings?.Where(b => b.Planet == unitFound.Planet);
                    var isStarportOnPlanet = buildingOnPlanet.Any(b => b.Type == "starport");
                    if (isStarportOnPlanet is false) 
                        return BadRequest("First One");

                    foreach (var resource in unitBody.ResourcesQuantity.Keys.ToList())
                    {
                        int bodyQuantity = unitBody.ResourcesQuantity[resource];
                        int cargoQuantity = unitFound.ResourcesQuantity[resource];
                        int diff = bodyQuantity - cargoQuantity;

                        if (diff > 0)
                        {
                            unitFound.ResourcesQuantity[resource] +=  diff;
                            user.ResourcesQuantity[resource] -= diff;
                        }
                        else if (diff < 0)
                        {
                            user.ResourcesQuantity[resource] -= diff;
                            unitFound.ResourcesQuantity[resource] += diff;
                        }
                    }
                }
                else return BadRequest("Second one");
            }

            if (user.ResourcesQuantity.Any(kv => kv.Value < 0 ))
            {
                return BadRequest("Third one");
            }


            var building = user.Buildings.FirstOrDefault(b => b.BuilderId == unitBody.Id);

            if (building != null &&
                ((unitBody.DestinationSystem == unitBody.System && unitBody.DestinationPlanet != unitBody.Planet) ||
                 (unitBody.DestinationSystem != unitBody.System && unitBody.DestinationPlanet == unitBody.Planet)))
            {
                building.CancellationSource.Cancel();
                user.Buildings.Remove(user.Buildings.FirstOrDefault(b => b.BuilderId == unitBody.Id));
            }

            return unitFound;
        }

        [SwaggerOperation(Summary = "Return information about one single unit of a user")]
        [HttpGet("users/{userId}/Units/{unitId}")]
        public async Task<ActionResult<Unit>> GetUnitInformation(string userId, string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);
            if (unitFound == null)
                return NotFound("Not Found unitFound");

            await _unitsService.VerifyTimeDifference(unitFound);

            return unitFound;
        }

        [SwaggerOperation(Summary = "Get unit location by user ID and unit ID")]
        [HttpGet("users/{userId}/Units/{unitId}/location")]
        public ActionResult<UnitLocation> GetUnitLocation(string userId, string unitId)
        {
            User? user = _usersRepo.GetUserWithUnitsByUserId(userId);
            if (user == null)
                return NotFound("Not Found userWithUnits");

            Unit? unitFound = _unitsRepo.GetUnitByUnitId(unitId, user);
            if (unitFound == null)
                return NotFound("Not Found unitFound");

            UnitLocation unitInformation = new UnitLocation();
            unitInformation.System = unitFound.System;
            unitInformation.Planet = unitFound.Planet;

            SystemSpecification? system = _systemsRepo.GetSystemByName(unitFound.System, _map.Map.Systems);
            if (system == null)
                return NotFound("Not Found system");

            PlanetSpecification? planet = _planetRepo.GetPlanetByName(unitFound.Planet, system);
            if (planet == null)
                return NotFound("Not Found planet");

            if (unitFound.Type == "scout")
                unitInformation.ResourcesQuantity = planet.ResourceQuantity.ToDictionary(r => r.Key.ToString().ToLower(), r => r.Value);

            return unitInformation;
        }
    }
}
