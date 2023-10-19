using Duncan.Model;
using Duncan.Repositories;
using Duncan.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Duncan.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildingsController : ControllerBase
    {
        private readonly UsersRepo _usersRepo;
        private readonly UnitsRepo _unitsRepo;

        public BuildingsController(UnitsRepo unitsRepo, UsersRepo usersRepo)
        {
            this._unitsRepo = unitsRepo;
            this._usersRepo = usersRepo;
        }

        [SwaggerOperation(Summary = "Create a building at a location")]
        [HttpPost("/users/{userId}/buildings")]
        public ActionResult<Building> BuildMine(string userId, [FromBody] BuildingBody building)
        {

            UserWithUnits? userWithUnits = _usersRepo.GetUserWithUnitsByUserId(userId);

            if (userWithUnits == null)
                return NotFound();

            Unit? unitFound = _unitsRepo.GetUnitWithType("builder", userWithUnits);

            if (unitFound == null)
                return NotFound("Not Found unit");

            if (building == null)
                return BadRequest("Building is null");

            if (building.BuilderId == null)
                return BadRequest("Builder'id is null");

            if (unitFound.Id != building.BuilderId)
                return BadRequest("BuilderId is different than the unitfoundId"); 

            if (building.Type != "mine")
                return BadRequest("Type of building is different than mine");

            if(unitFound.DestinationPlanet != unitFound.Planet) 
                return BadRequest("Builder is not over a planet");    

            return Created("", new Building
            {
                Id = building.Id,
                Type = "mine",
                System = unitFound.System,
                Planet = unitFound.Planet,
            });
        }
    }
}