using Microsoft.AspNetCore.Mvc;
using O24OpenAPI.Framework.Helpers;

namespace O24OpenAPI.Framework.Controllers;

/// <summary>
///
/// </summary>
[ProducesResponseType(typeof(string), 401)]
[ApiController]
[Authorize]
[Produces("application/json", [])]
[Route("api/[controller]/[action]", Order = 2147483647)]
public abstract class BaseController : ControllerBase { }
