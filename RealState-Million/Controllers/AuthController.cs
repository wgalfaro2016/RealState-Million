using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Application.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg) { _cfg = cfg; }

    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<string> Token([FromBody] LoginDto login) {
        if (login.User != "wilmar" || login.Password != "wilmar") return Unauthorized();

        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.Sub, login.User),
            new Claim("perm", "properties:read"),
            new Claim("perm", "properties:write"),
            new Claim("perm", "properties:price"),
            new Claim("perm", "properties:trace"),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: creds);

        return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    
}
