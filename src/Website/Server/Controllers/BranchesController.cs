using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Server.Controllers
{
    [Authorize(Roles = RoleConstants.AdminAndSeller)]
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly BranchesRepository branchesRepository;
        private readonly ProductsRepository productsRepository;

        public BranchesController(BranchesRepository branchesRepository, ProductsRepository productsRepository)
        {
            this.branchesRepository = branchesRepository;
            this.productsRepository = productsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddBranchAsync([FromBody] MBranch branch)
        {
            if (!await productsRepository.IsProductSellerAsync(branch.ProductId, int.Parse(User.Identity.Name)))
            {
                return BadRequest();
            }

            return Ok(await branchesRepository.AddBranchAsync(branch));
        }

        [HttpPut]
        public async Task<IActionResult> PutBranchAsync([FromBody] MBranch branch)
        {
            if (!await branchesRepository.IsBranchSellerAsync(branch.Id, int.Parse(User.Identity.Name)))
            {
                return BadRequest();
            }

            await branchesRepository.UpdateBranchAsync(branch);
            return Ok();
        }
    }
}
